using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusBackend.DTOs;
using NexusBackend.Helpers;
using NexusBackend.Services;

namespace NexusBackend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
    {
        // BUG FIX: كان بيرجع 200 مش 201 للـ Created
        var result = await _authService.RegisterAsync(dto);
        return StatusCode(201, ApiResponse<AuthResponseDTO>.Created(result, "Registered successfully."));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(ApiResponse<AuthResponseDTO>.Ok(result, "Logged in successfully."));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDTO dto)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
        return Ok(ApiResponse<AuthResponseDTO>.Ok(result, "Token refreshed successfully."));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDTO dto)
    {
        await _authService.LogoutAsync(dto.RefreshToken);
        return Ok(ApiResponse<string>.Ok("Logged out successfully."));
    }
}