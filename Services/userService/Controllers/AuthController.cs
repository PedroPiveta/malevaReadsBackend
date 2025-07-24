using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using userService.DTOs.User;
using userService.Services;

[ApiController]
[Route("api/users")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly AuthService _authService;

    public AuthController(IUserService userService, AuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var (success, token, error) = await _userService.RegisterAsync(dto);
        
        if (!success)
            return BadRequest(error);
            
        return Created("api/user/register", new { token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var (success, token, error) = await _userService.LoginAsync(dto);
        
        if (!success)
            return Unauthorized(error);
            
        return Ok(new { token });
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetOwnProfile()
    {
        var userId = _authService.GetUserId();

        if (userId == null)
            return Unauthorized();

        var user = await _userService.GetUserByIdAsync(userId.Value);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateOwnProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = _authService.GetUserId();

        if (userId == null)
            return Unauthorized();

        var (success, error) = await _userService.UpdateUserProfileAsync(userId.Value, dto);

        if (!success)
            return NotFound();

        return Ok();
    }




}