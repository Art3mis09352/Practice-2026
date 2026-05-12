using Domain.Entities;
using Infrastructure.Data;

namespace Tests.Integration.Helpers;

public static class BlockSeedHelper
{
    public static async Task<Block> AddApprovedBlockAsync(
        AppDbContext db,
        int id,
        string ownerId = "owner-1",
        string city = "Moscow")
    {
        var block = new Block
        {
            Id = id,
            OwnerId = ownerId,
            Title = $"Block {id}",
            Description = $"Description {id}",
            Category = "Food",
            City = city,
            Address = $"Address {id}",
            Latitude = 55.7558m,
            Longitude = 37.6173m,
            AvgPrice = 1000,
            IsApproved = true
        };

        db.Blocks.Add(block);
        await db.SaveChangesAsync();
        return block;
    }

    public static async Task<Block> AddUnapprovedBlockAsync(
        AppDbContext db,
        int id,
        string ownerId = "owner-1",
        string city = "Moscow")
    {
        var block = new Block
        {
            Id = id,
            OwnerId = ownerId,
            Title = $"Block {id}",
            Description = $"Description {id}",
            Category = "Food",
            City = city,
            Address = $"Address {id}",
            Latitude = 55.7558m,
            Longitude = 37.6173m,
            AvgPrice = 1000,
            IsApproved = false
        };

        db.Blocks.Add(block);
        await db.SaveChangesAsync();
        return block;
    }
}
