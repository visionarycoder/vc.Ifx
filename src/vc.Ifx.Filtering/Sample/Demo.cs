using Microsoft.EntityFrameworkCore;
using VisionaryCoder.Framework.Filtering.EFCore;
using VisionaryCoder.Framework.Filtering.Poco;
using VisionaryCoder.Framework.Filtering.Abstractions;

namespace VisionaryCoder.Framework.Filtering.Sample;

public static class Demo
{
    public static async Task Run()
    {
        // 1) Build a reusable filter:
        //    Users whose name contains "Smith" AND have at least one Order with Total > 1000
        FilterNode filter = Filter.For<User>()
            .Where(u => u.Name.Contains("Smith"))
            .Where(u => u.Orders.Any(o => o.Total > 1000))
            .Build();

        // 2) Use with in-memory POCOs via PocoFilterExecutionStrategy
        var users = new List<User>
        {
            new User { Id = 1, Name = "John Smith", Age = 40, Orders = new List<Order> { new Order { Id = 1, Total = 1500 } } },
            new User { Id = 2, Name = "Ann Smith", Age = 30, Orders = new List<Order> { new Order { Id = 2, Total = 200 } } },
            new User { Id = 3, Name = "Bob Brown", Age = 50, Orders = new List<Order> { new Order { Id = 3, Total = 2000 } } }
        };

        var pocoStrategy = new PocoFilterExecutionStrategy();
        var pocoService = new UserService(pocoStrategy);
        IEnumerable<User> matchedPoco = pocoService.Query(users, filter);
        Console.WriteLine("POCO matches:");
        foreach (var u in matchedPoco)
            Console.WriteLine($" - {u.Name} (Id={u.Id})");

        // 3) Use with EF Core via EfFilterExecutionStrategy
        //    (the same FilterNode is applied to an EF queryable)
        using var db = new AppDbContext();
        // seed
        if (!db.Users.Any())
        {
            db.Users.AddRange(users);
            await db.SaveChangesAsync();
        }

        var efStrategy = new EfFilterExecutionStrategy(db);
        var efService = new UserService(efStrategy);

        IQueryable<User> efQuery = efService.Query(db.Users.AsQueryable(), filter);
        List<User> matchedEf = await efQuery.ToListAsync();

        Console.WriteLine("EF Core matches:");
        foreach (var u in matchedEf)
            Console.WriteLine($" - {u.Name} (Id={u.Id})");

        // --- NEW: Examples showing IN support ---

        // Example A: constant collection variable used as left-side .Contains -> translated to IN
        var allowedNames = new[] { "John Smith", "Bob Brown" };
        FilterNode inFilterVariable = Filter.For<User>()
            .Where(u => allowedNames.Contains(u.Name))
            .Build();

        var pocoMatchesInVar = pocoService.Query(users, inFilterVariable);
        Console.WriteLine("POCO IN (variable) matches:");
        foreach (var u in pocoMatchesInVar)
            Console.WriteLine($" - {u.Name} (Id={u.Id})");

        IQueryable<User> efInQueryVar = efService.Query(db.Users.AsQueryable(), inFilterVariable);
        var efInVarMatches = await efInQueryVar.ToListAsync();
        Console.WriteLine("EF IN (variable) matches:");
        foreach (var u in efInVarMatches)
            Console.WriteLine($" - {u.Name} (Id={u.Id})");

        // Example B: array literal left-side .Contains -> also translated to IN
        FilterNode inFilterLiteral = Filter.For<User>()
            .Where(u => new[] { "Ann Smith", "Bob Brown" }.Contains(u.Name))
            .Build();

        var pocoMatchesInLit = pocoService.Query(users, inFilterLiteral);
        Console.WriteLine("POCO IN (literal) matches:");
        foreach (var u in pocoMatchesInLit)
            Console.WriteLine($" - {u.Name} (Id={u.Id})");

        IQueryable<User> efInQueryLit = efService.Query(db.Users.AsQueryable(), inFilterLiteral);
        var efInLitMatches = await efInQueryLit.ToListAsync();
        Console.WriteLine("EF IN (literal) matches:");
        foreach (var u in efInLitMatches)
            Console.WriteLine($" - {u.Name} (Id={u.Id})");
    }
}

