// 5) Swagger/AuthorizeOperationFilter.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Practice.Swagger
{
    public class AuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasAllowAnonymous =
                context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() ||
                (context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() ?? false);

            if (hasAllowAnonymous)
                return;

            var authorizeAttributes =
                context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().ToList();

            if (context.MethodInfo.DeclaringType is not null)
            {
                authorizeAttributes.AddRange(
                    context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>());
            }

            if (!authorizeAttributes.Any())
                return;

            operation.Security ??= new List<OpenApiSecurityRequirement>();

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer"),
                    new List<string>()
                }
            });



            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });
        }
    }
}
