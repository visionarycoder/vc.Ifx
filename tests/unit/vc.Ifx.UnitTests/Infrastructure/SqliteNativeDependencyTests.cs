using Microsoft.EntityFrameworkCore;

namespace VisionaryCoder.Framework.Tests.Infrastructure;

[TestClass]
public sealed class SqliteNativeDependencyTests
{
    [TestMethod]
    public void SqliteProviderLoadsPatchedNativeLibraryAndExecutesRelationalQuery()
    {
        var options = new DbContextOptionsBuilder().UseSqlite("Data Source=:memory:").Options;
        using var context = new DbContext(options);
        context.Database.OpenConnection();

        var version = context.Database.SqlQueryRaw<string>("SELECT sqlite_version() AS Value").Single();
        Assert.IsTrue(Version.Parse(version) >= new Version(3, 50, 2));
        Assert.AreEqual(42, context.Database.SqlQueryRaw<int>("SELECT 6 * 7 AS Value").Single());
    }
}
