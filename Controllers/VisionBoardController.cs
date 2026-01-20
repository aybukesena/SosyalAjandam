using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SosyalAjandam.Data;
using SosyalAjandam.Models;
using System.Security.Claims;

namespace SosyalAjandam.Controllers
{
    [Authorize]
    public class VisionBoardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public VisionBoardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var items = await _context.VisionBoardItems
                .Where(i => i.UserId == user.Id)
                .ToListAsync();

            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] List<VisionBoardItem> items)
        {
             var userId = _userManager.GetUserId(User);
             if (string.IsNullOrEmpty(userId)) return Unauthorized();

             // Wipe and Replace strategy
             var existingItems = await _context.VisionBoardItems.Where(i => i.UserId == userId).ToListAsync();
             _context.VisionBoardItems.RemoveRange(existingItems);
             
             if(items != null && items.Any())
             {
                 foreach(var item in items)
                 {
                     item.Id = 0;
                     item.UserId = userId;
                     _context.VisionBoardItems.Add(item);
                 }
             }

             await _context.SaveChangesAsync();
             return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] VisionBoardItem item)
        {
             var userId = _userManager.GetUserId(User);
             if (string.IsNullOrEmpty(userId)) return Unauthorized();

             item.UserId = userId;
             // Default position if not provided, though frontend should send it.
             if(item.Width == 0) item.Width = 150;
             if(item.Height == 0) item.Height = 150;
             
             _context.VisionBoardItems.Add(item);
             await _context.SaveChangesAsync();
             
             // Return the ID so frontend can update the element
             return Json(new { success = true, id = item.Id });
        }


        
        // Deleted GenerateFallbackStickers method

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var item = await _context.VisionBoardItems.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
            if (item != null)
            {
                _context.VisionBoardItems.Remove(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Item not found" });
        }
    }
}
