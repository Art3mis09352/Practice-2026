using Application.Data.DTO.Route.Read;
using System.Net;
using Tests.Integration.Helpers;

namespace Tests.Integration.Routes;

public class RouteLikesIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RouteLikesIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task User_Can_Like_And_Unlike_Route()
    {
        var ownerClient = ApiTestHelper.CreateClient(_factory);
        var likerClient = ApiTestHelper.CreateClient(_factory);

        var (_, owner) = await ApiTestHelper.RegisterUserAsync(ownerClient);
        var (_, liker) = await ApiTestHelper.RegisterUserAsync(likerClient);

        await ApiTestHelper.AuthenticateAsUserAsync(ownerClient, owner.Email, owner.Password);
        await ApiTestHelper.AuthenticateAsUserAsync(likerClient, liker.Email, liker.Password);

        var route = await ApiTestHelper.CreateRouteAndReadAsync(ownerClient, new
        {
            title = "Liked route",
            description = "Route for likes",
            startDate = "2026-05-20",
            endDate = "2026-05-21",
            budget = 1000,
            isPublic = true,
            days = new[]
            {
                new
                {
                    dayNumber = 1,
                    title = "Day 1",
                    notes = "Like me",
                    blocks = Array.Empty<object>()
                }
            }
        });

        var likeResponse = await likerClient.PostAsync($"/api/routes/{route.Id}/like", content: null);
        Assert.Equal(HttpStatusCode.NoContent, likeResponse.StatusCode);

        var likedResponse = await likerClient.GetAsync("/api/routes/liked?search=Liked");
        Assert.Equal(HttpStatusCode.OK, likedResponse.StatusCode);

        var likedRoutes = await ApiTestHelper.ReadAsAsync<PagedRoutesResponseDTO>(likedResponse);
        Assert.Contains(likedRoutes.Items, x =>
            x.Id == route.Id &&
            x.Title == "Liked route" &&
            x.IsLikedByCurrentUser &&
            x.LikesCount == 1);

        var unlikeResponse = await likerClient.DeleteAsync($"/api/routes/{route.Id}/like");
        Assert.Equal(HttpStatusCode.NoContent, unlikeResponse.StatusCode);

        var afterUnlikeResponse = await likerClient.GetAsync("/api/routes/liked");
        Assert.Equal(HttpStatusCode.OK, afterUnlikeResponse.StatusCode);

        var afterUnlike = await ApiTestHelper.ReadAsAsync<PagedRoutesResponseDTO>(afterUnlikeResponse);
        Assert.DoesNotContain(afterUnlike.Items, x => x.Id == route.Id);
    }
}
