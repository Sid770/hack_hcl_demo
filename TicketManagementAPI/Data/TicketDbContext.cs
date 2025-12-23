using Microsoft.EntityFrameworkCore;
using TicketManagementAPI.Models;

namespace TicketManagementAPI.Data
{
    public class TicketDbContext : DbContext
    {
        public TicketDbContext(DbContextOptions<TicketDbContext> options) : base(options)
        {
        }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Ticket>()
                .HasMany(t => t.Comments)
                .WithOne(c => c.Ticket)
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var tickets = new[]
            {
                new Ticket
                {
                    Id = "1",
                    Title = "Login page not working",
                    Description = "Users are unable to login to the system. Getting 500 error.",
                    Category = "Bug",
                    Priority = "Critical",
                    Status = "Open",
                    AssignedTo = "John Doe",
                    ReportedBy = "Jane Smith",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Ticket
                {
                    Id = "2",
                    Title = "Add export functionality",
                    Description = "Need ability to export tickets to CSV format.",
                    Category = "Feature Request",
                    Priority = "Medium",
                    Status = "In Progress",
                    AssignedTo = "Mike Johnson",
                    ReportedBy = "Sarah Williams",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Ticket
                {
                    Id = "3",
                    Title = "Billing discrepancy",
                    Description = "Customer charged twice for the same service.",
                    Category = "Billing",
                    Priority = "High",
                    Status = "Resolved",
                    AssignedTo = "Emily Davis",
                    ReportedBy = "Robert Brown",
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    ResolvedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Ticket
                {
                    Id = "4",
                    Title = "How to reset password?",
                    Description = "User needs help resetting their password.",
                    Category = "General",
                    Priority = "Low",
                    Status = "Closed",
                    AssignedTo = "Support Team",
                    ReportedBy = "Alice Cooper",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-4),
                    ResolvedAt = DateTime.UtcNow.AddDays(-4)
                },
                new Ticket
                {
                    Id = "5",
                    Title = "Server performance issues",
                    Description = "API response time is very slow during peak hours.",
                    Category = "Technical",
                    Priority = "High",
                    Status = "On Hold",
                    AssignedTo = "DevOps Team",
                    ReportedBy = "System Monitor",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow
                }
            };

            modelBuilder.Entity<Ticket>().HasData(tickets);

            var comments = new[]
            {
                new Comment
                {
                    Id = "c1",
                    TicketId = "2",
                    Author = "Mike Johnson",
                    Text = "Working on implementation",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Comment
                {
                    Id = "c2",
                    TicketId = "3",
                    Author = "Emily Davis",
                    Text = "Refund processed",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            modelBuilder.Entity<Comment>().HasData(comments);
        }
    }
}
