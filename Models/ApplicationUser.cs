using Microsoft.AspNetCore.Identity;

namespace SosyalAjandam.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Nickname { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty; // New Display Name
        public int Level { get; set; } = 1;
        public int ExperiencePoints { get; set; } = 0;
        
        // Navigation properties if needed later
        public virtual ICollection<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
        public virtual ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
        public virtual ICollection<UserBadge> Badges { get; set; } = new List<UserBadge>();
    }
}
