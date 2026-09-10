using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;
using vc.Ifx.Analyzers.Rules.Diagnostics;
using vc.Ifx.CodeFixes.Providers.Diagnostics;
using vc.Ifx.Roslyn;

namespace VisionaryCoder.Framework.Tests.Roslyn.SharedContracts;

[TestClass]
public sealed class DiagnosticContractTests
{
    [TestMethod]
    public void CatalogContainsOnlyStableOwnedPoliciesInIdOrder()
    {
        ImmutableArray<DiagnosticDescriptor> catalog = DiagnosticDescriptors.All;

        CollectionAssert.AreEqual(new[] { "IFX1000", "IFX1001" }, catalog.Select(rule => rule.Id).ToArray());
        Assert.AreEqual("IFX1000", DiagnosticIdentifiers.DiagnosticReferenceRequiresDisposition);
        Assert.AreEqual("IFX1001", DiagnosticIdentifiers.DiagnosticSuppressionRequiresJustification);
        Assert.AreEqual("Maintainability", DiagnosticCategories.Maintainability);
        Assert.AreEqual("DiagnosticId", DiagnosticPropertyNames.DiagnosticId);
        Assert.AreSame(DiagnosticDescriptors.DiagnosticReferenceRequiresDisposition, catalog[0]);
        Assert.AreSame(DiagnosticDescriptors.DiagnosticSuppressionRequiresJustification, catalog[1]);
        Assert.AreEqual(0, catalog.Clear().Length);
        Assert.AreEqual(2, DiagnosticDescriptors.All.Length);
    }

    [TestMethod]
    [DataRow("IFX1000", "Diagnostic reference requires disposition", "Diagnostic '{0}' is referenced without an explicit disposition", DiagnosticSeverity.Info)]
    [DataRow("IFX1001", "Diagnostic suppression requires justification", "Suppression for diagnostic '{0}' requires a justification", DiagnosticSeverity.Warning)]
    public void DescriptorMetadataMatchesCatalog(string id, string title, string message, DiagnosticSeverity severity)
    {
        DiagnosticDescriptor descriptor = DiagnosticDescriptors.All.Single(rule => rule.Id == id);

        Assert.AreEqual(title, descriptor.Title.ToString(CultureInfo.InvariantCulture));
        Assert.AreEqual(message, descriptor.MessageFormat.ToString(CultureInfo.InvariantCulture));
        Assert.AreEqual(severity, descriptor.DefaultSeverity);
        Assert.AreEqual("Maintainability", descriptor.Category);
        Assert.IsTrue(descriptor.IsEnabledByDefault);
        Assert.IsFalse(string.IsNullOrWhiteSpace(descriptor.Description.ToString(CultureInfo.InvariantCulture)));
        Assert.AreEqual($"https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#{id.ToLowerInvariant()}", descriptor.HelpLinkUri);
        Assert.AreEqual(0, descriptor.CustomTags.Count());
    }

    [TestMethod]
    public void SharedDescriptorsPreserveExistingAnalyzerAndFixContracts()
    {
        ImmutableArray<DiagnosticDescriptor> existing = new DiagnosticDebtAnalyzer().SupportedDiagnostics;
        ImmutableArray<string> fixableIds = new DiagnosticDebtCodeFixProvider().FixableDiagnosticIds;

        foreach (DiagnosticDescriptor descriptor in DiagnosticDescriptors.All)
        {
            DiagnosticDescriptor consumer = existing.Single(rule => rule.Id == descriptor.Id);
            Assert.AreEqual(consumer.Title, descriptor.Title);
            Assert.AreEqual(consumer.MessageFormat, descriptor.MessageFormat);
            Assert.AreEqual(consumer.Category, descriptor.Category);
            Assert.AreEqual(consumer.DefaultSeverity, descriptor.DefaultSeverity);
            Assert.AreEqual(consumer.IsEnabledByDefault, descriptor.IsEnabledByDefault);
            Assert.AreEqual(consumer.Description, descriptor.Description);
            CollectionAssert.AreEqual(consumer.CustomTags.ToArray(), descriptor.CustomTags.ToArray());
            Assert.IsTrue(fixableIds.Contains(descriptor.Id));
        }
    }

    [TestMethod]
    [DataRow("CA1822")]
    [DataRow("CS0168")]
    [DataRow("IFX1001")]
    public void DiagnosticPropertiesCarryReferencedIdWithoutChangingEmittedId(string referencedId)
    {
        foreach (DiagnosticDescriptor descriptor in DiagnosticDescriptors.All)
        {
            Diagnostic diagnostic = Diagnostic.Create(descriptor, Location.None,
                ImmutableDictionary<string, string?>.Empty.Add(DiagnosticPropertyNames.DiagnosticId, referencedId), referencedId);

            Assert.AreEqual(descriptor.Id, diagnostic.Id);
            Assert.AreEqual(referencedId, diagnostic.Properties["DiagnosticId"]);
            Assert.AreEqual(string.Format(CultureInfo.InvariantCulture, descriptor.MessageFormat.ToString(CultureInfo.InvariantCulture), referencedId),
                diagnostic.GetMessage(CultureInfo.InvariantCulture));
            Assert.AreEqual(descriptor.DefaultSeverity, diagnostic.Severity);
        }
    }
}
