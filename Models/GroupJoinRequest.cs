using System;

namespace SosyalAjandam.Models
{
    public class GroupJoinRequest
    {
        public int Id { get; set; }

        public int GroupId { get; set; }
        public Group? Group { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.Now;

        // Pending, Approved, Rejected
        public string Status { get; set; } = "Pending";
    }
}
