using Microsoft.AspNetCore.Mvc;

namespace GiftOfTheGivers.Controllers
{
    public class HomeController : Controller
    {
        private readonly Data.ApplicationDbContext _context;

        public HomeController(Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        public IActionResult About() => View();

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new Models.ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });

        [HttpGet]
        public IActionResult Contact() => View(new Models.ViewModels.ContactViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(Models.ViewModels.ContactViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var enquiry = new Models.ContactEnquiry
            {
                Name = model.Name,
                Surname = model.Surname,
                Email = model.Email,
                Phone = model.Phone,
                Enquiry = model.Enquiry
            };

            _context.ContactEnquiries.Add(enquiry);
            await _context.SaveChangesAsync();

            TempData["ContactSuccess"] = $"Thanks {model.Name}, we've received your enquiry and will be in touch shortly.";
            return RedirectToAction(nameof(Contact));
        }
    }
}