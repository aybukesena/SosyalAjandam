using System;
using System.Collections.Generic;

namespace SosyalAjandam.Models.ViewModels
{
    public class PlannerItemInput
    {
        public string Category { get; set; } = string.Empty;
        public int Hours { get; set; }
        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
        public List<string> SelectedDays { get; set; } = new List<string>(); // "Monday", "Wednesday", etc.
    }

    public class PlannerInputViewModel
    {
        // We can accept a list of inputs. 
        // For the UI, we might bind to a list.
        public List<PlannerItemInput> Inputs { get; set; } = new List<PlannerItemInput>();
    }

    public class PlannedTaskViewModel
    {
        public string Day { get; set; } = string.Empty; // "Pazartesi", etc.
        public DateTime Date { get; set; }
        public string Title { get; set; } = string.Empty; // e.g., "Matematik Çalışması (1 Sa)"
        public int Duration { get; set; } // in hours
        public string DurationDescription { get; set; } = string.Empty; // "1 Saat"
        public PriorityLevel Priority { get; set; }
    }

    public class PlannerResultViewModel
    {
        public List<PlannedTaskViewModel> PlannedTasks { get; set; } = new List<PlannedTaskViewModel>();
    }
}
