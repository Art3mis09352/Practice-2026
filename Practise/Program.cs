

using Application.Services;
using Infrastructure.Data;
using Infrastructure.Services.Auth;
using Infrastructure.Services.Email;
using Infrastructure.Services.Identity;
using Infrastructure.Services.Infrastructure;
using Infrastructure.Services.MinIO;
using Infrastructure.Services.Storage;
using Infrastructure.Services.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Minio;
using Practice.Infrastructure;
using Practice.Swagger;
using System.Text;
using IUserRouteService = Infrastructure.Services.Users.IUserRouteService;

namespace Practice
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            
            builder.Services.AddAppIdentity();
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddFrontendCors(builder.Configuration);


            builder.Services.AddScoped<JwtTokenService>();
            builder.Services.AddScoped<AuthCookieService>();
            builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
            builder.Services.AddScoped<IEmailConfirmationService, EmailConfirmationService>();
            builder.Services.AddScoped<IUserRouteService, UserRouteService>();

            builder.Services.AddAuthorization();
            builder.Services.AddEndpointsApiExplorer();
            
            builder.Services.AddSwaggerGen(options =>
            {
                options.EnableAnnotations();

                options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("bearer", document)] = []
                });
            });

            builder.Services.AddAppAntiforgery();


            var port = Environment.GetEnvironmentVariable("PORT");

            if (!string.IsNullOrWhiteSpace(port))
            {
                builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
            }
            else
            {
                builder.WebHost.UseUrls("https://localhost:7191", "http://localhost:5096");
            }

            builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));

            builder.Services.AddSingleton<IMinioClient>(_ =>
            {
                var storage = builder.Configuration.GetSection("Storage");
                return new MinioClient()
                    .WithEndpoint(storage["Endpoint"])
                    .WithCredentials(storage["AccessKey"], storage["SecretKey"])
                    .WithSSL(bool.Parse(storage["UseSsl"] ?? "false"))
                    .Build();
            });

            builder.Services.AddScoped<IObjectStorageService, MinioObjectStorageService>();




            var app = builder.Build();

            app.UseExceptionHandler();

            if (!app.Environment.IsEnvironment("Testing"))
            {
                await app.InitializeDatabaseAsync();
            }

            // Configure the HTTP request pipeline.
            
            app.UseSwagger();
            app.UseSwaggerUI();

            

            app.UseHttpsRedirection();

            app.UseCors("FrontendCorsPolicy");

            

            app.UseAuthentication();

            //app.UseAppAntiforgery();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }

    
}
