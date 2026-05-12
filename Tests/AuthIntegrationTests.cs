using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_ReturnsCreated_And_Login_Succeeds()
        {
            var email = "integtest1@example.com";
            var password = "Password123!";

            var registerObj = new
            {
                Email = email,
                Password = password,
                Phone = "+10000000000",
                Username = "integtest1"
            };

            var regContent = new StringContent(JsonSerializer.Serialize(registerObj), Encoding.UTF8, "application/json");
            var regResponse = await _client.PostAsync("/api/auth/register", regContent);

            Assert.Equal(HttpStatusCode.Created, regResponse.StatusCode);

            var regBody = await regResponse.Content.ReadAsStringAsync();
            using var regDoc = JsonDocument.Parse(regBody);
            Assert.Equal(email, regDoc.RootElement.GetProperty("email").GetString());

            // Попробуем залогиниться
            var loginObj = new
            {
                Email = email,
                Password = password
            };

            var loginContent = new StringContent(JsonSerializer.Serialize(loginObj), Encoding.UTF8, "application/json");
            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);

            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var loginBody = await loginResponse.Content.ReadAsStringAsync();
            using var loginDoc = JsonDocument.Parse(loginBody);
            Assert.Equal(email, loginDoc.RootElement.GetProperty("email").GetString());
        }

        [Fact]
        public async Task Register_SameEmail_ReturnsConflict()
        {

            var email = "integtest2@example.com";
            var password = "Password123!";

            var registerObj = new
            {
                Email = email,
                Password = password,
                Phone = "+10000000001",
                Username = "integtest2"
            };

            var regContent = new StringContent(JsonSerializer.Serialize(registerObj), Encoding.UTF8, "application/json");
            var first = await _client.PostAsync("/api/auth/register", regContent);
            Assert.Equal(HttpStatusCode.Created, first.StatusCode);

            // Повторная регистрация тем же email => Conflict
            var second = await _client.PostAsync("/api/auth/register", new StringContent(JsonSerializer.Serialize(registerObj), Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        }
    }
}