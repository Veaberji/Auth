using Auth.Models;
using Auth.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Auth.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;


        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            var userRoles = new List<UserRoleViewModel>();
            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                var isInRole = await _userManager.IsInRoleAsync(
                    user, AppRoles.BannedRole);
                userRoles.Add(new UserRoleViewModel
                {
                    User = user,
                    RoleName = isInRole ? AppRoles.BannedRole : null
                });
            }
            return View(userRoles);
        }

        [AllowAnonymous]
        public IActionResult Create()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = new User
            {
                UserName = model.Login,
                Email = model.Email,
                RegistrationDate = DateTime.Now,
                LastLoginDate = null
            };
            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (createResult.Succeeded)
            {
                return RedirectToAction("Index");
            }
            AddErrorsFromResult(createResult);
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByNameAsync(model.Login);
            if (user == null)
            {
                AddLoginError();
                return View(model);
            }
            if (await _userManager.IsInRoleAsync(user, AppRoles.BannedRole))
            {
                TempData["message"] = $"User '{user.UserName}' was blocked";
                return View(model);
            }
            await _signInManager.SignOutAsync();
            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, false, false);

            if (result.Succeeded)
            {
                user.LastLoginDate = DateTime.Now;
                await _userManager.UpdateAsync(user);
                return RedirectToAction("Index");
            }
            AddLoginError();
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> BlockAll(string[] selectedUsers)
        {
            foreach (var id in selectedUsers)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    continue;
                }
                var result = await _userManager.AddToRoleAsync(
                    user, AppRoles.BannedRole);
                if (!result.Succeeded)
                {
                    AddErrorsFromResult(result);
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UnblockAll(string[] selectedUsers)
        {
            foreach (var id in selectedUsers)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    continue;
                }
                var result = await _userManager.RemoveFromRoleAsync(
                    user, AppRoles.BannedRole);
                if (!result.Succeeded)
                {
                    AddErrorsFromResult(result);
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAll(string[] selectedUsers)
        {
            foreach (var id in selectedUsers)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    continue;
                }
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    AddErrorsFromResult(result);
                }
                await LogoutUser(user);
            }
            return RedirectToAction("Index");
        }

        private void AddLoginError()
        {
            ModelState.AddModelError(nameof(LoginViewModel.Login),
                "Invalid Login or password");
        }

        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        private async Task LogoutUser(User user)
        {
            if (User.Identity != null && User.Identity.Name == user.UserName)
            {
                await _signInManager.SignOutAsync();
            }
        }
    }
}
