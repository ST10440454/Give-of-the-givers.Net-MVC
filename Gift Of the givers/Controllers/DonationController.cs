using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using GiftOfTheGivers.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Controllers
{
    public class DonationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DonationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new DonationCreateViewModel
            {
                IsGuest = !User.Identity!.IsAuthenticated,
                AvailableProjects = await _context.ReliefProjects
                    .Where(p => p.Status == ProjectStatus.Active)
                    .OrderBy(p => p.ProjectName)
                    .ToListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DonationCreateViewModel model)
        {
            model.IsGuest = !User.Identity!.IsAuthenticated;

            if (model.IsGuest && string.IsNullOrWhiteSpace(model.GuestName))
                ModelState.AddModelError(nameof(model.GuestName), "Please enter a name, or log in to donate as a registered donor.");

            if (!ModelState.IsValid)
            {
                model.AvailableProjects = await _context.ReliefProjects
                    .Where(p => p.Status == ProjectStatus.Active)
                    .OrderBy(p => p.ProjectName)
                    .ToListAsync();
                return View(model);
            }

            // Placeholder tax certificate is created together with the donation (single SaveChanges),
            // per the brief: "the system must generate a placeholder tax certificate"
            var donation = new Donation
            {
                Amount = model.Amount,
                Currency = model.Currency,
                DonationType = model.DonationType,
                ProjectId = model.ProjectId,
                PaymentStatus = PaymentStatus.Completed, // dummy record — no real payment gateway at prototype stage
                TaxCertificate = new TaxCertificate
                {
                    CertificateNumber = GiftOfTheGivers.Helpers.TaxCertificateHelper.FormatCertificateNumber()
                }
            };

            if (model.IsGuest)
            {
                donation.GuestName = model.GuestName;
            }
            else
            {
                var user = await _userManager.GetUserAsync(User);
                donation.DonorUserId = user?.Id;
            }

            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Certificate), new { id = donation.DonationId });
        }

        [HttpGet]
        public async Task<IActionResult> Certificate(int id)
        {
            var donation = await _context.Donations
                .Include(d => d.TaxCertificate)
                .Include(d => d.Project)
                .Include(d => d.DonorUser)
                .FirstOrDefaultAsync(d => d.DonationId == id);

            if (donation is null || donation.TaxCertificate is null) return NotFound();

            return View(donation);
        }
    }
}