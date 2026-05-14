using System.Net;
using Application.DTO.Block;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Helpers;

namespace Tests.Integration.Public;

public class UnauthorizedPointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public UnauthorizedPointsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Public_Can_Get_Approved_Point_By_Id()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await BlockSeedHelper.AddApprovedBlockAsync(db, 401, city: "Moscow");
        }

        var client = ApiTestHelper.CreateClient(_factory);

        var response = await client.GetAsync("/api/unauthorizedpoint/401");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var block = await ApiTestHelper.ReadAsAsync<BlockResponseDTO>(response);
        Assert.Equal(401, block.Id);
        Assert.True(block.IsApproved);
    }

    [Fact]
    public async Task Public_Cannot_Get_Unapproved_Point_By_Id()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await BlockSeedHelper.AddUnapprovedBlockAsync(db, 402, city: "Moscow");
        }

        var client = ApiTestHelper.CreateClient(_factory);

        var response = await client.GetAsync("/api/unauthorizedpoint/402");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Public_Can_Get_Only_Approved_Points_List()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await BlockSeedHelper.AddApprovedBlockAsync(db, 403, city: "Moscow");
            await BlockSeedHelper.AddUnapprovedBlockAsync(db, 404, city: "Moscow");
        }

        var client = ApiTestHelper.CreateClient(_factory);

        var response = await client.GetAsync("/api/unauthorizedpoint");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var list = await ApiTestHelper.ReadAsAsync<PagedBlocksResponseDTO>(response);
        Assert.Contains(list.Items, x => x.Id == 403);
        Assert.DoesNotContain(list.Items, x => x.Id == 404);
    }

    [Fact]
    public async Task Public_Can_Filter_Points_By_City()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await BlockSeedHelper.AddApprovedBlockAsync(db, 405, city: "Moscow");
            await BlockSeedHelper.AddApprovedBlockAsync(db, 406, city: "Kazan");
        }

        var client = ApiTestHelper.CreateClient(_factory);

        var response = await client.GetAsync("/api/unauthorizedpoint?city=Moscow");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var list = await ApiTestHelper.ReadAsAsync<PagedBlocksResponseDTO>(response);
        Assert.All(list.Items, x => Assert.Equal("Moscow", x.City));
    }

    [Fact]
    public async Task Public_Can_Search_Points_By_Title_Or_Address()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await BlockSeedHelper.AddApprovedBlockAsync(db, 407, city: "Moscow");
            await BlockSeedHelper.AddApprovedBlockAsync(db, 408, city: "Moscow");
        }

        var client = ApiTestHelper.CreateClient(_factory);

        var byTitleResponse = await client.GetAsync("/api/unauthorizedpoint?search=Block 407");
        Assert.Equal(HttpStatusCode.OK, byTitleResponse.StatusCode);

        var byTitleList = await ApiTestHelper.ReadAsAsync<PagedBlocksResponseDTO>(byTitleResponse);
        Assert.Contains(byTitleList.Items, x => x.Id == 407);
        Assert.DoesNotContain(byTitleList.Items, x => x.Id == 408);

        var byAddressResponse = await client.GetAsync("/api/unauthorizedpoint?search=Address 408");
        Assert.Equal(HttpStatusCode.OK, byAddressResponse.StatusCode);

        var byAddressList = await ApiTestHelper.ReadAsAsync<PagedBlocksResponseDTO>(byAddressResponse);
        Assert.Contains(byAddressList.Items, x => x.Id == 408);
        Assert.DoesNotContain(byAddressList.Items, x => x.Id == 407);
    }
}
