using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Controllers
{
    public class ReliefProjectController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReliefProjectController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Public — anyone can see ongoing relief projects and their latest updates
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var projects = await _context.ReliefProjects
                .Include(p => p.Updates!.OrderByDescending(u => u.PostedAt).Take(3))
                .Where(p => p.Status == ProjectStatus.Active)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();

            return View(projects);
        }

        // "Employee: can log in, post updates on ongoing relief projects"
        [Authorize(Roles = "Employee")]
        [HttpGet]
        public async Task<IActionResult> PostUpdate(int projectId)
        {
            var project = await _context.ReliefProjects.FindAsync(projectId);
            if (project is null) return NotFound();

            ViewBag.Project = project;
            return View(new ReliefUpdate { ProjectId = projectId });
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostUpdate([Bind("ProjectId,Content")] ReliefUpdate update)
        {
            var project = await _context.ReliefProjects.FindAsync(update.ProjectId);
            if (project is null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Project = project;
                return View(update);
            }

            var user = await _userManager.GetUserAsync(User);
            update.PostedByUserId = user!.Id;
            update.PostedAt = DateTime.UtcNow;

            _context.ReliefUpdates.Add(update);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}