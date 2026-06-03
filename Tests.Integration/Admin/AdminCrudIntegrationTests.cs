using Application.DTO.Admin;
using Application.DTO.Block;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Tests.Integration.Helpers;

namespace Tests.Integration.Admin;

public class AdminCrudIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AdminCrudIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient client, string email, string password)> CreateAdminAsync()
    {
        var client = ApiTestHelper.CreateClient(_factory);
        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);

        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Domain.Entities.User>>();
            var dbUser = await userManager.FindByEmailAsync(user.Email)
                ?? throw new InvalidOperationException("User not found.");

            await userManager.AddToRoleAsync(dbUser, "Admin");
        }

        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);
        return (client, user.Email, user.Password);
    }

    [Fact]
    public async Task Admin_Can_Approve_Block()
    {
        int blockId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var block = await BlockSeedHelper.AddUnapprovedBlockAsync(db, 301);
            blockId = block.Id;
        }

        var (client, _, _) = await CreateAdminAsync();

        var response = await client.PatchAsJsonAsync($"/api/admin/blocks/{blockId}/approve", new { });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var block = await db.Blocks.FindAsync(blockId);

            Assert.NotNull(block);
            Assert.Equal(Domain.Enums.BlockStatus.Approved, block.Status);
        }
    }

    [Fact]
    public async Task Admin_Approve_Missing_Block_ReturnsNotFound()
    {
        var (client, _, _) = await CreateAdminAsync();

        var response = await client.PatchAsJsonAsync("/api/admin/blocks/999999/approve", new { });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Admin_Can_Delete_User()
    {
        var (adminClient, _, _) = await CreateAdminAsync();

        var victimClient = ApiTestHelper.CreateClient(_factory);
        var (_, victim) = await ApiTestHelper.RegisterUserAsync(victimClient);

        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Domain.Entities.User>>();
            var dbUser = await userManager.FindByEmailAsync(victim.Email)
                ?? throw new InvalidOperationException("Victim not found.");

            var response = await adminClient.DeleteAsync($"/api/admin/users/{dbUser.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }

    [Fact]
    public async Task Admin_Delete_Missing_User_ReturnsNotFound()
    {
        var (client, _, _) = await CreateAdminAsync();

        var response = await client.DeleteAsync("/api/admin/users/missing-user-id");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Admin_Can_Delete_Block()
    {
        int blockId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var block = await BlockSeedHelper.AddUnapprovedBlockAsync(db, 302);
            blockId = block.Id;
        }

        var (client, _, _) = await CreateAdminAsync();

        var response = await client.DeleteAsync($"/api/admin/blocks/{blockId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var block = await db.Blocks.FindAsync(blockId);

            Assert.Null(block);
        }
    }

    [Fact]
    public async Task Admin_Can_Get_Pending_Blocks_With_Search_And_Pagination()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await BlockSeedHelper.AddUnapprovedBlockAsync(db, 303);
            await BlockSeedHelper.AddUnapprovedBlockAsync(db, 304);
            await BlockSeedHelper.AddApprovedBlockAsync(db, 305);

            var target = await db.Blocks.FindAsync(304);
            if (target != null)
            {
                target.Title = "Hidden admin pending block";
                target.Address = "Secret street";
                await db.SaveChangesAsync();
            }
        }

        var (client, _, _) = await CreateAdminAsync();

        var response = await client.GetAsync("/api/admin/blocks/pending?search=secret&page=1&pageSize=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await ApiTestHelper.ReadAsAsync<PagedBlocksResponseDTO>(response);

        Assert.Equal(1, body.Page);
        Assert.Equal(1, body.PageSize);
        Assert.Equal(1, body.TotalCount);
        Assert.Single(body.Items);
        Assert.Equal("Hidden admin pending block", body.Items.Single().Title);
        Assert.Equal(Domain.Enums.BlockStatus.Pending, body.Items.Single().Status);
    }

    [Fact]
    public async Task Admin_Can_Get_Users_With_Search_And_Pagination()
    {
        var (client, _, _) = await CreateAdminAsync();

        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var targetEmail = $"target_{uniqueSuffix}@example.com";

        await ApiTestHelper.RegisterUserAsync(ApiTestHelper.CreateClient(_factory), email: targetEmail);
        await ApiTestHelper.RegisterUserAsync(ApiTestHelper.CreateClient(_factory), email: $"other_{uniqueSuffix}@example.com");

        var response = await client.GetAsync($"/api/admin/users?search=target_{uniqueSuffix}&page=1&pageSize=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await ApiTestHelper.ReadAsAsync<PagedAdminUsersResponseDTO>(response);

        Assert.Equal(1, body.Page);
        Assert.Equal(1, body.PageSize);
        Assert.Equal(1, body.TotalCount);
        Assert.Single(body.Items);
        Assert.Equal(targetEmail, body.Items.Single().Email);
    }

    [Fact]
    public async Task Owner_Cannot_Access_Admin_Endpoint()
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

        var response = await client.PatchAsJsonAsync("/api/admin/blocks/1/approve", new { });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
