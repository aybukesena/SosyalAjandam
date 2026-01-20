using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SosyalAjandam.Data;
using SosyalAjandam.Models;
using System.Globalization;

namespace SosyalAjandam.Controllers
{
    [Authorize]
    public class AgendaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AgendaController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? month, int? year)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // 1. Determine Date Range
            DateTime now = DateTime.Today;
            int targetMonth = month ?? now.Month;
            int targetYear = year ?? now.Year;
            
            // Validation
            if (targetMonth < 1) { targetMonth = 12; targetYear--; }
            if (targetMonth > 12) { targetMonth = 1; targetYear++; }

            DateTime targetDate = new DateTime(targetYear, targetMonth, 1);
            
            // ViewBags for Navigation
            ViewBag.CurrentMonthName = CultureInfo.GetCultureInfo("tr-TR").DateTimeFormat.GetMonthName(targetMonth);
            ViewBag.CurrentYear = targetYear;
            ViewBag.CurrentMonth = targetMonth; // For logic
            
            DateTime prev = targetDate.AddMonths(-1);
            DateTime next = targetDate.AddMonths(1);
            
            ViewBag.PrevMonth = prev.Month;
            ViewBag.PrevYear = prev.Year;
            ViewBag.NextMonth = next.Month;
            ViewBag.NextYear = next.Year;


            // 2. Fetch Tasks for target Month/Year
            var allTasks = await _context.TodoItems
                .Where(t => t.OwnerId == user.Id || (t.AssignedToUserId != null && t.AssignedToUserId == user.Id))
                .Where(t => t.DueDate.Month == targetMonth && t.DueDate.Year == targetYear)
                .ToListAsync();

            // 3. Sorting Logic
            // Group By Date
            var groupedTasks = allTasks.GroupBy(t => t.DueDate.Date)
                .Select(g => new 
                { 
                    Date = g.Key, 
                    Tasks = g.ToList(),
                    IsAllCompleted = g.All(t => t.IsCompleted) && g.Any() 
                })
                .ToList();

            List<DateTime> sortedDates;

            bool isCurrentMonth = (targetMonth == now.Month && targetYear == now.Year);
            bool isPastMonth = (targetDate < new DateTime(now.Year, now.Month, 1));
            
            if (isCurrentMonth)
            {
                // New Logic as per Request:
                // 1. Future/Today (Top) vs Past (Bottom) -> g.Date < now
                // 2. Future Group: Active First, Completed Last -> (g.Date >= now && g.IsAllCompleted)
                // 3. Past Group: Strict Date Order (Completion doesn't affect position)
                
                sortedDates = groupedTasks
                    .OrderBy(g => g.Date < now) // False (Future/Today) returns 0, True (Past) returns 1. So Future is Top.
                    .ThenBy(g => (g.Date >= now && g.IsAllCompleted) ? 1 : 0) // Move Completed Future tasks to bottom of Future section.
                    .ThenBy(g => g.Date) // Chronological within groups
                    .Select(g => g.Date)
                    .ToList();
            }
            else
            {
                // Past/Future Months: Strict Chronological
                sortedDates = groupedTasks.OrderBy(g => g.Date).Select(g => g.Date).ToList();
            }

            // Ensure we have at least Today if we are in Current Date and it has no tasks?
            // Existing logic had: Union(new [] { today })...
            // Let's pass the date list via ViewBag to let View iterate precisely.
            ViewBag.SortedDates = sortedDates;

            // Calculate Stats (Global or Monthly? Monthly makes sense for the view)
            ViewBag.DailyTotal = allTasks.Count;
            ViewBag.DailyCompleted = allTasks.Count(t => t.IsCompleted);

            return View(allTasks);
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Title,Description,DueDate,Priority,EstimatedDuration")] TodoItem todo)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (ModelState.IsValid)
            {
                todo.OwnerId = user.Id;
                todo.IsCompleted = false;
                _context.Add(todo);
                await _context.SaveChangesAsync();
                
                // Redirect to the month of the created task
                return RedirectToAction(nameof(Index), new { month = todo.DueDate.Month, year = todo.DueDate.Year });
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Edit([Bind("Id,Title,Description,DueDate,Priority,EstimatedDuration")] TodoItem todo)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var existing = await _context.TodoItems.FirstOrDefaultAsync(t => t.Id == todo.Id && t.OwnerId == user.Id);
            if (existing != null)
            {
                existing.Title = todo.Title;
                existing.Description = todo.Description ?? "";
                existing.DueDate = todo.DueDate;
                existing.Priority = todo.Priority;
                existing.EstimatedDuration = todo.EstimatedDuration ?? "";

                await _context.SaveChangesAsync();
                
                // Redirect to that month
                return RedirectToAction(nameof(Index), new { month = todo.DueDate.Month, year = todo.DueDate.Year });
            }
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var todo = await _context.TodoItems.FirstOrDefaultAsync(t => t.Id == id && (t.OwnerId == user.Id || (t.AssignedToUserId != null && t.AssignedToUserId == user.Id)));
            
            if (todo != null)
            {
                todo.IsCompleted = !todo.IsCompleted;
                todo.CompletedDate = todo.IsCompleted ? DateTime.Now : null;
                
                // Add XP (simplified for brevity, keep existing logic if robust)
                if(todo.IsCompleted)
                {
                    var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
                    if(currentUser != null)
                    {
                         currentUser.ExperiencePoints += 10;
                         // ... Level logic ...
                         if ((currentUser.ExperiencePoints / 100) + 1 > currentUser.Level) currentUser.Level++;
                    }
                }
                
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { month = todo.DueDate.Month, year = todo.DueDate.Year });
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
             var user = await _userManager.GetUserAsync(User);
             if (user == null) return Challenge();

            var todo = await _context.TodoItems.FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == user.Id);
            if (todo != null)
            {
                int m = todo.DueDate.Month;
                int y = todo.DueDate.Year;
                _context.TodoItems.Remove(todo);
                await _context.SaveChangesAsync();
                 return RedirectToAction(nameof(Index), new { month = m, year = y });
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
