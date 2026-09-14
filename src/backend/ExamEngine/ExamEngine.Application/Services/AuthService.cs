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
        // 1. Kiểm tra xác nhận mật khẩu
        if (request.Password != request.ConfirmPassword)
        {
            throw new Exception("Mật khẩu xác nhận không khớp.");
        }

        // 2. Chặn đăng ký quyền Admin trái phép
        if (request.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("Không được phép đăng ký vai trò Quản trị viên.");
        }

        // 3. Kiểm tra trùng Email
        var isEmailTaken = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (isEmailTaken)
        {
            throw new Exception("Email này đã được sử dụng.");
        }

        // 4. Mặc định người đăng ký luôn là Student nếu không truyền
        var targetRoleName = string.IsNullOrWhiteSpace(request.RoleName) ? "Student" : request.RoleName.Trim();

        // 1. Chặn tuyệt đối không cho tự đăng ký quyền Admin
        if (targetRoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("Không được phép đăng ký vai trò Quản trị viên.");
        }

        // 2. Chỉ cho phép 2 vai trò hợp lệ trong hệ thống thi: Student hoặc Instructor
        var allowedRoles = new[] { "Student", "Instructor" };
        if (!allowedRoles.Any(r => r.Equals(targetRoleName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new Exception("Vai trò đăng ký không hợp lệ. Chỉ chấp nhận Student hoặc Instructor.");
        }

        // 3. Tìm Role trong cơ sở dữ liệu
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name.ToLower() == targetRoleName.ToLower());

        if (role == null)
        {
            throw new Exception($"Hệ thống chưa khởi tạo vai trò '{targetRoleName}'.");
        }

        // 5. Băm mật khẩu thông qua Interface
        var hashedPassword = _passwordHasher.HashPassword(request.Password);

        // 6. Tạo đối tượng User và lưu vào PostgreSQL
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = hashedPassword,
            RoleId = role.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync();

        // 7. Tạo mã Token thông qua Interface
        var token = _jwtTokenGenerator.GenerateToken(newUser, role.Name);

        // 8. Trả kết quả về Controller
        return new AuthResponseDto
        {
            UserId = newUser.Id,
            FullName = newUser.FullName,
            Email = newUser.Email,
            Role = role.Name,
            AccessToken = token
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        // 1. Tìm User theo Email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (user == null)
        {
            throw new Exception("Tài khoản hoặc mật khẩu không chính xác.");
        }

        // 2. Kiểm tra trạng thái tài khoản
        if (!user.IsActive)
        {
            throw new Exception("Tài khoản của bạn đã bị khóa.");
        }

        // 3. So khớp mật khẩu qua Interface
        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new Exception("Tài khoản hoặc mật khẩu không chính xác.");
        }

        // 4. Lấy Role tương ứng
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == user.RoleId);
        var roleName = role != null ? role.Name : "Student";

        // 5. Sinh JWT Token
        var token = _jwtTokenGenerator.GenerateToken(user, roleName);

        // 6. Trả kết quả về Controller
        return new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = roleName,
            AccessToken = token
        };
    }
}