using Application.Data.DTO.Auth;
using Application.Data.DTO.Route.Read;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Tests.Integration.Helpers;

public static class ApiTestHelper
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static HttpClient CreateClient(CustomWebApplicationFactory factory)
    {
        return factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public static async Task<(HttpResponseMessage response, RegisterUserDto dto)> RegisterUserAsync(
        HttpClient client,
        string? email = null,
        string password = "Password123!")
    {
        var dto = new RegisterUserDto
        {
            Email = email ?? $"user_{Guid.NewGuid():N}@example.com",
            Password = password,
            Phone = "+10000000000",
            Username = $"user_{Guid.NewGuid():N}"[..12]
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", dto);
        return (response, dto);
    }

    public static async Task<HttpResponseMessage> LoginAsync(
        HttpClient client,
        string email,
        string password)
    {
        var dto = new
        {
            Email = email,
            Password = password
        };

        return await client.PostAsJsonAsync("/api/auth/login", dto);
    }

    public static async Task AuthenticateAsUserAsync(
        HttpClient client,
        string email,
        string password)
    {
        var loginResponse = await LoginAsync(client, email, password);

        if (!loginResponse.IsSuccessStatusCode)
        {
            var body = await loginResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Не удалось залогиниться. Status={(int)loginResponse.StatusCode}, Body={body}");
        }

        var token = ExtractAuthCookie(loginResponse);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task<int> CreateRouteAsync(HttpClient client, object? payload = null)
    {
        payload ??= new
        {
            title = "Integration route",
            description = "Created from integration test",
            startDate = "2026-05-20T00:00:00Z",
            endDate = "2026-05-21T00:00:00Z",
            budget = 1000,
            isPublic = false,
            days = new[]
            {
                new
                {
                    dayNumber = 1,
                    title = "Day 1",
                    notes = "Notes",
                    blocks = Array.Empty<object>()
                }
            }
        };

        var response = await client.PostAsJsonAsync("/api/routes", payload);
        response.EnsureSuccessStatusCode();

        var route = await response.Content.ReadFromJsonAsync<RouteResponseDTO>(JsonOptions)
            ?? throw new InvalidOperationException("RouteResponseDTO не десериализовался.");

        return route.Id;
    }

    public static async Task<T> ReadAsAsync<T>(HttpResponseMessage response)
    {
        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return result ?? throw new InvalidOperationException($"Не удалось прочитать {typeof(T).Name} из ответа.");
    }

    private static string ExtractAuthCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
        {
            throw new InvalidOperationException("В ответе нет Set-Cookie.");
        }

        var authCookie = setCookieHeaders.FirstOrDefault(x => x.StartsWith("auth=", StringComparison.OrdinalIgnoreCase));
        if (authCookie == null)
        {
            throw new InvalidOperationException("В ответе нет auth cookie.");
        }

        var tokenPart = authCookie.Split(';', StringSplitOptions.RemoveEmptyEntries)[0];
        var token = tokenPart["auth=".Length..];

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("JWT token в cookie пустой.");
        }

        return token;
    }
    public static async Task<RouteResponseDTO> CreateRouteAndReadAsync(HttpClient client, object? payload = null)
    {
        payload ??= new
        {
            title = "Integration route",
            description = "Created from integration test",
            startDate = "2026-05-20T00:00:00Z",
            endDate = "2026-05-21T00:00:00Z",
            budget = 1000,
            isPublic = false,
            days = new[]
            {
                new
                {
                    dayNumber = 1,
                    title = "Day 1",
                    notes = "Notes",
                    blocks = Array.Empty<object>()
                }
            }
        };

        var response = await client.PostAsJsonAsync("/api/routes", payload);
        response.EnsureSuccessStatusCode();

        return await ReadAsAsync<RouteResponseDTO>(response);
    }

    public static async Task<HttpResponseMessage> RegisterAndAuthenticateAsync(
        HttpClient client,
        string? email = null,
        string password = "Password123!")
    {
        var (registerResponse, dto) = await RegisterUserAsync(client, email, password);

        if (registerResponse.StatusCode != HttpStatusCode.Created)
        {
            var body = await registerResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Регистрация не удалась. Status={(int)registerResponse.StatusCode}, Body={body}");
        }

        await AuthenticateAsUserAsync(client, dto.Email, dto.Password);
        return registerResponse;
    }
}
