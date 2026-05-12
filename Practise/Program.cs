

using Application.Services;
using Infrastructure.Data;
using Infrastructure.Services.Auth;
using Infrastructure.Services.Identity;
using Infrastructure.Services.Infrastructure;
using Infrastructure.Services.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
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

            
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

            //builder.Services.AddDbContext<AppDbContext>(options =>
                //options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            
            builder.Services.AddAppIdentity();
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddFrontendCors();

            builder.Services.AddScoped<JwtTokenService>();
            builder.Services.AddScoped<AuthCookieService>();
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


            builder.WebHost.UseUrls("https://localhost:7191", "http://localhost:5096");





            var app = builder.Build();

            
            //await app.InitializeDatabaseAsync();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                
                app.UseSwagger();
                app.UseSwaggerUI();

            }

            //app.UseHttpsRedirection();

            app.UseCors("FrontendCorsPolicy");

            

            app.UseAuthentication();

            //app.UseAppAntiforgery();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }

    
}
