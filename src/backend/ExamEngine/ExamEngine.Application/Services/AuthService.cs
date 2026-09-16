using ExamEngine.Application.DTOs.Auth;
using ExamEngine.Application.Interfaces;
using ExamEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExamEngine.Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailService _emailService;

    public AuthService(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IEmailService emailService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailService = emailService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // 1. Chuẩn hóa dữ liệu đầu vào
        var cleanEmail = request.Email.Trim().ToLowerInvariant();
        var cleanFullName = request.FullName.Trim();

        var targetRoleName = string.IsNullOrWhiteSpace(request.RoleName) ? "Student" : request.RoleName.Trim();

        // 2. Kiểm tra mật khẩu xác nhận
        if (request.Password != request.ConfirmPassword)
        {
            throw new Exception("Mật khẩu xác nhận không khớp.");
        }

        // 3. Chặn đăng ký quyền Admin
        if (targetRoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("Không được phép đăng ký vai trò Quản trị viên.");
        }

        // 4. Chỉ chấp nhận Student hoặc Instructor
        var allowedRoles = new[] { "Student", "Instructor" };
        if (!allowedRoles.Any(r => r.Equals(targetRoleName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new Exception("Vai trò đăng ký không hợp lệ. Chỉ chấp nhận Student hoặc Instructor.");
        }

        // 5. Kiểm tra trùng Email
        var isEmailTaken = await _context.Users.AnyAsync(u => u.Email.ToLower() == cleanEmail);
        if (isEmailTaken)
        {
            throw new Exception("Email này đã được sử dụng.");
        }

        // 6. Tìm Role trong Database
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name.ToLower() == targetRoleName.ToLower());

        if (role == null)
        {
            throw new Exception($"Hệ thống chưa khởi tạo vai trò '{targetRoleName}'.");
        }

        // 7. Băm mật khẩu
        var hashedPassword = _passwordHasher.HashPassword(request.Password);

        // 8. Sinh mã OTP 6 số xác thực
        var verificationOtp = new Random().Next(100000, 999999).ToString();

        // 9. Tạo User ở trạng thái chưa kích hoạt
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = cleanFullName,
            Email = cleanEmail,
            PasswordHash = hashedPassword,
            RoleId = role.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            IsEmailConfirmed = false,
            VerificationToken = verificationOtp,
            VerificationTokenExpiresAt = DateTime.UtcNow.AddMinutes(15) // Hết hạn sau 15 phút
        };

        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync();

        // 10. Gửi email xác thực thật bằng SMTP
        var emailBody = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; line-height: 1.6; color: #333;'>
                <h2 style='color: #2563eb;'>Chào mừng {newUser.FullName} đến với ExamEngine!</h2>
                <p>Cảm ơn bạn đã đăng ký tài khoản. Để hoàn tất, vui lòng nhập mã xác thực sau:</p>
                <div style='margin: 20px 0;'>
                    <span style='background-color: #f1f5f9; padding: 12px 24px; font-size: 28px; font-weight: bold; letter-spacing: 6px; color: #2563eb; border-radius: 8px; border: 1px dashed #2563eb;'>
                        {verificationOtp}
                    </span>
                </div>
                <p>Mã xác thực này có hiệu lực trong vòng <strong>15 phút</strong>.</p>
                <p style='color: #64748b; font-size: 13px;'>Nếu bạn không thực hiện đăng ký tài khoản này, vui lòng bỏ qua email này.</p>
            </div>";

        await _emailService.SendEmailAsync(newUser.Email, "Xác thực tài khoản ExamEngine", emailBody);

        // Đăng ký xong trả về thông tin nhưng chưa cấp Token (vì cần kích hoạt email trước khi login)
        return new AuthResponseDto
        {
            UserId = newUser.Id,
            FullName = newUser.FullName,
            Email = newUser.Email,
            Role = role.Name,
            AccessToken = string.Empty,
            RefreshToken = string.Empty
        };
    }

    public async Task<bool> VerifyEmailAsync(VerifyEmailRequestDto request)
    {
        var cleanEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == cleanEmail);

        if (user == null)
        {
            throw new Exception("Không tìm thấy tài khoản tương ứng với email này.");
        }

        if (user.IsEmailConfirmed)
        {
            throw new Exception("Tài khoản này đã được xác thực trước đó.");
        }

        if (user.VerificationToken != request.Token.Trim() || user.VerificationTokenExpiresAt <= DateTime.UtcNow)
        {
            throw new Exception("Mã xác thực không chính xác hoặc đã hết hạn.");
        }

        // Kích hoạt tài khoản và xóa token
        user.IsEmailConfirmed = true;
        user.VerificationToken = null;
        user.VerificationTokenExpiresAt = null;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var cleanEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == cleanEmail);

        if (user == null)
        {
            throw new Exception("Tài khoản hoặc mật khẩu không chính xác.");
        }

        if (!user.IsActive)
        {
            throw new Exception("Tài khoản của bạn đã bị khóa.");
        }

        // Chặn đăng nhập nếu chưa xác thực Email
        if (!user.IsEmailConfirmed)
        {
            throw new Exception("Tài khoản chưa được kích hoạt. Vui lòng kiểm tra email để xác thực trước khi đăng nhập.");
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new Exception("Tài khoản hoặc mật khẩu không chính xác.");
        }

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == user.RoleId);
        var roleName = role != null ? role.Name : "Student";

        // 1. Sinh Tokens
        var accessToken = _jwtTokenGenerator.GenerateToken(user, roleName);
        var refreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();

        // 2. Lưu Refresh Token vào Database (Hạn 7 ngày)
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshTokenString,
            UserId = user.Id,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            CreatedAtUtc = DateTime.UtcNow,
            IsRevoked = false
        };

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = roleName,
            AccessToken = accessToken,
            RefreshToken = refreshTokenString
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        // 1. Tìm Refresh Token
        var existingToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (existingToken == null || existingToken.IsRevoked || existingToken.ExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh Token không hợp lệ hoặc đã hết hạn.");
        }

        var user = existingToken.User;
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Người dùng không hợp lệ hoặc tài khoản đã bị khóa.");
        }

        // 2. Lấy role của user
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == user.RoleId);
        var roleName = role != null ? role.Name : "Student";

        // 3. TOKEN ROTATION: Thu hồi vé cũ để chống tấn công phát lại (Replay Attack)
        existingToken.IsRevoked = true;

        // 4. Cấp cặp token mới
        var newAccessToken = _jwtTokenGenerator.GenerateToken(user, roleName);
        var newRefreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = newRefreshTokenString,
            UserId = user.Id,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            CreatedAtUtc = DateTime.UtcNow,
            IsRevoked = false
        };

        await _context.RefreshTokens.AddAsync(newRefreshToken);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = roleName,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenString
        };
    }

    public async Task<bool> RevokeTokenAsync(RevokeTokenRequestDto request)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (token == null || token.IsRevoked)
        {
            return false;
        }

        token.IsRevoked = true;
        await _context.SaveChangesAsync();
        return true;
    }
}