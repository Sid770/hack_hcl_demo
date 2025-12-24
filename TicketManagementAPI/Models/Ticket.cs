using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TicketManagementAPI.Models
{
    public class Ticket
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [Required]
        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [BsonElement("category")]
        public string Category { get; set; } = string.Empty;

        [Required]
        [BsonElement("priority")]
        public string Priority { get; set; } = string.Empty;

        [Required]
        [BsonElement("status")]
        public string Status { get; set; } = string.Empty;

        [Required]
        [BsonElement("assignedTo")]
        public string AssignedTo { get; set; } = string.Empty;

        [Required]
        [BsonElement("reportedBy")]
        public string ReportedBy { get; set; } = string.Empty;

        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("resolvedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? ResolvedAt { get; set; }

        [BsonElement("comments")]
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
