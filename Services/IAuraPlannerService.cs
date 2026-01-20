using SosyalAjandam.Models.ViewModels;

namespace SosyalAjandam.Services
{
    public interface IAuraPlannerService
    {
        PlannerResultViewModel GeneratePlan(PlannerInputViewModel input);
    }
}
