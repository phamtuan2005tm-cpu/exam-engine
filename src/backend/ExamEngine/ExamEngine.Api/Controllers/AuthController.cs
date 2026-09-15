using ExamEngine.Application.Common;
using ExamEngine.Application.DTOs.Auth;
using ExamEngine.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
}