using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SosyalAjandam.Models
{
    public enum BadgeType
    {
        FirstTask,
        GroupLeader,
        EarlyBird
    }

    public class UserBadge
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public BadgeType Badge { get; set; }

        public DateTime EarnedDate { get; set; } = DateTime.Now;
    }
}
