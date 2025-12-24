using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using TicketManagementAPI.Data;
using TicketManagementAPI.Models;

namespace TicketManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(MongoDbService mongoDbService, ILogger<CommentsController> logger)
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
        }

        // GET: api/Comments/ticket/5
        [HttpGet("ticket/{ticketId}")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsByTicket(string ticketId)
        {
            try
            {
                // Get comments from the Comments collection
                var comments = await _mongoDbService.Comments
                    .Find(c => c.TicketId == ticketId)
                    .SortBy(c => c.CreatedAt)
                    .ToListAsync();

                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching comments for ticket {TicketId}", ticketId);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Comments
        [HttpPost]
        public async Task<ActionResult<Comment>> AddComment(Comment comment)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verify ticket exists
                var ticket = await _mongoDbService.Tickets
                    .Find(t => t.Id == comment.TicketId)
                    .FirstOrDefaultAsync();
                    
                if (ticket == null)
                {
                    return NotFound(new { message = "Ticket not found" });
                }

                comment.CreatedAt = DateTime.UtcNow;

                // Insert comment into Comments collection
                await _mongoDbService.Comments.InsertOneAsync(comment);

                // Also add comment to ticket's Comments array
                ticket.Comments.Add(comment);
                ticket.UpdatedAt = DateTime.UtcNow;
                await _mongoDbService.Tickets.ReplaceOneAsync(t => t.Id == ticket.Id, ticket);

                return CreatedAtAction(nameof(GetCommentsByTicket), 
                    new { ticketId = comment.TicketId }, comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(string id)
        {
            try
            {
                var comment = await _mongoDbService.Comments
                    .Find(c => c.Id == id)
                    .FirstOrDefaultAsync();
                    
                if (comment == null)
                {
                    return NotFound(new { message = "Comment not found" });
                }

                // Delete from Comments collection
                await _mongoDbService.Comments.DeleteOneAsync(c => c.Id == id);

                // Also remove from ticket's Comments array
                var ticket = await _mongoDbService.Tickets
                    .Find(t => t.Id == comment.TicketId)
                    .FirstOrDefaultAsync();
                    
                if (ticket != null)
                {
                    ticket.Comments.RemoveAll(c => c.Id == id);
                    await _mongoDbService.Tickets.ReplaceOneAsync(t => t.Id == ticket.Id, ticket);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
