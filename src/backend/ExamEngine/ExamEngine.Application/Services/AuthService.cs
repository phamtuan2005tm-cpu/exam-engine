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

    public AuthService(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
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

        // 8. Tạo User
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = cleanFullName,
            Email = cleanEmail,
            PasswordHash = hashedPassword,
            RoleId = role.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            IsEmailConfirmed = false
        };

        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync();

        // 9. Sinh Access Token và Refresh Token
        var token = _jwtTokenGenerator.GenerateToken(newUser, role.Name);
        var refreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshTokenString,
            UserId = newUser.Id,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            CreatedAtUtc = DateTime.UtcNow,
            IsRevoked = false
        };

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            UserId = newUser.Id,
            FullName = newUser.FullName,
            Email = newUser.Email,
            Role = role.Name,
            AccessToken = token,
            RefreshToken = refreshTokenString
        };
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