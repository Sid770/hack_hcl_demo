using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketManagementAPI.Data;
using TicketManagementAPI.Models;

namespace TicketManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly TicketDbContext _context;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(TicketDbContext context, ILogger<TicketsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Tickets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets()
        {
            try
            {
                var tickets = await _context.Tickets
                    .Include(t => t.Comments)
                    .OrderByDescending(t => t.CreatedAt)
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
                var ticket = await _context.Tickets
                    .Include(t => t.Comments)
                    .FirstOrDefaultAsync(t => t.Id == id);

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
                var tickets = await _context.Tickets.ToListAsync();

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

                var tickets = await _context.Tickets
                    .Include(t => t.Comments)
                    .Where(t => t.Title.Contains(term) ||
                               t.Description.Contains(term) ||
                               t.AssignedTo.Contains(term) ||
                               t.ReportedBy.Contains(term))
                    .OrderByDescending(t => t.CreatedAt)
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

                ticket.Id = Guid.NewGuid().ToString();
                ticket.CreatedAt = DateTime.UtcNow;
                ticket.UpdatedAt = DateTime.UtcNow;
                ticket.Comments = new List<Comment>();

                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

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

                var existingTicket = await _context.Tickets.FindAsync(id);
                if (existingTicket == null)
                {
                    return NotFound(new { message = "Ticket not found" });
                }

                existingTicket.Title = ticket.Title;
                existingTicket.Description = ticket.Description;
                existingTicket.Category = ticket.Category;
                existingTicket.Priority = ticket.Priority;
                existingTicket.Status = ticket.Status;
                existingTicket.AssignedTo = ticket.AssignedTo;
                existingTicket.ReportedBy = ticket.ReportedBy;
                existingTicket.UpdatedAt = DateTime.UtcNow;

                if (ticket.Status == "Resolved" && existingTicket.ResolvedAt == null)
                {
                    existingTicket.ResolvedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

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
                var ticket = await _context.Tickets.FindAsync(id);
                if (ticket == null)
                {
                    return NotFound(new { message = "Ticket not found" });
                }

                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();

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
