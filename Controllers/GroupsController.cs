using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SosyalAjandam.Data;
using SosyalAjandam.Models;

namespace SosyalAjandam.Controllers
{
    [Authorize]
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GroupsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var myGroups = await _context.GroupMembers
                .Where(gm => gm.UserId == user.Id)
                .Include(gm => gm.Group)
                .ThenInclude(g => g.Members)
                .ThenInclude(m => m.User)
                .Include(gm => gm.Group)
                .ThenInclude(g => g.JoinRequests)
                .Select(gm => gm.Group)
                .ToListAsync();

            var myGroupIds = myGroups.Select(g => g.Id).ToList();

            var discoverQuery = _context.Groups
                .Where(g => !g.IsPrivate && !myGroupIds.Contains(g.Id))
                .Include(g => g.Members)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                discoverQuery = discoverQuery.Where(g => g.Name.Contains(searchString) || g.Description.Contains(searchString));
            }

            var discoverGroups = await discoverQuery.ToListAsync();

            var model = new SosyalAjandam.ViewModels.GroupIndexViewModel
            {
                MyGroups = myGroups,
                DiscoverGroups = discoverGroups,
                SearchString = searchString
            };

            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,IsPrivate")] Group group)
        {
            var user = await _userManager.GetUserAsync(User);
            if (ModelState.IsValid)
            {
                group.InviteCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                group.LeaderId = user.Id;
                
                _context.Add(group);
                await _context.SaveChangesAsync();

                // Add creator as leader member
                var member = new GroupMember
                {
                    GroupId = group.Id,
                    UserId = user.Id,
                    IsLeader = true
                };
                _context.Add(member);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(group);
        }
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var group = await _context.Groups
                .Include(g => g.Members)
                .ThenInclude(m => m.User)
                .Include(g => g.SharedTasks)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null) return NotFound();

            // Check membership
            if (!group.Members.Any(m => m.UserId == user.Id)) return Forbid();

            // Calculate Stats
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday); // Assuming Mon start
            
            var memberStats = new List<SosyalAjandam.ViewModels.GroupMemberStat>();
            
            // Note: In a real app complexity, I'd query this more efficiently or via a separate Stats table
            // For now, iterate memebers
            foreach (var member in group.Members)
            {
                 // Count completed tasks assigned to this user in this group since start of week
                 int weeklyCompleted = await _context.TodoItems.CountAsync(t => 
                     t.GroupId == group.Id && 
                     t.AssignedToUserId == member.UserId && 
                     t.IsCompleted && 
                     t.CompletedDate >= startOfWeek);
                 
                 int weeklyTotal = await _context.TodoItems.CountAsync(t => 
                     t.GroupId == group.Id && 
                     t.AssignedToUserId == member.UserId && 
                     (t.DueDate >= startOfWeek || t.IsCompleted)); // Crude approx for 'active this week' or just all assigned
                 
                 // Better metric: All tasks assigned to user in this group that are NOT completed OR completed this week
                 // Or just simpler: Total tasks assigned to user in this group ever? Request says "Weekly Success Ranking"
                 // Let's stick to: Tasks Completed This Week vs Total Tasks Assigned that were Due This Week (or completed this week)
                 
                 // Revised Total: Tasks Due This Week Assigned To User
                 weeklyTotal = await _context.TodoItems.CountAsync(t => 
                     t.GroupId == group.Id && 
                     t.AssignedToUserId == member.UserId && 
                     t.DueDate >= startOfWeek && t.DueDate <= DateTime.Today.AddDays(7));

                 memberStats.Add(new SosyalAjandam.ViewModels.GroupMemberStat
                 {
                     UserId = member.UserId,
                     UserName = member.User.UserName,
                     WeeklyCompleted = weeklyCompleted,
                     WeeklyTotalAssigned = weeklyTotal,
                     TotalXp = member.User.ExperiencePoints
                 });
            }

            // Rank by Efficiency Percent first, then Count
            memberStats = memberStats.OrderByDescending(ms => ms.EfficiencyPercent).ThenByDescending(ms => ms.WeeklyCompleted).ToList();
            for(int i=0; i<memberStats.Count; i++) memberStats[i].Rank = i + 1;

            // Overall Group Progress
            int totalGroupTasks = group.SharedTasks.Count;
            int completedGroupTasks = group.SharedTasks.Count(t => t.IsCompleted);
            int overallProgress = totalGroupTasks > 0 ? (int)((double)completedGroupTasks / totalGroupTasks * 100) : 0;

            var pendingRequests = new List<GroupJoinRequest>();
            if (group.LeaderId == user.Id)
            {
                pendingRequests = await _context.GroupJoinRequests
                    .Include(r => r.User)
                    .Where(r => r.GroupId == id && r.Status == "Pending")
                    .ToListAsync();
            }

            var model = new SosyalAjandam.ViewModels.GroupDetailsViewModel
            {
                Group = group,
                IsLeader = group.LeaderId == user.Id,
                OverallProgress = overallProgress,
                MemberStats = memberStats,
                ActiveTasks = group.SharedTasks.Where(t => !t.IsCompleted).OrderBy(t => t.DueDate).ToList(),
                PendingRequests = pendingRequests
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddMember(int groupId, string email)
        {
             var user = await _userManager.GetUserAsync(User);
             var group = await _context.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == groupId);
             
             if(group != null && group.LeaderId == user.Id && !string.IsNullOrWhiteSpace(email))
             {
                 var targetUser = await _userManager.FindByEmailAsync(email);
                 if(targetUser != null)
                 {
                     if(!group.Members.Any(m => m.UserId == targetUser.Id))
                     {
                         var member = new GroupMember { GroupId = groupId, UserId = targetUser.Id, IsLeader = false };
                         _context.Add(member);
                         await _context.SaveChangesAsync();
                         TempData["Success"] = "Kullanıcı eklendi.";
                     }
                     else
                     {
                          TempData["Error"] = "Kullanıcı zaten grupta.";
                     }
                 }
                 else
                 {
                     TempData["Error"] = "Kullanıcı bulunamadı.";
                 }
             }
             return RedirectToAction(nameof(Details), new { id = groupId });
        }

        [HttpPost]
        public async Task<IActionResult> AssignTask(int groupId, string title, DateTime dueDate, string assignedUserId)
        {
            var user = await _userManager.GetUserAsync(User);
            var group = await _context.Groups.FindAsync(groupId);
            
            // Only Leader can assign
            if (group != null && group.LeaderId == user.Id)
            {
                 var task = new TodoItem
                 {
                     Title = title,
                     DueDate = dueDate,
                     GroupId = groupId,
                     AssignedToUserId = assignedUserId,
                     OwnerId = user.Id, // Creator is owner
                     IsCompleted = false
                 };
                 _context.Add(task);
                 await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id = groupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendJoinRequest(int groupId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null) return NotFound();

            if (await _context.GroupMembers.AnyAsync(gm => gm.GroupId == groupId && gm.UserId == user.Id))
            {
                TempData["Error"] = "Already a member.";
                return RedirectToAction(nameof(Index));
            }

            if (await _context.GroupJoinRequests.AnyAsync(r => r.GroupId == groupId && r.UserId == user.Id && r.Status == "Pending"))
            {
                TempData["Error"] = "Request already pending.";
                return RedirectToAction(nameof(Index));
            }

            var request = new GroupJoinRequest
            {
                GroupId = groupId,
                UserId = user.Id,
                Status = "Pending",
                RequestDate = DateTime.Now
            };
            _context.Add(request);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Join request sent.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveGroup(int groupId)
        {
             var user = await _userManager.GetUserAsync(User);
             var member = await _context.GroupMembers
                 .Include(gm => gm.Group)
                 .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == user.Id);
             
             if (member != null)
             {
                 if (member.IsLeader)
                 {
                     _context.Groups.Remove(member.Group); // Delete group if leader leaves
                     TempData["Success"] = "Group deleted because leader left.";
                 }
                 else
                 {
                     _context.GroupMembers.Remove(member);
                     TempData["Success"] = "You have left the group.";
                 }
                 await _context.SaveChangesAsync();
             }
             return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRequest(int requestId)
        {
            var user = await _userManager.GetUserAsync(User);
            var request = await _context.GroupJoinRequests
                .Include(r => r.Group)
                .FirstOrDefaultAsync(r => r.Id == requestId);
            
            if (request != null && request.Group.LeaderId == user.Id)
            {
                request.Status = "Approved";
                
                var member = new GroupMember
                {
                    GroupId = request.GroupId,
                    UserId = request.UserId,
                    IsLeader = false
                };
                _context.Add(member);
                _context.GroupJoinRequests.Remove(request); // Delete request after approval
                await _context.SaveChangesAsync();
                TempData["Success"] = "Request approved.";
            }
            return RedirectToAction(nameof(Details), new { id = request?.GroupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRequest(int requestId)
        {
             var user = await _userManager.GetUserAsync(User);
             var request = await _context.GroupJoinRequests
                .Include(r => r.Group)
                .FirstOrDefaultAsync(r => r.Id == requestId);

             if (request != null && request.Group.LeaderId == user.Id)
             {
                 _context.GroupJoinRequests.Remove(request);
                 await _context.SaveChangesAsync();
                 TempData["Success"] = "Request rejected.";
             }
             return RedirectToAction(nameof(Details), new { id = request?.GroupId });
        }
    }
}
