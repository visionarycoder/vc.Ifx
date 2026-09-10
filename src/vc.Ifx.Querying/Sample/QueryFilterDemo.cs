using Microsoft.EntityFrameworkCore;

namespace VisionaryCoder.Framework.Querying.Sample;

public static class QueryFilterDemo
{
    public static async Task Run()
    {
        // Build two reusable QueryFilter<User> instances
        var nameContainsSmith = QueryFilterExtensions.ContainsIgnoreCase<User>(u => u.Name, "smith");
        var highValueOrder = new QueryFilter<User>(u => u.Orders.Any(o => o.Total > 1000m));

        // Combine filters: name contains "smith" AND has a high-value order
        var combined = nameContainsSmith.And(highValueOrder);

        // Sample POCO list
        var users = new List<User>
        {
            new User { Id = 1, Name = "John Smith", Orders = new List<Order> { new Order { Id = 1, Total = 1500 } } },
            new User { Id = 2, Name = "Ann Smith", Orders = new List<Order> { new Order { Id = 2, Total = 200 } } },
            new User { Id = 3, Name = "Bob Brown", Orders = new List<Order> { new Order { Id = 3, Total = 2000 } } },
        };

        var service = new UserQueryService();

        // Apply to POCO (in-memory)
        IEnumerable<User> pocoMatches = service.ApplyToEnumerable(users, combined);
        Console.WriteLine("POCO matches:");
        foreach (var u in pocoMatches) Console.WriteLine($" - {u.Name} (Id={u.Id})");

        // Apply to EF Core
        using var db = new AppDbContext();
        if (!db.Users.Any())
        {
            db.Users.AddRange(users);
            await db.SaveChangesAsync();
        }

        IQueryable<User> efQuery = service.ApplyToQueryable(db.Users.AsQueryable(), combined);
        List<User> efMatches = await efQuery.ToListAsync();

        Console.WriteLine("EF Core matches:");
        foreach (var u in efMatches) Console.WriteLine($" - {u.Name} (Id={u.Id})");
    }
}