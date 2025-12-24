using MongoDB.Driver;
using TicketManagementAPI.Models;

namespace TicketManagementAPI.Data
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoDbConnection");
            var databaseName = configuration["MongoDbSettings:DatabaseName"];
            
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<Ticket> Tickets => _database.GetCollection<Ticket>("Tickets");
        public IMongoCollection<Comment> Comments => _database.GetCollection<Comment>("Comments");

        public async Task SeedDataAsync()
        {
            // Check if data already exists
            var ticketCount = await Tickets.CountDocumentsAsync(FilterDefinition<Ticket>.Empty);
            if (ticketCount > 0)
            {
                return; // Data already seeded
            }

            var tickets = new[]
            {
                new Ticket
                {
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
                    Title = "Server performance issues",
                    Description = "API response time is very slow during peak hours.",
                    Category = "Technical",
                    Priority = "High",
                    Status = "On Hold",
                    AssignedTo = "DevOps Team",
                    ReportedBy = "System Monitor",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow
                },
                new Ticket
                {
                    Title = "Improve dashboard UI",
                    Description = "Dashboard layout needs to be more intuitive and user-friendly.",
                    Category = "Enhancement",
                    Priority = "Medium",
                    Status = "Open",
                    AssignedTo = "UI Team",
                    ReportedBy = "Product Manager",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            await Tickets.InsertManyAsync(tickets);
        }
    }
}
