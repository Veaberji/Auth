using Auth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Auth.Middleware
{
    public class LogoutMiddleware
    {
        private readonly RequestDelegate _next;

        public LogoutMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, SignInManager<User> signInManager,
            UserManager<User> userManager)
        {
            var role = AppRoles.BannedRole;
            var userName = context.User.Identity?.Name ?? "";
            var userInDb = await userManager.FindByNameAsync(userName);
            if (userInDb == null ||
            await userManager.IsInRoleAsync(
                    await userManager.FindByNameAsync(userName), role))
            {
                await signInManager.SignOutAsync();
            }
            await _next.Invoke(context);
        }
    }
}
