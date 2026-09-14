using ExamEngine.Application.Interfaces;

namespace ExamEngine.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        // BCrypt tự động tạo Salt ngẫu nhiên và băm mật khẩu
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        // So khớp mật khẩu người dùng nhập với chuỗi hash trong DB
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}