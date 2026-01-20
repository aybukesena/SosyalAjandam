using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SosyalAjandam.Data;
using SosyalAjandam.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SosyalAjandam.Controllers
{
    [Authorize]
    public class StatsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StatsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Weekly Stats (Last 7 Days)
            var sevenDaysAgo = DateTime.Today.AddDays(-6);
            var completedTasks = await _context.TodoItems
                .Where(t => t.OwnerId == user.Id && t.IsCompleted && t.CompletedDate >= sevenDaysAgo)
                .ToListAsync();

            var labels = new List<string>();
            var data = new List<int>();

            for (int i = 0; i < 7; i++)
            {
                var date = sevenDaysAgo.AddDays(i);
                labels.Add(date.ToString("ddd", new System.Globalization.CultureInfo("tr-TR"))); // Turkish abbreviated day names
                data.Add(completedTasks.Count(t => t.CompletedDate.HasValue && t.CompletedDate.Value.Date == date));
            }

            ViewBag.ChartLabels = labels;
            ViewBag.ChartData = data;
            
            // Stats Summary
            ViewBag.TotalXP = user.ExperiencePoints;
            ViewBag.CurrentLevel = user.Level;
            ViewBag.CompletedTasksCount = await _context.TodoItems.CountAsync(t => t.OwnerId == user.Id && t.IsCompleted);

            return View();
        }
    }
}
