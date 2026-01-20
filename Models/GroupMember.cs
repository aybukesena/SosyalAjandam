namespace SosyalAjandam.Models
{
    public class GroupMember
    {
        public int Id { get; set; } // PK for join table if needed, or composite key
        public int GroupId { get; set; }
        public Group? Group { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public bool IsLeader { get; set; }
    }
}
