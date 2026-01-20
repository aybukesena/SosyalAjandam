using SosyalAjandam.Models;

namespace SosyalAjandam.ViewModels
{
    public class GroupDetailsViewModel
    {
        public Group Group { get; set; }
        public bool IsLeader { get; set; }
        public int OverallProgress { get; set; }
        public List<GroupMemberStat> MemberStats { get; set; } = new List<GroupMemberStat>();
        public List<TodoItem> ActiveTasks { get; set; } = new List<TodoItem>();
        public List<GroupJoinRequest> PendingRequests { get; set; } = new List<GroupJoinRequest>();
    }

    public class GroupMemberStat
    {
        public string UserName { get; set; }
        public string UserId { get; set; }
        public int WeeklyCompleted { get; set; }
        public int WeeklyTotalAssigned { get; set; }
        public int EfficiencyPercent => WeeklyTotalAssigned > 0 ? (int)((double)WeeklyCompleted / WeeklyTotalAssigned * 100) : 0;
        public int Rank { get; set; }
        public int TotalXp { get; set; } // Optional: Show total XP
    }
}
