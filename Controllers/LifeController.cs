using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SosyalAjandam.Data;
using SosyalAjandam.Models;

namespace SosyalAjandam.Controllers
{
    [Authorize]
    public class LifeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LifeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(ModuleType type)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var items = await _context.LifeItems
                .Where(l => l.OwnerId == user.Id && l.Type == type)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            ViewBag.ModuleType = type;
            ViewBag.PageTitle = GetTitle(type);
            ViewBag.Icon = GetIcon(type);

            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string title, string details, ModuleType type)
        {
             var user = await _userManager.GetUserAsync(User);
             if (user != null && !string.IsNullOrWhiteSpace(title))
             {
                 var item = new LifeItem
                 {
                     OwnerId = user.Id,
                     Title = title,
                     Details = details,
                     Type = type,
                     IsCompleted = false
                 };
                 _context.Add(item);
                 await _context.SaveChangesAsync();
             }
             return RedirectToAction(nameof(Index), new { type = type });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, ModuleType type)
        {
            var user = await _userManager.GetUserAsync(User);
            var item = await _context.LifeItems.FirstOrDefaultAsync(l => l.Id == id && l.OwnerId == user.Id);
            if (item != null)
            {
                item.IsCompleted = !item.IsCompleted;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { type = type });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, ModuleType type)
        {
            var user = await _userManager.GetUserAsync(User);
            var item = await _context.LifeItems.FirstOrDefaultAsync(l => l.Id == id && l.OwnerId == user.Id);
            if (item != null)
            {
                _context.LifeItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { type = type });
        }

        private string GetTitle(ModuleType type)
        {
            return type switch
            {
                ModuleType.Wishlist => "İstek Listem",
                ModuleType.VisionBoard => "Vizyon Panosu",
                ModuleType.Movie => "İzlenecek Filmler",
                ModuleType.Book => "Okunacak Kitaplar",
                _ => "Modül"
            };
        }

        private string GetIcon(ModuleType type)
        {
             return type switch
            {
                ModuleType.Wishlist => "bi-gift",
                ModuleType.VisionBoard => "bi-stars",
                ModuleType.Movie => "bi-film",
                ModuleType.Book => "bi-book",
                _ => "bi-box"
            };
        }
    }
}
