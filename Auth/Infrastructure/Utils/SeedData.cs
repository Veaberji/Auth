using Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Auth.Infrastructure.Utils
{
    public static class SeedData
    {
        public static async Task CreateBannedRole(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var roleManager =
                    scope.ServiceProvider.GetService(typeof(RoleManager<IdentityRole>)) as RoleManager<IdentityRole>;
                var role = AppRoles.BannedRole;

                if (await roleManager.FindByNameAsync(role) == null)
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }

        }
    }
}
