using SosyalAjandam.Models;

namespace SosyalAjandam.Services
{
    public interface IAuraService
    {
        Task<AuraAnalysisResult> AnalyzeUserStatusAsync(string userId);
    }

    public class AuraAnalysisResult
    {
        public string Message { get; set; } = "Hazırım!";
        public string VisualState { get; set; } = "aura-normal"; // aura-normal, aura-warning, aura-celebrate
        public string StrategyPlan { get; set; } = "Henüz bir strateji yok.";
    }
}
