using Application.DTO.Block;
using Application.DTO.Block;
using System.Net;
using System.Net.Http.Json;
using Tests.Integration.Helpers;

namespace Tests.Integration.Owner;

public class OwnerCrudIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public OwnerCrudIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static object CreateOwnerBlockPayload(string title = "Owner block", string city = "Moscow")
    {
        return new
        {
            title,
            description = "Owner description",
            category = "Museum",
            city,
            address = "Tverskaya 1",
            latitude = 55.7558m,
            longitude = 37.6173m,
            avgPrice = 500
        };
    }

    [Fact]
    public async Task Owner_Can_Create_Block()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var ownerEmail = $"owner_{Guid.NewGuid():N}@example.com";
        var ownerPassword = "Password123!";

        var registerPayload = new
        {
            email = ownerEmail,
            password = ownerPassword,
            phone = "+10000000000",
            username = $"owner_{Guid.NewGuid():N}"[..12]
        };

        var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register-owner",
            registerPayload);

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        await ApiTestHelper.AuthenticateAsUserAsync(client, ownerEmail, ownerPassword);

        var response = await client.PostAsJsonAsync("/api/owner/blocks", CreateOwnerBlockPayload());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var block = await ApiTestHelper.ReadAsAsync<BlockResponseDTO>(response);
        Assert.Equal("Owner block", block.Title);
        Assert.Equal("Moscow", block.City);
        Assert.Equal(Domain.Enums.BlockStatus.Pending, block.Status);
    }

    [Fact]
    public async Task Owner_Can_Get_Own_Block_Details()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var ownerEmail = $"owner_{Guid.NewGuid():N}@example.com";
        var ownerPassword = "Password123!";

        await client.PostAsJsonAsync("/api/auth/register-owner", new
        {
            email = ownerEmail,
            password = ownerPassword,
            phone = "+10000000000",
            username = $"owner_{Guid.NewGuid():N}"[..12]
        });

        await ApiTestHelper.AuthenticateAsUserAsync(client, ownerEmail, ownerPassword);

        var createResponse = await client.PostAsJsonAsync("/api/owner/blocks", new
        {
            title = "Detailed owner block",
            description = "Detailed description",
            category = "Museum",
            city = "Moscow",
            address = "Tverskaya 10",
            latitude = 55.7558m,
            longitude = 37.6173m,
            avgPrice = 1500
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await ApiTestHelper.ReadAsAsync<BlockResponseDTO>(createResponse);

        var getResponse = await client.GetAsync($"/api/owner/blocks/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var block = await ApiTestHelper.ReadAsAsync<BlockResponseDTO>(getResponse);
        Assert.Equal(created.Id, block.Id);
        Assert.Equal("Detailed owner block", block.Title);
        Assert.Equal("Detailed description", block.Description);
        Assert.Equal(1500, block.AvgPrice);
    }

    [Fact]
    public async Task Owner_Can_Update_Own_Block()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var ownerEmail = $"owner_{Guid.NewGuid():N}@example.com";
        var ownerPassword = "Password123!";

        await client.PostAsJsonAsync("/api/auth/register-owner", new
        {
            email = ownerEmail,
            password = ownerPassword,
            phone = "+10000000000",
            username = $"owner_{Guid.NewGuid():N}"[..12]
        });

        await ApiTestHelper.AuthenticateAsUserAsync(client, ownerEmail, ownerPassword);

        var createResponse = await client.PostAsJsonAsync("/api/owner/blocks", CreateOwnerBlockPayload());
        var created = await ApiTestHelper.ReadAsAsync<BlockResponseDTO>(createResponse);

        var updatePayload = new
        {
            title = "Updated owner block",
            city = "Saint Petersburg",
            avgPrice = 900
        };

        var updateResponse = await client.PatchAsJsonAsync($"/api/owner/blocks/{created.Id}", updatePayload);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await ApiTestHelper.ReadAsAsync<BlockResponseDTO>(updateResponse);
        Assert.Equal("Updated owner block", updated.Title);
        Assert.Equal("Saint Petersburg", updated.City);
        Assert.Equal(900, updated.AvgPrice);
        Assert.Equal(Domain.Enums.BlockStatus.Pending, updated.Status);
    }

    [Fact]
    public async Task Owner_Can_Delete_Own_Block()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var ownerEmail = $"owner_{Guid.NewGuid():N}@example.com";
        var ownerPassword = "Password123!";

        await client.PostAsJsonAsync("/api/auth/register-owner", new
        {
            email = ownerEmail,
            password = ownerPassword,
            phone = "+10000000000",
            username = $"owner_{Guid.NewGuid():N}"[..12]
        });

        await ApiTestHelper.AuthenticateAsUserAsync(client, ownerEmail, ownerPassword);

        var createResponse = await client.PostAsJsonAsync("/api/owner/blocks", CreateOwnerBlockPayload());
        var created = await ApiTestHelper.ReadAsAsync<BlockResponseDTO>(createResponse);

        var deleteResponse = await client.DeleteAsync($"/api/owner/blocks/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var myPointsResponse = await client.GetAsync("/api/getmypoints/mypoints");
        var myPoints = await ApiTestHelper.ReadAsAsync<PagedBlocksResponseDTO>(myPointsResponse);

        Assert.DoesNotContain(myPoints.Items, x => x.Id == created.Id);
    }

    [Fact]
    public async Task Owner_Can_Get_Only_Own_Points()
    {
        var ownerClientA = ApiTestHelper.CreateClient(_factory);
        var ownerClientB = ApiTestHelper.CreateClient(_factory);

        var ownerAEmail = $"ownerA_{Guid.NewGuid():N}@example.com";
        var ownerBEmail = $"ownerB_{Guid.NewGuid():N}@example.com";
        const string password = "Password123!";

        await ownerClientA.PostAsJsonAsync("/api/auth/register-owner", new
        {
            email = ownerAEmail,
            password,
            phone = "+10000000000",
            username = $"ownera_{Guid.NewGuid():N}"[..12]
        });

        await ownerClientB.PostAsJsonAsync("/api/auth/register-owner", new
        {
            email = ownerBEmail,
            password,
            phone = "+10000000000",
            username = $"ownerb_{Guid.NewGuid():N}"[..12]
        });

        await ApiTestHelper.AuthenticateAsUserAsync(ownerClientA, ownerAEmail, password);
        await ApiTestHelper.AuthenticateAsUserAsync(ownerClientB, ownerBEmail, password);

        await ownerClientA.PostAsJsonAsync("/api/owner/blocks", CreateOwnerBlockPayload("A block", "Moscow"));
        await ownerClientB.PostAsJsonAsync("/api/owner/blocks", CreateOwnerBlockPayload("B block", "Kazan"));

        var responseA = await ownerClientA.GetAsync("/api/getmypoints/mypoints");
        var responseB = await ownerClientB.GetAsync("/api/getmypoints/mypoints");

        Assert.Equal(HttpStatusCode.OK, responseA.StatusCode);
        Assert.Equal(HttpStatusCode.OK, responseB.StatusCode);

        var blocksA = await ApiTestHelper.ReadAsAsync<PagedBlocksResponseDTO>(responseA);
        var blocksB = await ApiTestHelper.ReadAsAsync<PagedBlocksResponseDTO>(responseB);

        Assert.Contains(blocksA.Items, x => x.Title == "A block");
        Assert.DoesNotContain(blocksA.Items, x => x.Title == "B block");

        Assert.Contains(blocksB.Items, x => x.Title == "B block");
        Assert.DoesNotContain(blocksB.Items, x => x.Title == "A block");
    }

    [Fact]
    public async Task Owner_Can_Get_Stats()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var ownerEmail = $"owner_{Guid.NewGuid():N}@example.com";
        var ownerPassword = "Password123!";

        await client.PostAsJsonAsync("/api/auth/register-owner", new
        {
            email = ownerEmail,
            password = ownerPassword,
            phone = "+10000000000",
            username = $"owner_{Guid.NewGuid():N}"[..12]
        });

        await ApiTestHelper.AuthenticateAsUserAsync(client, ownerEmail, ownerPassword);

        var response = await client.GetAsync("/api/checkstats/stats");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task User_Cannot_Update_Other_Owner_Block()
    {
        var ownerClient = ApiTestHelper.CreateClient(_factory);
        var userClient = ApiTestHelper.CreateClient(_factory);

        var ownerEmail = $"owner_{Guid.NewGuid():N}@example.com";
        const string ownerPassword = "Password123!";

        await ownerClient.PostAsJsonAsync("/api/auth/register-owner", new
        {
            email = ownerEmail,
            password = ownerPassword,
            phone = "+10000000000",
            username = $"owner_{Guid.NewGuid():N}"[..12]
        });

        await ApiTestHelper.AuthenticateAsUserAsync(ownerClient, ownerEmail, ownerPassword);

        var createResponse = await ownerClient.PostAsJsonAsync("/api/owner/blocks", CreateOwnerBlockPayload());
        var created = await ApiTestHelper.ReadAsAsync<BlockResponseDTO>(createResponse);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(userClient);
        await ApiTestHelper.AuthenticateAsUserAsync(userClient, user.Email, user.Password);

        var response = await userClient.PatchAsJsonAsync($"/api/owner/blocks/{created.Id}", new
        {
            title = "Hacked title"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
