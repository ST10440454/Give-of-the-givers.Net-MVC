using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var donations = await _context.Donations
                .Include(d => d.DonorUser)
                .Include(d => d.Project)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();

            // Totals grouped by currency — amounts in different currencies can't be
            // summed together at prototype stage (no live exchange rate conversion).
            var totalsByCurrency = donations
                .GroupBy(d => d.Currency)
                .Select(g => new CurrencyTotal
                {
                    Currency = g.Key,
                    Total = g.Sum(d => d.Amount),
                    Count = g.Count()
                })
                .OrderBy(t => t.Currency)
                .ToList();

            var model = new EmployeeDashboardViewModel
            {
                Donations = donations,
                TotalsByCurrency = totalsByCurrency
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Enquiries()
        {
            var enquiries = await _context.ContactEnquiries
                .OrderByDescending(e => e.SubmittedAt)
                .ToListAsync();

            return View(enquiries);
        }
    }
}