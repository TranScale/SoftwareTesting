using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineSalesManagementSystem.Domain.Entities;
using OnlineSalesManagementSystem.Areas.Admin.ViewModels.Auth;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
public class AuthController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        var vm = new LoginViewModel
        {
            ReturnUrl = returnUrl
        };

        return View(vm);
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var credential = (vm.Email ?? string.Empty).Trim();

        var user = await _userManager.FindByEmailAsync(credential)
            ?? await _userManager.FindByNameAsync(credential);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(vm);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Your account is inactive. Please contact administrator.");
            return View(vm);
        }

        var result = await _signInManager.PasswordSignInAsync(
            userName: user.UserName!,
            password: vm.Password,
            isPersistent: vm.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);

            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(vm);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
