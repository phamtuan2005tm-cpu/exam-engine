using ExamEngine.Api.Middlewares;
using ExamEngine.Application.Interfaces;
using ExamEngine.Application.Services;
using ExamEngine.Infrastructure;
using ExamEngine.Infrastructure.Data;
using ExamEngine.Infrastructure.Security;

namespace ExamEngine.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Đăng ký toàn bộ dịch vụ của tầng Infrastructure
            builder.Services.AddInfrastructureServices(builder.Configuration);

            // 1. Đăng ký các dịch vụ Security & Token
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            // 2. Đăng ký DbContext interface
            builder.Services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            // 3. Đăng ký tầng nghiệp vụ Application Service
            builder.Services.AddScoped<IAuthService, AuthService>();
            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
