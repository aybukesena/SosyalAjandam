using System.Collections.Generic;

namespace SosyalAjandam.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string InviteCode { get; set; } = string.Empty;
        public bool IsPrivate { get; set; } = false;

        public string LeaderId { get; set; } = string.Empty;
        public ApplicationUser? Leader { get; set; }

        public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
        public virtual ICollection<TodoItem> SharedTasks { get; set; } = new List<TodoItem>();
        public virtual ICollection<GroupJoinRequest> JoinRequests { get; set; } = new List<GroupJoinRequest>();
    }
}
