using Infrastructure.Data;
using Infrastructure.Services.Email;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Practice.Program>
{
    private readonly string _dbName = $"IntegrationTestsDb_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "01234567890123456789012345678901",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["EmailConfirmation:PublicApiBaseUrl"] = "https://localhost",
                ["EmailConfirmation:FrontendResultUrl"] = ""
            };

            config.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            services.AddDataProtection()
                .UseEphemeralDataProtectionProvider();

            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();

            var optionsConfigurationDescriptors = services
                .Where(d =>
                    d.ServiceType.FullName != null &&
                    d.ServiceType.FullName.StartsWith("Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration") &&
                    d.ServiceType.IsGenericType &&
                    d.ServiceType.GenericTypeArguments.Length == 1 &&
                    d.ServiceType.GenericTypeArguments[0] == typeof(AppDbContext))
                .ToList();

            foreach (var descriptor in optionsConfigurationDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });

            services.RemoveAll<IEmailSender>();
            services.AddScoped<IEmailSender, FakeEmailSender>();

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;

            var db = scopedServices.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
            SeedRolesAsync(roleManager).GetAwaiter().GetResult();
        });
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "User", "Owner", "Admin" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private sealed class FakeEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            return Task.CompletedTask;
        }
    }
}
