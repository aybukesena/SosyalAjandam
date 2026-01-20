using Microsoft.EntityFrameworkCore;
using SosyalAjandam.Data;
using SosyalAjandam.Models;

namespace SosyalAjandam.Services
{
    public class AuraService : IAuraService
    {
        private readonly ApplicationDbContext _context;

        public AuraService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AuraAnalysisResult> AnalyzeUserStatusAsync(string userId)
        {
            var result = new AuraAnalysisResult();
            var today = DateTime.Today;
            
            // Fetch relevant tasks
            var tasks = await _context.TodoItems
                .Where(t => t.OwnerId == userId && !t.IsCompleted)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            var overdueCount = tasks.Count(t => t.DueDate.Date < today);
            var todayCount = tasks.Count(t => t.DueDate.Date == today);
            
            // 1. Determine Visual State & Message
            if (overdueCount > 0)
            {
                result.VisualState = "aura-warning";
                result.Message = $"Dikkat! Tarihi geçmiş {overdueCount} görevin var.";
            }
            else if (todayCount > 3)
            {
                result.VisualState = "aura-focus"; // Maybe a intense blue?
                result.Message = "Bugün yoğun geçecek, odaklanmalısın.";
            }
            else if (tasks.Count == 0)
            {
                result.VisualState = "aura-calm";
                result.Message = "Her şey yolunda, ajandan tertemiz!";
            }
            else
            {
                // Progress Check (if any completed today)
                var completedToday = await _context.TodoItems.CountAsync(t => t.OwnerId == userId && t.IsCompleted && t.CompletedDate >= today);
                if (completedToday > 0)
                {
                    result.Message = $"Bugün {completedToday} görev tamamladın, harika gidiyorsun!";
                }
                else
                {
                     result.Message = "Güne başlamaya hazır mısın?";
                }
            }

            // 2. Generate Strategy
            if (tasks.Any())
            {
                var topTasks = tasks.Take(3).ToList();
                var strategy = "⚡ **Günün Stratejisi**\n\n";
                
                strategy += "1. Önce enerji topla, çünkü en önemli görevin:\n";
                strategy += $"   🔹 **{topTasks[0].Title}** (Tarih: {topTasks[0].DueDate:dd.MM})\n";
                
                if (topTasks.Count > 1) 
                    strategy += $"2. Ardından buna odaklan: **{topTasks[1].Title}**\n";
                
                if (topTasks.Count > 2)
                    strategy += $"3. Son olarak günü bununla bitir: **{topTasks[2].Title}**\n";
                
                if (overdueCount > 0)
                    strategy += "\n⚠️ **Uyarı:** Gecikmiş görevlerin var, bunları temizlemeden yeni iş alma!";
                
                result.StrategyPlan = strategy;
            }
            else
            {
                result.StrategyPlan = "Bugün için yapman gereken acil bir görev yok.\nKendine vakit ayırabilir veya 'Wishlist'ine göz atabilirsin.";
            }

            return result;
        }
    }
}
