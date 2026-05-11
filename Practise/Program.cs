

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Practice.Data;
using Practice.Models.Entities;
using Practice.Services;
using Practice.Services.Auth;
using Practice.Services.Identity;
using Practice.Services.Infrastructure;
using Practice.Services.Users;
using Practice.Swagger;
using System.Text;

namespace Practice
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            //builder.Services.AddIdentity<User, IdentityRole>(options => 
            //{
            //    options.User.RequireUniqueEmail = true;

            //    options.Password.RequireDigit = false;
            //    options.Password.RequireLowercase = false;
            //    options.Password.RequireUppercase = false;
            //    options.Password.RequireNonAlphanumeric = false;
            //    options.Password.RequiredLength = 8;
            //})
            //    .AddEntityFrameworkStores<AppDbContext>()
            //    .AddDefaultTokenProviders();
            builder.Services.AddAppIdentity();
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddFrontendCors();

            builder.Services.AddScoped<JwtTokenService>();
            builder.Services.AddScoped<AuthCookieService>();
            builder.Services.AddScoped<IUserRouteService, UserRouteService>();

            builder.Services.AddAuthorization();
            builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddScoped<JwtTokenService>();

            //builder.Services.AddScoped<IUserRouteService, UserRouteService>();
            //builder.Services.AddCors(options =>
            //{
            //    options.AddPolicy("FrontendCorsPolicy", policy =>
            //    {
            //        policy
            //            .WithOrigins(
            //                "http://localhost:5173",
            //                "http://localhost:4173",
            //                "https://localhost:5173",
            //                "https://localhost:4173")
            //            .AllowAnyHeader()
            //            .AllowAnyMethod()
            //            .AllowCredentials();
            //    });
            //});

            //builder.Services
            //    .AddAuthentication(options =>
            //    {
            //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            //    })
            //    .AddJwtBearer(options =>
            //    {
            //        options.TokenValidationParameters = new TokenValidationParameters
            //        {
            //            ValidateIssuer = true,
            //            ValidateAudience = true,
            //            ValidateLifetime = true,
            //            ValidateIssuerSigningKey = true,
            //            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            //            ValidAudience = builder.Configuration["Jwt:Audience"],
            //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            //        };
            //        options.Events = new JwtBearerEvents
            //        {
            //            OnMessageReceived = context =>
            //            {
            //                if (context.Request.Cookies.TryGetValue("auth", out var token))
            //                {
            //                    context.Token = token;
            //                    return Task.CompletedTask;
            //                }

            //                var authHeader = context.Request.Headers.Authorization.ToString();
            //                if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer "))
            //                {
            //                    context.Token = authHeader["Bearer ".Length..].Trim();
            //                }

            //                return Task.CompletedTask;
            //            },
            //            OnAuthenticationFailed = context =>
            //            {
            //                Console.WriteLine($"JWT auth failed: {context.Exception.Message}");
            //                return Task.CompletedTask;
            //            },
            //            OnTokenValidated = context =>
            //            {
            //                var name = context.Principal?.Identity?.Name;
            //                Console.WriteLine($"JWT ok for: {name}");
            //                return Task.CompletedTask;
            //            },
            //            OnChallenge = context =>
            //            {
            //                Console.WriteLine($"JWT challenge: {context.Error} | {context.ErrorDescription}");
            //                return Task.CompletedTask;
            //            }

            //        };
            //    });
            //builder.Services.AddAuthorization();


            //builder.Services.AddEndpointsApiExplorer();
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


            builder.WebHost.UseUrls("https://localhost:7191", "http://localhost:5096");





            var app = builder.Build();

            //using (var scope = app.Services.CreateScope())
            //{
            //    //var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            //    //db.Database.Migrate();
            //    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            //    var roles = new[] { "Admin", "User", "Owner"};

            //    foreach (var role in roles)
            //    {
            //        if (!roleManager.RoleExistsAsync(role).Result)
            //        {
            //            if(!await roleManager.RoleExistsAsync(role))
            //            {
            //                await roleManager.CreateAsync(new IdentityRole { Name = role});
            //            }
            //        }
            //    }
            //}
            await app.InitializeDatabaseAsync();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                
                app.UseSwagger();
                app.UseSwaggerUI();

            }

            app.UseHttpsRedirection();

            app.UseCors("FrontendCorsPolicy");

            

            app.UseAuthentication();

            app.UseAppAntiforgery();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }

    
}
