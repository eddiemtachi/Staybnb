using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Staybnb.Models;
using Staybnb.Services;
using Staybnb.ViewModels;

namespace Staybnb.Controllers;
public class AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, ActivityLogger logger) : Controller
{
    [HttpGet] public IActionResult Register() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName };
        var result = await users.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await users.AddToRoleAsync(user, "Guest");
            await logger.LogAsync(user.Id, "Register", "New account created with Guest role");
            await signIn.SignInAsync(user, false);
            return RedirectToAction("Index", "Home");
        }
        foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
        return View(model);
    }
    [HttpGet] public IActionResult Login(string? returnUrl = null) { ViewBag.ReturnUrl = returnUrl; return View(); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        if (!ModelState.IsValid) return View(model);
        var result = await signIn.PasswordSignInAsync(model.Email, model.Password, false, true);
        if (result.Succeeded)
        {
            var user = await users.FindByEmailAsync(model.Email);
            await logger.LogAsync(user?.Id, "Login", "Successful login");
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
        ModelState.AddModelError("", "Invalid login attempt.");
        return View(model);
    }
    [HttpPost] public async Task<IActionResult> Logout()
    {
        var id = users.GetUserId(User);
        await logger.LogAsync(id, "Logout");
        await signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
    public IActionResult AccessDenied() => View();
}
