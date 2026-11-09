using System.Text.Json;
using Ifx.Filtering;
using Ifx.Querying;

namespace Ifx.Tests.Filtering;

[TestClass]
public sealed class FilterSpecBoundaryTests
{
    [TestMethod]
    public void FilterNode_JsonRoundTrip_PreservesQueryMeaning()
    {
        FilterNode node = new FilterGroup(FilterCombination.And,
        [
            new FilterCondition(nameof(Row.Id), FilterOperation.GreaterOrEqual, "2"),
            new FilterNegation(new FilterCondition(nameof(Row.Name), FilterOperation.Equals, "skip"))
        ]);
        string json = JsonSerializer.Serialize(node);
        FilterNode restored = JsonSerializer.Deserialize<FilterNode>(json)!;
        Row[] rows = [new(1, "one"), new(2, "two"), new(3, "skip")];

        Row[] result = QuerySpec<Row>.For(rows.AsQueryable()).ApplyFilter(restored).ToQueryable().ToArray();

        result.Should().Equal(rows[1]);
        json.Should().NotContain("System.Linq.Expressions");
    }

    [TestMethod]
    public void CursorPage_ResumeToken_ReturnsFollowingRows()
    {
        Row[] rows = [new(3, "three"), new(1, "one"), new(2, "two")];
        CursorPage<Row> first = QuerySpec<Row>.For(rows.AsQueryable()).UseKey(row => row.Id).ToCursorPage(2);

        CursorPage<Row> second = QuerySpec<Row>.For(rows.AsQueryable()).UseKey(row => row.Id)
            .AfterCursor(first.NextCursor!).ToCursorPage(2);

        first.Items.Select(row => row.Id).Should().Equal(1, 2);
        second.Items.Select(row => row.Id).Should().Equal(3);
        second.NextCursor.Should().BeNull();
    }

    [TestMethod]
    public void CursorPage_InvalidToken_RejectsMalformedInput()
    {
        Row[] rows = [new(1, "one")];
        Action invalid = () => QuerySpec<Row>.For(rows.AsQueryable()).UseKey(row => row.Id)
            .AfterCursor("invalid token").ToCursorPage(2);

        invalid.Should().Throw<FormatException>();
    }

    [TestMethod]
    public void CoreAssembly_References_ExcludeOptionalProvidersAndCompilerTools()
    {
        string[] references = typeof(FilterNode).Assembly.GetReferencedAssemblies().Select(item => item.Name!).ToArray();

        references.Should().NotContain(name => name.StartsWith("Azure.", StringComparison.Ordinal)
            || name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)
            || name.StartsWith("Microsoft.CodeAnalysis", StringComparison.Ordinal)
            || (name.StartsWith("Ifx.", StringComparison.Ordinal) && name != "Ifx"));
    }

    private sealed record Row(int Id, string Name);

    [TestMethod]
    public void IntegrationAssembly_References_ExcludeAzureAndWindowsDependencies()
    {
        string[] references = typeof(Ifx.Filtering.EntityFrameworkCore.EfFilterExecutionStrategy).Assembly
            .GetReferencedAssemblies().Select(item => item.Name!).ToArray();

        references.Should().NotContain(name => name.StartsWith("Azure.", StringComparison.Ordinal)
            || name.Contains("AzureAppConfiguration", StringComparison.Ordinal)
            || name == "Experimental.System.Messaging"
            || name == "Ifx.Integrations.Azure"
            || name == "Ifx.Integrations.Windows");
    }
}
