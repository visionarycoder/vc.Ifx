namespace Ifx.Tests.Web.AspNetCore;

[TestClass]
public sealed class AspNetCoreAssemblySmokeTests
{
    [TestMethod]
    public void AspNetCoreAssemblyLoadsPlaceholderType()
    {
        var instance = new global::Ifx.Web.AspNetCore.Class1();

        instance.Should().NotBeNull();
        instance.GetType().Assembly.GetName().Name.Should().Be("Ifx.Web.AspNetCore");
    }
}
