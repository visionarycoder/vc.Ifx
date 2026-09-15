using Ifx.Analyzers.Abstractions;

namespace Ifx.Tests.Analyzers.Abstractions;

[TestClass]
public sealed class AbstractionsAssemblySmokeTests
{
    [TestMethod]
    public void AbstractionsAssemblyLoadsAndExposesPublishedContracts()
    {
        var assembly = typeof(ProjectType).Assembly;

        assembly.GetName().Name.Should().Be("Ifx.Analyzers.Abstractions");
        typeof(DiagnosticIds).Assembly.Should().BeSameAs(assembly);
        Enum.GetValues<ProjectType>().Should().Contain(ProjectType.Infrastructure);
        Enum.GetValues<Layer>().Should().Contain(Layer.Client);
        Enum.GetValues<VolatilityLevel>().Should().Contain(VolatilityLevel.VeryVolatile);
    }
}
