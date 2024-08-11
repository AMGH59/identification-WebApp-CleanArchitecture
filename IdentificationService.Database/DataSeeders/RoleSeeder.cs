using IdentificationService.Application.Constants;
using IdentificationService.Database.Interfaces;
using IdentificationService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace IdentificationService.Database.DataSeeders
{
    internal class RoleSeeder
    {
        public static async Task Seed(IApplicationDbContext applicationDbContext, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            var roles = new List<ApplicationRole> { 
                new(RoleTypes.Admin), 
                new(RoleTypes.Operator) 
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role.Name))
                {
                    await roleManager.CreateAsync(role);
                }
            }

            var superAdminUser = new ApplicationUser("SuperAdmin")
            {
                Id = Guid.NewGuid(),
            };

            var adminResult = await userManager.CreateAsync(superAdminUser, "123abc123ABC+");

            if (adminResult.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdminUser, roles[0].Name);
            }

            var opeUser = new ApplicationUser("Ope")
            {
                Id = Guid.NewGuid(),
                Email = "b.a@gmail.com",
                EmailConfirmed = true,
            };

            var opeResult = await userManager.CreateAsync(opeUser, "123abc123ABC+");


            if (opeResult.Succeeded)
            {
                await userManager.AddToRoleAsync(opeUser, roles[1].Name);
            }

            var a = await userManager.GetRolesAsync(superAdminUser);

            await applicationDbContext.SaveChangesAsync();
        }
    }
}
