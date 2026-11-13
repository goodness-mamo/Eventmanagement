using EventManagementApi.DTOs.Auth;
using EventManagementApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST: api/Auth/register
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (result.Response == null)
        {
            return Conflict(new
            {
                message = result.Error
            });
        }

        return Created(
            "/api/Auth/login",
            result.Response);
    }

    // POST: api/Auth/login
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result.Response == null)
        {
            return Unauthorized(new
            {
                message = result.Error
            });
        }

        return Ok(result.Response);
    }
}