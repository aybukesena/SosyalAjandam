using System;

namespace SosyalAjandam.Models
{
    public class TodoItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }

        public string OwnerId { get; set; } = string.Empty;
        public ApplicationUser? Owner { get; set; }

        public int? GroupId { get; set; }
        public Group? Group { get; set; }

        public string? AssignedToUserId { get; set; }
        public ApplicationUser? AssignedToUser { get; set; }

        // New Fields for Aura Planner
        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
        public string EstimatedDuration { get; set; } = string.Empty;
    }

    public enum PriorityLevel
    {
        Low,
        Medium,
        High
    }
}
