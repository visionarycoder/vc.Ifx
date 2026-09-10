using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace VisionaryCoder.Framework.Tests.Roslyn.Analyzers.Legacy;

[TestClass]
public sealed class ProjectClassificationTests
{
    [TestMethod]
    [DataRow(null, ProjectType.Unknown)]
    [DataRow(" ", ProjectType.Unknown)]
    [DataRow("Unclassified", ProjectType.Unknown)]
    [DataRow("Access.Data.Contract", ProjectType.Contract)]
    [DataRow("Access.Data.service", ProjectType.Service)]
    [DataRow("Access.Data.Orm", ProjectType.Orm)]
    [DataRow("Access.Data.Orm.Financial", ProjectType.Orm)]
    [DataRow("Ifx.Core", ProjectType.Infrastructure)]
    [DataRow("vc.Ifx.Core", ProjectType.Infrastructure)]
    [DataRow("Client.WebApi", ProjectType.WebApi)]
    [DataRow("Client.AzureFunctions", ProjectType.AzureFunctions)]
    [DataRow("Performance.Benchmarks", ProjectType.Benchmarks)]
    [DataRow("App.Tests", ProjectType.Test)]
    public void ClassifiesProjectNames(string? name, ProjectType expected) => Assert.AreEqual(expected, ProjectAnalyzer.GetProjectType(name!));

    [TestMethod]
    [DataRow(null, Layer.Unknown)]
    [DataRow(" ", Layer.Unknown)]
    [DataRow("Other", Layer.Unknown)]
    [DataRow("access.Data.Contract", Layer.Access)]
    [DataRow("Engine.Data.Service", Layer.Engine)]
    [DataRow("Manager.Data.Service", Layer.Manager)]
    [DataRow("Client.WebApi", Layer.Client)]
    [DataRow("Ifx.Core", Layer.Infrastructure)]
    [DataRow("vc.Ifx.Core", Layer.Infrastructure)]
    public void ClassifiesLayers(string? name, Layer expected) => Assert.AreEqual(expected, ProjectAnalyzer.GetLayer(name!));

    [TestMethod]
    public void VolatilityAndDisplayContractsCoverEveryProjectType()
    {
        var expected = new (ProjectType Type, VolatilityLevel Level, string Name)[]
        {
            (ProjectType.Unknown, VolatilityLevel.Moderate, "Unknown"),
            (ProjectType.Infrastructure, VolatilityLevel.VeryStable, "Infrastructure (Very Stable)"),
            (ProjectType.Contract, VolatilityLevel.Stable, "Contract (Stable)"),
            (ProjectType.Orm, VolatilityLevel.Moderate, "ORM (Moderate)"),
            (ProjectType.Service, VolatilityLevel.Volatile, "Service (Volatile)"),
            (ProjectType.WebApi, VolatilityLevel.VeryVolatile, "Web API (Very Volatile)"),
            (ProjectType.AzureFunctions, VolatilityLevel.VeryVolatile, "Azure Functions (Very Volatile)"),
            (ProjectType.Test, VolatilityLevel.Moderate, "Test"),
            (ProjectType.Benchmarks, VolatilityLevel.Moderate, "Benchmarks")
        };
        foreach (var item in expected)
        {
            Assert.AreEqual(item.Level, ProjectAnalyzer.GetVolatilityLevel(item.Type));
            Assert.AreEqual(item.Name, ProjectAnalyzer.GetProjectTypeName(item.Type));
        }
        foreach (var source in expected)
        foreach (var target in expected)
        {
            bool allowed = source.Type is ProjectType.Test or ProjectType.Benchmarks or ProjectType.Unknown || target.Type == ProjectType.Unknown || source.Level >= target.Level;
            Assert.AreEqual(allowed, ProjectAnalyzer.IsDependencyAllowed(source.Type, target.Type), $"{source.Type} -> {target.Type}");
        }
    }

    [TestMethod]
    public void LayerMatrixMatchesPublishedCompatibilityPolicy()
    {
        // Rows/columns: Unknown, Access, Engine, Manager, Client, Infrastructure.
        string[] rows = ["111111", "010001", "011001", "011101", "011111", "111111"];
        foreach (Layer from in Enum.GetValues<Layer>())
        foreach (Layer to in Enum.GetValues<Layer>())
            Assert.AreEqual(rows[(int)from][(int)to] == '1', ProjectAnalyzer.IsLayerDependencyAllowed(from, to), $"{from} -> {to}");
    }
}
