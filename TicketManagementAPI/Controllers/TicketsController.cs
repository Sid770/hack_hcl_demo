using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using TicketManagementAPI.Data;
using TicketManagementAPI.Models;

namespace TicketManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(MongoDbService mongoDbService, ILogger<TicketsController> logger)
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
        }

        // GET: api/Tickets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets()
        {
            try
            {
                var tickets = await _mongoDbService.Tickets
                    .Find(_ => true)
                    .SortByDescending(t => t.CreatedAt)
                    .ToListAsync();
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tickets");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Tickets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Ticket>> GetTicket(string id)
        {
            try
            {
                var ticket = await _mongoDbService.Tickets
                    .Find(t => t.Id == id)
                    .FirstOrDefaultAsync();

                if (ticket == null)
                {
                    return NotFound(new { message = "Ticket not found" });
                }

                return Ok(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ticket {TicketId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Tickets/stats
        [HttpGet("stats")]
        public async Task<ActionResult<TicketStats>> GetTicketStats()
        {
            try
            {
                var tickets = await _mongoDbService.Tickets.Find(_ => true).ToListAsync();

                var stats = new TicketStats
                {
                    Total = tickets.Count,
                    Open = tickets.Count(t => t.Status == "Open"),
                    InProgress = tickets.Count(t => t.Status == "In Progress"),
                    Resolved = tickets.Count(t => t.Status == "Resolved"),
                    Closed = tickets.Count(t => t.Status == "Closed"),
                    HighPriority = tickets.Count(t => t.Priority == "High" || t.Priority == "Critical")
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating ticket stats");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Tickets/search?term=login
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Ticket>>> SearchTickets([FromQuery] string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                {
                    return await GetTickets();
                }

                var filter = Builders<Ticket>.Filter.Or(
                    Builders<Ticket>.Filter.Regex(t => t.Title, new MongoDB.Bson.BsonRegularExpression(term, "i")),
                    Builders<Ticket>.Filter.Regex(t => t.Description, new MongoDB.Bson.BsonRegularExpression(term, "i")),
                    Builders<Ticket>.Filter.Regex(t => t.AssignedTo, new MongoDB.Bson.BsonRegularExpression(term, "i")),
                    Builders<Ticket>.Filter.Regex(t => t.ReportedBy, new MongoDB.Bson.BsonRegularExpression(term, "i"))
                );

                var tickets = await _mongoDbService.Tickets
                    .Find(filter)
                    .SortByDescending(t => t.CreatedAt)
                    .ToListAsync();

                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tickets");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Tickets
        [HttpPost]
        public async Task<ActionResult<Ticket>> CreateTicket(Ticket ticket)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                ticket.CreatedAt = DateTime.UtcNow;
                ticket.UpdatedAt = DateTime.UtcNow;
                ticket.Comments = new List<Comment>();

                await _mongoDbService.Tickets.InsertOneAsync(ticket);

                return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/Tickets/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicket(string id, Ticket ticket)
        {
            try
            {
                if (id != ticket.Id)
                {
                    return BadRequest(new { message = "ID mismatch" });
                }

                var existingTicket = await _mongoDbService.Tickets
                    .Find(t => t.Id == id)
                    .FirstOrDefaultAsync();
                    
                if (existingTicket == null)
                {
                    return NotFound(new { message = "Ticket not found" });
                }

                ticket.UpdatedAt = DateTime.UtcNow;
                ticket.CreatedAt = existingTicket.CreatedAt;

                if (ticket.Status == "Resolved" && existingTicket.ResolvedAt == null)
                {
                    ticket.ResolvedAt = DateTime.UtcNow;
                }
                else if (ticket.Status != "Resolved")
                {
                    ticket.ResolvedAt = existingTicket.ResolvedAt;
                }

                await _mongoDbService.Tickets.ReplaceOneAsync(t => t.Id == id, ticket);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket {TicketId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/Tickets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(string id)
        {
            try
            {
                var result = await _mongoDbService.Tickets.DeleteOneAsync(t => t.Id == id);
                
                if (result.DeletedCount == 0)
                {
                    return NotFound(new { message = "Ticket not found" });
                }

                // Also delete associated comments
                await _mongoDbService.Comments.DeleteManyAsync(c => c.TicketId == id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ticket {TicketId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
