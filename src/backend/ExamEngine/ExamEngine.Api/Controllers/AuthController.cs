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

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResult(result, "Làm mới Token thành công."));
    }

    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequestDto request)
    {
        var success = await _authService.RevokeTokenAsync(request);
        if (!success)
        {
            return BadRequest(ApiResponse<string>.FailureResult("Token không hợp lệ hoặc đã bị thu hồi trước đó."));
        }
        return Ok(ApiResponse<string>.SuccessResult("Thu hồi Token thành công (Đăng xuất thiết bị)."));
    }


    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto request)
    {
        var result = await _authService.VerifyEmailAsync(request);
        return Ok(ApiResponse<bool>.SuccessResult(result, "Xác thực email thành công! Bây giờ bạn có thể đăng nhập."));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        await _authService.ForgotPasswordAsync(request);
        return Ok(ApiResponse<string>.SuccessResult(string.Empty, "Nếu email tồn tại trong hệ thống, mã xác thực đã được gửi đi."));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        return Ok(ApiResponse<bool>.SuccessResult(result, "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập bằng mật khẩu mới."));
    }
}