using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in new[] { "Employee", "Donor" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }

            const string employeeEmail = "employee@giftofthegivers.org";
            if (await userManager.FindByEmailAsync(employeeEmail) is null)
            {
                var employee = new ApplicationUser
                {
                    UserName = employeeEmail,
                    Email = employeeEmail,
                    FullName = "Lerothodi Ramokhoase",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(employee, "Employee@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(employee, "Employee");
                }
            }

            // Demo relief project so the Donate dropdown and Relief Projects page have content
            var db = services.GetRequiredService<ApplicationDbContext>();
            if (!await db.ReliefProjects.AnyAsync())
            {
                var emp = await userManager.FindByEmailAsync(employeeEmail);
                if (emp is not null)
                {
                    db.ReliefProjects.Add(new ReliefProject
                    {
                        ProjectName = "Flood Relief – Eastern Cape",
                        Description = "Food, shelter, bedding and hygiene kits for flood-affected families.",
                        Location = "Eastern Cape",
                        Status = ProjectStatus.Active,
                        CreatedByUserId = emp.Id
                    });
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}