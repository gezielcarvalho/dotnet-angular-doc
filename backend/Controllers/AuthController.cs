using Backend.Models.DTO.Auth;
using Backend.Models.DTO.Common;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        
        if (response == null)
            return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse("Invalid username or password"));

        return Ok(ApiResponse<LoginResponse>.SuccessResponse(response, "Login successful"));
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Register([FromBody] RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        
        if (response == null)
            return BadRequest(ApiResponse<LoginResponse>.ErrorResponse("Username or email already exists"));

        return Ok(ApiResponse<LoginResponse>.SuccessResponse(response, "Registration successful"));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var success = await _authService.ChangePasswordAsync(userId, request);
        
        if (!success)
            return BadRequest(ApiResponse<bool>.ErrorResponse("Invalid current password"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Password changed successfully"));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserInfo>>> GetCurrentUser()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var user = await _authService.GetUserByIdAsync(userId);
        
        if (user == null)
            return NotFound(ApiResponse<UserInfo>.ErrorResponse("User not found"));

        var userInfo = new UserInfo
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role
        };

        return Ok(ApiResponse<UserInfo>.SuccessResponse(userInfo));
    }
}
