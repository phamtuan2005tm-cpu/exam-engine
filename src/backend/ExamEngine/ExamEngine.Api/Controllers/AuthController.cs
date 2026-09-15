using ExamEngine.Application.Common;
using ExamEngine.Application.DTOs.Auth;
using ExamEngine.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
namespace ExamEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        // 1. Chỉ gọi Service, không cần try-catch
        var result = await _authService.RegisterAsync(request);

        // 2. Chỉ xử lý khi THÀNH CÔNG: bọc vào hộp ApiResponse và trả HTTP 200 OK
        return Ok(ApiResponse<AuthResponseDto>.SuccessResult(result, "Đăng ký tài khoản thành công."));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResult(result, "Đăng nhập thành công."));
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUserProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);

        var data = new
        {
            UserId = userId,
            Email = email,
            Role = role
        };

        return Ok(ApiResponse<object>.SuccessResult(data, "Lấy thông tin người dùng từ Token thành công."));
    }

    [Authorize(Roles = "Instructor")]
    [HttpGet("instructor-only")]
    public IActionResult CheckInstructorAccess()
    {
        return Ok(ApiResponse<string>.SuccessResult("Chào mừng Giảng viên truy cập khu vực quản lý đề thi!"));
    }
}