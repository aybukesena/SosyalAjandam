using System;
using System.Collections.Generic;
using System.Linq;
using SosyalAjandam.Models;
using SosyalAjandam.Models.ViewModels;

namespace SosyalAjandam.Services
{
    public class AuraPlannerService : IAuraPlannerService
    {
        public PlannerResultViewModel GeneratePlan(PlannerInputViewModel input)
        {
            var result = new PlannerResultViewModel();
            var startDate = DateTime.Today;
            
            // Map Day Names
            string[] turkishDays = { "Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi" };

            // Process each input item separately to respect its specific constraints
            foreach (var item in input.Inputs.Where(i => i.Hours > 0 && !string.IsNullOrEmpty(i.Category)))
            {
                // Create units for this specific item
                var itemUnits = new List<PlannerItemInput>();
                for (int i = 0; i < item.Hours; i++)
                {
                    itemUnits.Add(item);
                }

                // Determine valid days for this item
                List<DayOfWeek> validDays = new List<DayOfWeek>();
                if (item.SelectedDays != null && item.SelectedDays.Any())
                {
                   foreach(var d in item.SelectedDays)
                   {
                       if(Enum.TryParse<DayOfWeek>(d, true, out var dow))
                       {
                           validDays.Add(dow);
                       }
                       // Handle Turkish names if necessary, but we'll try to send English DayOfWeek names from frontend
                   }
                }
                
                // If no specific days selected, allow all
                if (!validDays.Any())
                {
                     validDays = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().ToList();
                }

                // Distribution Logic for this Item
                // We want to distribute itemUnits across validDays in the next 7 days.
                
                int totalUnits = itemUnits.Count;
                int unitsAssigned = 0;
                int dayIndex = 0;

                // Simple Round Robin for now, but respecting days
                // We iterate 7 days starting from today
                for (int d = 0; d < 7; d++)
                {
                    if (unitsAssigned >= totalUnits) break;

                    var currentDate = startDate.AddDays(d);
                    if (validDays.Contains(currentDate.DayOfWeek))
                    {
                        // Assign one unit here
                        string dayName = turkishDays[(int)currentDate.DayOfWeek];
                        
                        result.PlannedTasks.Add(new PlannedTaskViewModel
                        {
                            Date = currentDate,
                            Day = dayName,
                            Title = $"{item.Category} Çalışması",
                            Duration = 1,
                            DurationDescription = "1 Saat",
                            Priority = item.Priority
                        });
                        unitsAssigned++;
                    }
                }
                
                // If we have more units than days (e.g. 4 hours on 1 day), we need to double up.
                // The above loop only puts 1 per valid day.
                // Let's improve: Distribute TotalUnits / ValidDaysCount per day roughly.
            }
            
            // Re-sort the result by Date then by Priority
            result.PlannedTasks = result.PlannedTasks
                .OrderBy(t => t.Date)
                .ThenByDescending(t => t.Priority)
                .ToList();

            return result;
        }
    }
}
