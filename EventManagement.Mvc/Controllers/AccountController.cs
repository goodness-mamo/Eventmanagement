
using System.Security.Claims;
using EventManagement.Mvc.Models.ViewModels;
using EventManagement.Mvc.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Mvc.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly AuthenticationApiService _authService;

    public AccountController(AuthenticationApiService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var (response, error) =
            await _authService.RegisterAsync(model);

        if (response == null)
        {
            ModelState.AddModelError(string.Empty,
                error ?? "Registration failed.");

            return View(model);
        }

        await SignInUserAsync(response);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var (response, error) =
            await _authService.LoginAsync(model);

        if (response == null)
        {
            ModelState.AddModelError(string.Empty,
                error ?? "Login failed.");

            return View(model);
        }

        await SignInUserAsync(response);

        return RedirectToAction("Index", "Home");
    }

    private async Task SignInUserAsync(ApiAuthResponse response)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier,
                response.UserId.ToString()),

            new Claim(ClaimTypes.Email, response.Email),

            new Claim(ClaimTypes.Name, response.Email),

            new Claim(ClaimTypes.Role, response.Role),

            new Claim("AccessToken", response.Token)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties
        {
            IsPersistent = false,
            ExpiresUtc = response.ExpiresAt > DateTime.UtcNow
                ? new DateTimeOffset(response.ExpiresAt)
                : DateTimeOffset.UtcNow.AddHours(1)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}

