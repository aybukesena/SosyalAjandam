using SosyalAjandam.Models;

namespace SosyalAjandam.ViewModels
{
    public class DashboardViewModel
    {
        public string DisplayName { get; set; } // Display Name
        public int DailyTotal { get; set; }
        public int DailyCompleted { get; set; }
        public int CompletionRate => DailyTotal > 0 ? (int)((double)DailyCompleted / DailyTotal * 100) : 0;
        public List<TodoItem> CriticalTasks { get; set; } = new();
        public string DailyQuote { get; set; } = string.Empty;
        public int CurrentLevel { get; set; }
        public int CurrentXP { get; set; }
        public int XPToNextLevel { get; set; }
        public int XPProgress => XPToNextLevel > 0 ? (int)((double)(CurrentXP % 100) / 100 * 100) : 0;
        
        // Aura Intelligence
        public string AuraMessage { get; set; }
        public string AuraVisualState { get; set; }
        public string DailyStrategy { get; set; }
    }
}
