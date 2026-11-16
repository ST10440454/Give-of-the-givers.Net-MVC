using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using GiftOfTheGivers.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Controllers
{
    public class VolunteerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VolunteerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Create() => View(new VolunteerRegisterViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VolunteerRegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var volunteer = new Volunteer
            {
                FullName = model.FullName,
                Skills = model.Skills,
                Availability = model.Availability,
                Status = VolunteerStatus.Pending
            };

            if (User.Identity!.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user is not null)
                {
                    if (await _context.Volunteers.AnyAsync(v => v.UserId == user.Id))
                    {
                        ModelState.AddModelError(string.Empty, "You have already registered as a volunteer.");
                        return View(model);
                    }
                    volunteer.UserId = user.Id;
                }
            }

            _context.Volunteers.Add(volunteer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ThankYou));
        }

        [HttpGet]
        public IActionResult ThankYou() => View();

        // "Employee: ... view volunteer sign-ups"
        [Authorize(Roles = "Employee")]
        [HttpGet]
        public async Task<IActionResult> SignUps()
        {
            var volunteers = await _context.Volunteers
                .OrderByDescending(v => v.RegisteredAt)
                .ToListAsync();
            return View(volunteers);
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var volunteer = await _context.Volunteers.FindAsync(id);
            if (volunteer is null) return NotFound();

            volunteer.Status = VolunteerStatus.Approved;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(SignUps));
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var volunteer = await _context.Volunteers.FindAsync(id);
            if (volunteer is null) return NotFound();

            volunteer.Status = VolunteerStatus.Rejected;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(SignUps));
        }
    }
}