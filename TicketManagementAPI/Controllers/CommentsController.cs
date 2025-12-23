using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketManagementAPI.Data;
using TicketManagementAPI.Models;

namespace TicketManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly TicketDbContext _context;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(TicketDbContext context, ILogger<CommentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Comments/ticket/5
        [HttpGet("ticket/{ticketId}")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsByTicket(string ticketId)
        {
            try
            {
                var comments = await _context.Comments
                    .Where(c => c.TicketId == ticketId)
                    .OrderBy(c => c.CreatedAt)
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
                var ticketExists = await _context.Tickets.AnyAsync(t => t.Id == comment.TicketId);
                if (!ticketExists)
                {
                    return NotFound(new { message = "Ticket not found" });
                }

                comment.Id = Guid.NewGuid().ToString();
                comment.CreatedAt = DateTime.UtcNow;

                _context.Comments.Add(comment);

                // Update ticket's UpdatedAt
                var ticket = await _context.Tickets.FindAsync(comment.TicketId);
                if (ticket != null)
                {
                    ticket.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

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
                var comment = await _context.Comments.FindAsync(id);
                if (comment == null)
                {
                    return NotFound(new { message = "Comment not found" });
                }

                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();

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
