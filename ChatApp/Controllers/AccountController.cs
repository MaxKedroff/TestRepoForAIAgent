using ChatApp.Services;
using ChatApp.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers;

public class AccountController(AuthService authService) : Controller
{
    [HttpGet("/account/login")]
    [AllowAnonymous]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost("/account/login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await authService.GetUserByTokenAsync(model.Token.Trim());
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Неверный токен.");
            return View(model);
        }

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            authService.CreatePrincipalForUser(user));

        return RedirectToAction("Index", "Chat");
    }

    [HttpGet("/account/admin-login")]
    [AllowAnonymous]
    public IActionResult AdminLogin() => View(new AdminLoginViewModel());

    [HttpPost("/account/admin-login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminLogin(AdminLoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!authService.ValidateAdmin(model.Username.Trim(), model.Password))
        {
            ModelState.AddModelError(string.Empty, "Неверный логин или пароль администратора.");
            return View(model);
        }

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            authService.CreatePrincipalForAdmin(model.Username.Trim()));

        return RedirectToAction("Index", "Admin");
    }

    [Authorize]
    [HttpPost("/account/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("/account/denied")]
    public IActionResult Denied() => Content("Доступ запрещен.");
}
