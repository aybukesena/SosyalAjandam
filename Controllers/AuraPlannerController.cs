using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SosyalAjandam.Models;
using SosyalAjandam.Models.ViewModels;
using SosyalAjandam.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SosyalAjandam.Controllers
{
    [Authorize]
    public class AuraPlannerController : Controller
    {
        private readonly IAuraPlannerService _plannerService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SosyalAjandam.Data.ApplicationDbContext _context;

        public AuraPlannerController(IAuraPlannerService plannerService, UserManager<ApplicationUser> userManager, SosyalAjandam.Data.ApplicationDbContext context)
        {
            _plannerService = plannerService;
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            // Initial view with empty input
            return View(new PlannerInputViewModel());
        }

        [HttpPost]
        public IActionResult Generate([FromBody] PlannerInputViewModel input)
        {
            if (input == null || input.Inputs == null || !input.Inputs.Any())
            {
                return BadRequest("Geçerli bir giriş yapılmadı.");
            }

            var plan = _plannerService.GeneratePlan(input);
            return PartialView("_PlanResultPartial", plan);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm([FromBody] PlannerResultViewModel plan)
        {
            if (plan == null || plan.PlannedTasks == null)
            {
                return BadRequest("Plan bulunamadı.");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            foreach (var item in plan.PlannedTasks)
            {
                var todoItem = new TodoItem
                {
                    Title = item.Title,
                    Description = $"Aura Akıllı Planlayıcı tarafından oluşturuldu. ({item.DurationDescription})",
                    DueDate = item.Date,
                    IsCompleted = false,
                    OwnerId = user.Id,
                    Priority = item.Priority,
                    EstimatedDuration = item.DurationDescription
                };
                _context.TodoItems.Add(todoItem);
            }

            await _context.SaveChangesAsync();
            
            // Set success message for the redirection
            TempData["AuraMessage"] = "Planın harika görünüyor! Görevlerini Ajandana kaydettim.";
            return Json(new { success = true, redirectUrl = Url.Action("Index", "Agenda") });
        }
    }
}
