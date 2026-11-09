using System.Text.Json;
using Ifx.Filtering;
using Ifx.Querying;

namespace Ifx.Tests.Filtering;

[TestClass]
public sealed class SharedSpecificationTests
{
    [TestMethod]
    public void FilterSpec_PortableSnapshot_PreservesPredicateMeaning()
    {
        int minimum = 2;
        var current = new FilterSpec<Row>(row => row.Id >= minimum && row.Name.StartsWith("a"));
        string wire = JsonSerializer.Serialize(current.ToFilterNode());
        minimum = 100;
        FilterNode restored = JsonSerializer.Deserialize<FilterNode>(wire)!;
        Row[] rows = [new(1, "alpha"), new(2, "alpha"), new(3, "beta")];
        FilterSpec<Row>.FromFilterNode(restored).Apply(rows).Should().Equal(rows[1]);
        wire.Should().NotContain("Expression").And.NotContain("IQueryable");
    }

    [TestMethod]
    public void QuerySpec_ReusedImmutableShape_PreservesBranchesAndPrimaryOrdering()
    {
        Row[] rows = [new(3, "beta"), new(1, "alpha"), new(2, "alpha")];
        var original = new QuerySpec<Row>().Where(row => row.Id > 0).OrderBy(row => row.Name).ThenBy(row => row.Id);
        var branch = original.OrderByDescending(row => row.Id).Page(0, 1);
        branch.Apply(rows.AsQueryable()).Select(row => row.Id).Should().Equal(3);
        original.Apply(rows.AsQueryable()).Select(row => row.Id).Should().Equal(1, 2, 3);
        original.Apply(rows.AsQueryable()).Select(row => row.Id).Should().Equal(1, 2, 3);
    }

    private sealed record Row(int Id, string Name);

    [TestMethod]
    public void RuntimeApi_ExportedTypes_ContainsOnlyCanonicalSpecifications()
    {
        Type[] exported = typeof(FilterSpec<>).Assembly.GetExportedTypes();
        exported.Should().NotContain(type => type.Namespace != null && type.Namespace.StartsWith("vc.Ifx.FilterSpec", StringComparison.Ordinal));
        exported.Where(type => type.Name == "FilterSpec`1").Should().ContainSingle().Which.Namespace.Should().Be("Ifx.Filtering");
        exported.Where(type => type.Name == "QuerySpec`1").Should().ContainSingle().Which.Namespace.Should().Be("Ifx.Querying");
        exported.Should().NotContain(type => type.Name == "QueryPlan`1");
    }

    [TestMethod]
    public void QuerySpec_GroupedClause_PreservesParenthesizedMeaning()
    {
        Row[] rows = [new(1, "alpha"), new(2, "beta"), new(3, "beta"), new(4, "gamma")];
        var shape = new QuerySpec<Row>().Where(row => row.Id == 1)
            .Or(group => group.Where(row => row.Name == "beta").And(row => row.Id > 2));
        shape.Apply(rows.AsQueryable()).Select(row => row.Id).Should().Equal(1, 3);
    }
}
