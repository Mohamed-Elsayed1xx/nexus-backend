using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusBackend.DTOs;
using NexusBackend.Helpers;
using NexusBackend.Services;

namespace NexusBackend.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _userService.GetAllAsync();
        return Ok(ApiResponse<List<UserDTO>>.Ok(result));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var result = await _userService.GetByIdAsync(GetUserId());
        return Ok(ApiResponse<UserDTO>.Ok(result));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO dto)
    {
        var result = await _userService.UpdateProfileAsync(GetUserId(), dto);
        return Ok(ApiResponse<UserDTO>.Ok(result, "Profile updated successfully."));
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
    {
        await _userService.ChangePasswordAsync(GetUserId(), dto);
        return Ok(ApiResponse<string>.Ok("Password changed successfully."));
    }
}