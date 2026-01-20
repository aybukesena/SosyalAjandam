using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SosyalAjandam.Data;
using SosyalAjandam.Models;

namespace SosyalAjandam.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SosyalAjandam.Services.IAuraService _auraService;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SosyalAjandam.Services.IAuraService auraService)
    {
        _context = context;
        _userManager = userManager;
        _auraService = auraService;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
             var user = await _userManager.GetUserAsync(User);
             if (user != null)
             {
                 var today = DateTime.Today;
                 
                 // Fetch all Today's tasks first
                 var todaysTasks = await _context.TodoItems
                     .Where(t => t.OwnerId == user.Id && t.DueDate.Date == today)
                     .ToListAsync();

                 // Aura Analysis
                 var auraAnalysis = await _auraService.AnalyzeUserStatusAsync(user.Id);
                 
                 // Pass to Layout
                 ViewBag.AuraMessage = auraAnalysis.Message;
                 ViewBag.AuraState = auraAnalysis.VisualState;

                 // Determine Display Name
                 string displayName = !string.IsNullOrEmpty(user.FullName) ? user.FullName : user.Email.Split('@')[0];

                 var model = new SosyalAjandam.ViewModels.DashboardViewModel
                 {
                     DisplayName = displayName,
                     DailyTotal = todaysTasks.Count,
                     DailyCompleted = todaysTasks.Count(t => t.IsCompleted),
                     CriticalTasks = todaysTasks
                                        .Where(t => !t.IsCompleted)
                                        .OrderBy(t => t.DueDate) // Order by due date? Or ID?
                                        .Take(3)
                                        .ToList(),
                     DailyQuote = "Gelecek, bugün ne yaptığına bağlıdır.", // Placeholder or Random
                     CurrentLevel = user.Level,
                     CurrentXP = user.ExperiencePoints,
                     XPToNextLevel = user.Level * 100, // Formula: Level * 100
                     
                     // Aura Data
                     AuraMessage = auraAnalysis.Message,
                     AuraVisualState = auraAnalysis.VisualState,
                     DailyStrategy = auraAnalysis.StrategyPlan
                 };
                 
                 return View(model);
             }
        }
        return View(new SosyalAjandam.ViewModels.DashboardViewModel());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
