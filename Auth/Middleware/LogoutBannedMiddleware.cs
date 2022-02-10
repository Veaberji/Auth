using Auth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Auth.Middleware
{
    public class LogoutBannedMiddleware
    {
        private readonly RequestDelegate _next;

        public LogoutBannedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, SignInManager<User> signInManager,
            UserManager<User> userManager)
        {
            var role = AppRoles.BannedRole;
            var userName = context.User.Identity?.Name;
            if (userName != null && await userManager.IsInRoleAsync(
                    await userManager.FindByNameAsync(userName), role))
            {
                await signInManager.SignOutAsync();
            }
            await _next.Invoke(context);
        }
    }
}
