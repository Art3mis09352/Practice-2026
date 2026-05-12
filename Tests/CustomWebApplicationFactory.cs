using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Data;

namespace Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Practice.Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Добавляем тестовые Jwt-настройки в конфигурацию
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                var settings = new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "01234567890123456789012345678901", // минимум 32 байта для HMAC
                    ["Jwt:Issuer"] = "test-issuer",
                    ["Jwt:Audience"] = "test-audience"
                };
                conf.AddInMemoryCollection(settings);
            });

            builder.ConfigureServices(services =>
            {
                // Удаляем существующие регистрации DbContext/DbContextOptions
                var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);

                var appDbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(AppDbContext));
                if (appDbDescriptor != null) services.Remove(appDbDescriptor);

                // Регистрируем InMemory БД для тестов
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                // Собираем провайдер и создаём роли/базу
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var scoped = scope.ServiceProvider;
                var context = scoped.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();

                var roleManager = scoped.GetService<RoleManager<IdentityRole>>();
                if (roleManager != null)
                {
                    var roles = new[] { "User", "Owner", "Admin" };
                    foreach (var role in roles)
                    {
                        if (!roleManager.RoleExistsAsync(role).Result)
                        {
                            roleManager.CreateAsync(new IdentityRole(role)).Wait();
                        }
                    }
                }
            });
        }
    }
}