using ExamEngine.Domain.Entities;
using ExamEngine.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using ExamEngine.Application.Interfaces;
namespace ExamEngine.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {

    }

// Khai báo các bảng dữ liệu
public DbSet<User> Users { get; set; }
public DbSet<Role> Roles { get; set; }
public DbSet<RefreshToken> RefreshTokens { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Cấu hình bảng Roles & Seed dữ liệu mặc định
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(50);
            entity.Property(r => r.Description).HasMaxLength(250);

            // Nạp sẵn 3 vai trò cố định vào DB
            entity.HasData(
                new Role { Id = (int)RoleType.Admin, Name = nameof(RoleType.Admin), Description = "Administrator with full system privileges" },
                new Role { Id = (int)RoleType.Instructor, Name = nameof(RoleType.Instructor), Description = "Instructor who manages exams and questions" },
                new Role { Id = (int)RoleType.Student, Name = nameof(RoleType.Student), Description = "Student who participates in exams" }
            );
        });

        // 2. Cấu hình bảng Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(150);

            // Unique Index: Đảm bảo không bao giờ có 2 tài khoản trùng Email
            entity.HasIndex(u => u.Email).IsUnique();

            // Cấu hình quan hệ: Một Role có nhiều User
            entity.HasOne(u => u.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.RoleId)
                  .OnDelete(DeleteBehavior.Restrict); // Không cho phép xóa Role khi vẫn còn User
        });

        // 3. Cấu hình bảng RefreshTokens
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.Token).IsRequired().HasMaxLength(500);

            // Cấu hình quan hệ: Một User có nhiều RefreshToken
            entity.HasOne(rt => rt.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade); // Khi xóa User thì tự động dọn sạch các RefreshToken liên quan
        });
    }
}