using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using VisionaryCoder.Framework.Filtering;
using VisionaryCoder.Framework.Filtering.Abstractions;
using VisionaryCoder.Framework.Filtering.Poco;

namespace VisionaryCoder.Framework.Tests.Filtering;

[TestClass]
public sealed class FilterContractTests
{
    public sealed record Item(int Age, string? Name, bool Active, int? Optional, decimal Amount, double Ratio, List<Item> Children, string[] Tags);

    private static Item[] Items() =>
    [
        new(10, "Alpha", true, null, 1.25m, double.NaN, [], []),
        new(20, "Beta", false, 3, 2.50m, 2, [new(5, "Child", true, 1, 0, 0, [], ["tag"])], ["tag", "second"]),
        new(30, null, true, 7, 4.00m, 9, [], ["other"])
    ];

    public static IEnumerable<object[]> Predicates()
    {
        Expression<Func<Item, bool>>[] expressions =
        [
            item => item.Age == 20, item => item.Age != 20,
            item => item.Age > 20, item => item.Age >= 20,
            item => item.Age < 20, item => item.Age <= 20,
            item => 20 == item.Age, item => 20 != item.Age,
            item => 20 > item.Age, item => 20 >= item.Age,
            item => 20 < item.Age, item => 20 <= item.Age,
            item => !(item.Age == 20), item => !(item.Age != 20),
            item => !(item.Age > 20), item => !(item.Age >= 20),
            item => !(item.Age < 20), item => !(item.Age <= 20),
            item => item.Active, item => !item.Active,
            item => true, item => false,
            item => item.Age > 10 && (item.Active || item.Name == "Beta"),
            item => !(item.Age > 10 && item.Active),
            item => item.Optional == null, item => item.Optional != null,
            item => item.Optional >= 3, item => !(item.Optional >= 3),
            item => !(item.Ratio > 2), item => item.Ratio < 3,
            item => item.Name != null && item.Name.Contains("a"),
            item => item.Name != null && item.Name.StartsWith("Al"),
            item => item.Name != null && !item.Name.EndsWith("ta"),
            item => item.Children.Any(), item => item.Children.Any(child => child.Active),
            item => item.Children.All(child => child.Age > 1),
            item => item.Children.Any(child => child.Tags.Any(tag => tag == "tag")),
            item => item.Tags.Any(tag => tag.StartsWith("t")),
            item => item.Tags.Contains("tag"),
            item => new[] { 10, 30 }.Contains(item.Age),
            item => new string?[] { "Alpha", null }.Contains(item.Name),
            item => item.Amount > 2.25m
        ];
        return expressions.Select(expression => new object[] { expression });
    }

    [TestMethod]
    [DynamicData(nameof(Predicates), DynamicDataSourceType.Method)]
    public void PortableRoundtripMatchesClrPredicate(Expression<Func<Item, bool>> predicate)
    {
        FilterNode node = ExpressionToFilterNode.Translate(predicate);
        string json = JsonSerializer.Serialize(node);
        FilterNode restored = JsonSerializer.Deserialize<FilterNode>(json)!;
        Item[] items = Items();
        Item[] expected = items.Where(predicate.Compile()).ToArray();
        var strategy = new PocoFilterExecutionStrategy();
        strategy.Apply((IEnumerable<Item>)items, restored).Should().Equal(expected);
        strategy.Apply(items.AsQueryable(), restored).Should().Equal(expected);
    }

    [TestMethod]
    public void CapturesAreSnapshottedInvariantly()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            decimal minimum = 1.25m;
            Expression<Func<Item, bool>> predicate = item => item.Amount > minimum;
            var node = (FilterCondition)ExpressionToFilterNode.Translate(predicate);
            minimum = 100m;
            node.Value.Should().Be("1.25");
            new PocoFilterExecutionStrategy().Apply(Items(), node).Should().HaveCount(2);
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [TestMethod]
    public void CapturedListTranslatesToMembership()
    {
        var ages = new List<int> { 10, 30 };
        FilterNode node = ExpressionToFilterNode.Translate<Item>(item => ages.Contains(item.Age));
        ages.Clear();
        new PocoFilterExecutionStrategy().Apply(Items(), node).Should().HaveCount(2);
    }

    [TestMethod]
    public void BuilderAndGroupSnapshotLists()
    {
        FilterBuilder<Item> builder = Filter.For<Item>().Where(item => item.Active).Where(item => item.Age > 5);
        FilterNode first = builder.Build();
        builder.Where(item => false);
        new PocoFilterExecutionStrategy().Apply(Items(), first).Should().HaveCount(2);
        new PocoFilterExecutionStrategy().Apply(Items(), builder.Build()).Should().BeEmpty();
        var children = new List<FilterNode> { new FilterConstant(true) };
        var group = new FilterGroup(FilterCombination.And, children);
        FilterGroup copied = group with { Children = children };
        children.Clear();
        group.Children.Should().HaveCount(1);
        copied.Children.Should().HaveCount(1);
        Action mutate = () => ((IList<FilterNode>)group.Children).Clear();
        mutate.Should().Throw<NotSupportedException>();
        Action nullChildren = () => _ = group with { Children = null! };
        nullChildren.Should().Throw<ArgumentNullException>();
        Action nullConstructor = () => _ = new FilterGroup(FilterCombination.And, null!);
        nullConstructor.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void EmptyGroupsAndMembershipHaveBooleanIdentity()
    {
        var strategy = new PocoFilterExecutionStrategy();
        strategy.Apply(Items(), Filter.For<Item>().Build()).Should().HaveCount(3);
        strategy.Apply(Items(), new FilterGroup(FilterCombination.Or, [])).Should().BeEmpty();
        strategy.Apply(Items(), new FilterCondition("Age", FilterOperation.In, "[]")).Should().BeEmpty();
        Filter.For<Item>().Where(item => item.Active).Build().Should().BeOfType<FilterCondition>();
    }

    [TestMethod]
    public void SpecificationsComposeWithoutMutationOrInvocation()
    {
        var active = new FilterSpec<Item>(item => item.Active);
        FilterSpec<Item> older = active.Where(item => item.Age > 15);
        FilterSpec<Item> union = older.Or(new(item => item.Age == 20));
        active.Apply((IEnumerable<Item>)Items()).Should().HaveCount(2);
        older.Apply(Items().AsQueryable()).Should().ContainSingle().Which.Age.Should().Be(30);
        union.Apply(Items().AsQueryable()).Should().HaveCount(2);
        union.Not().Apply((IEnumerable<Item>)Items()).Should().ContainSingle().Which.Age.Should().Be(10);
        FilterSpec<Item>.All.Apply(Items().AsQueryable()).Should().HaveCount(3);
        union.Predicate.ToString().Should().NotContain("Invoke");
        new PocoFilterExecutionStrategy().Apply(Items(), union.ToFilterNode()).Should().HaveCount(2);
        FilterSpec<Item> nested = active.And(new(item => item.Children.Any(child => child.Active)));
        nested.Apply(Items().AsQueryable()).Should().BeEmpty();
    }

    [TestMethod]
    public void NullArgumentsAreRejectedAndNullFilterPreservesSource()
    {
        Action[] invalid =
        [
            () => _ = new FilterSpec<Item>(null!),
            () => FilterSpec<Item>.All.And(null!),
            () => FilterSpec<Item>.All.Or(null!),
            () => FilterSpec<Item>.All.Where(null!),
            () => FilterSpec<Item>.All.Apply((IEnumerable<Item>)null!),
            () => FilterSpec<Item>.All.Apply((IQueryable<Item>)null!),
            () => ExpressionToFilterNode.Translate((Expression)null!),
            () => ExpressionToFilterNode.Translate((Expression<Func<Item, bool>>)null!),
            () => new PocoFilterExecutionStrategy().Apply((IEnumerable<Item>)null!, null),
            () => new PocoFilterExecutionStrategy().Apply((IQueryable<Item>)null!, null)
        ];
        foreach (Action action in invalid) action.Should().Throw<ArgumentNullException>();
        IEnumerable<Item> source = Items();
        IQueryable<Item> query = source.AsQueryable();
        new PocoFilterExecutionStrategy().Apply(source, null).Should().BeSameAs(source);
        new PocoFilterExecutionStrategy().Apply(query, null).Should().BeSameAs(query);
        FilterExpression.Create<Item>(null).Compile()(Items()[0]).Should().BeTrue();
    }

    [TestMethod]
    public void UnsupportedSyntaxIsNeverDroppedFromAGroup()
    {
        Expression<Func<Item, bool>>[] invalid =
        [
            item => item.Active && item.Name!.GetHashCode() > 0,
            item => item.Active || item.Name!.GetHashCode() > 0,
            item => item.Children.Any(child => child.Age > item.Age),
            item => item.Name!.Contains("A", StringComparison.OrdinalIgnoreCase),
            item => item.Tags.Contains("a", StringComparer.OrdinalIgnoreCase),
            item => item.Age == item.Optional,
            item => (byte)item.Age == 10,
            item => item.Name!.Equals("A"),
            item => item.Age % 2 == 0,
            item => item.Name!.IsNormalized()
        ];
        foreach (var expression in invalid)
        {
            Action translate = () => ExpressionToFilterNode.Translate(expression);
            translate.Should().Throw<NotSupportedException>();
        }
    }

    [TestMethod]
    public void MalformedNodesFailBeforeEnumeration()
    {
        FilterNode[] invalid =
        [
            new FilterCondition("Missing", FilterOperation.Equals, "1"),
            new FilterCondition("Age", FilterOperation.Equals, "wrong"),
            new FilterCondition("Age", FilterOperation.Equals, null),
            new FilterCondition("Age", (FilterOperation)99, "1"),
            new FilterCondition("Age", FilterOperation.In, "[oops"),
            new FilterCondition("Age", FilterOperation.In, "null"),
            new FilterCondition("Age", FilterOperation.In, null),
            new FilterCondition("Name", FilterOperation.Contains, null),
            new FilterCondition("Age", FilterOperation.StartsWith, "1"),
            new FilterCondition("Age", FilterOperation.Contains, "1"),
            new FilterCollectionCondition("Name", FilterOperation.Any, new FilterConstant(true)),
            new FilterCollectionCondition("Children", FilterOperation.Any, null),
            new FilterCollectionCondition("Children", FilterOperation.HasElements, new FilterConstant(true)),
            new FilterCollectionCondition("Children", FilterOperation.In, null),
            new FilterGroup((FilterCombination)99, []),
            new FilterNegation(null!),
            new FilterCondition("", FilterOperation.Equals, "a")
        ];
        foreach (FilterNode node in invalid)
        {
            Action build = () => new PocoFilterExecutionStrategy().Apply(Items(), node);
            build.Should().Throw<Exception>();
        }
    }

    public sealed class Values
    {
        public Guid Id { get; init; }
        public DateTime Timestamp { get; init; }
        public DateTimeOffset Offset { get; init; }
        public DateOnly Date { get; init; }
        public TimeOnly Time { get; init; }
        public TimeSpan Duration { get; init; }
        public DayOfWeek Day { get; init; }
        public short Small { get; init; }
        public long Large { get; init; }
        public float Single { get; init; }
        public bool Flag;
        public object? Untyped { get; init; }
        public string this[int index] => index.ToString(CultureInfo.InvariantCulture);
        public IQueryable<int> Query { get; init; } = new[] { 1, 2 }.AsQueryable();
        public List<int> List { get; init; } = [1, 2];
        public Custom Number { get; init; }
    }

    [TestMethod]
    public void TypedValuesAndPublicFieldsPreserveInvariantValues()
    {
        var values = new Values
        {
            Id = Guid.Parse("aa36af90-4c17-46b7-9686-9d4d55eb09da"),
            Timestamp = new DateTime(2026, 9, 9, 10, 30, 0, DateTimeKind.Utc),
            Offset = new DateTimeOffset(2026, 9, 9, 10, 30, 0, TimeSpan.FromHours(2)),
            Date = new DateOnly(2026, 9, 9), Time = new TimeOnly(10, 30),
            Duration = TimeSpan.FromMinutes(5), Day = DayOfWeek.Wednesday,
            Small = 3, Large = 12345678900, Single = 1.25f, Flag = true
        };
        (string path, string text)[] comparisons =
        [
            ("Id", values.Id.ToString()), ("Timestamp", values.Timestamp.ToString("O")),
            ("Offset", values.Offset.ToString("O")), ("Date", "2026-09-09"),
            ("Time", "10:30:00"), ("Duration", "00:05:00"), ("Day", "Wednesday"),
            ("Small", "3"), ("Large", "12345678900"), ("Single", "1.25"), ("flag", "True")
        ];
        foreach ((string path, string text) in comparisons)
            FilterExpression.Create<Values>(new FilterCondition(path, FilterOperation.Equals, text)).Compile()(values).Should().BeTrue();
        foreach (FilterOperation operation in new[] { FilterOperation.GreaterThan, FilterOperation.GreaterOrEqual, FilterOperation.LessThan, FilterOperation.LessOrEqual })
        {
            bool expected = operation is FilterOperation.GreaterThan or FilterOperation.GreaterOrEqual;
            FilterExpression.Create<Values>(new FilterCondition("Day", operation, "Monday")).Compile()(values).Should().Be(expected);
        }
        FilterExpression.Create<Values>(ExpressionToFilterNode.Translate<Values>(item => item.Timestamp == values.Timestamp)).Compile()(values).Should().BeTrue();
        FilterExpression.Create<Values>(ExpressionToFilterNode.Translate<Values>(item => item.Offset == values.Offset)).Compile()(values).Should().BeTrue();
        FilterExpression.Create<Values>(ExpressionToFilterNode.Translate<Values>(item => item.Id == values.Id)).Compile()(values).Should().BeTrue();
        Action indexer = () => FilterExpression.Create<Values>(new FilterCondition("Item", FilterOperation.Equals, "1"));
        indexer.Should().Throw<ArgumentException>();
        Action nonCollection = () => FilterExpression.Create<Values>(new FilterCollectionCondition("Untyped", FilterOperation.Any, new FilterConstant(true)));
        nonCollection.Should().Throw<NotSupportedException>();
    }

    [TestMethod]
    public void UntypedExpressionEntryPointsValidateShape()
    {
        Expression<Func<Item, bool>> predicate = item => item.Active;
        ExpressionToFilterNode.Translate((Expression)predicate).Should().BeOfType<FilterCondition>();
        ExpressionToFilterNode.Translate(predicate.Body).Should().BeOfType<FilterCondition>();
        ExpressionToFilterNode.Translate(Expression.Constant(true)).Should().Be(new FilterConstant(true));
        Action nonBoolean = () => ExpressionToFilterNode.Translate(Expression.Constant(1));
        nonBoolean.Should().Throw<NotSupportedException>();
        Action multipleParameters = () => ExpressionToFilterNode.Translate((Expression)(Expression<Func<int, int, bool>>)((a, b) => a > b));
        multipleParameters.Should().Throw<NotSupportedException>();
        Action noParameters = () => ExpressionToFilterNode.Translate((Expression)(Expression<Func<bool>>)(() => true));
        noParameters.Should().Throw<NotSupportedException>();
    }

    [TestMethod]
    public void NullableMembershipAndExplicitNullGuardsWork()
    {
        var node = new FilterCondition("Optional", FilterOperation.In, "[null,\"3\"]");
        new PocoFilterExecutionStrategy().Apply(Items(), node).Select(item => item.Age).Should().Equal(10, 20);
        Item nullable = Items()[0] with { Name = null, Tags = null! };
        Expression<Func<Item, bool>> guarded = item => item.Name != null && item.Name.Contains("a");
        FilterExpression.Create<Item>(ExpressionToFilterNode.Translate(guarded)).Compile()(nullable).Should().BeFalse();
        Action unguarded = () => FilterExpression.Create<Item>(new FilterCondition("Name", FilterOperation.Contains, "a")).Compile()(nullable);
        unguarded.Should().Throw<NullReferenceException>();
    }

    public readonly record struct Custom(int Value)
    {
        public static bool operator >(Custom left, Custom right) => left.Value > right.Value;
        public static bool operator <(Custom left, Custom right) => left.Value < right.Value;
    }

    private sealed record UnknownFilter : FilterNode;

    public static int StaticMinimum => 15;
    public static readonly bool StaticFlag = true;

    [TestMethod]
    public void QueryableLambdasAndInstanceCollectionsTranslate()
    {
        var value = new Values();
        Expression<Func<Values, bool>>[] expressions =
        [
            item => item.Query.Any(number => number > 1),
            item => item.Query.All(number => number > 0),
            item => item.List.Contains(2)
        ];
        foreach (var expression in expressions)
            FilterExpression.Create<Values>(ExpressionToFilterNode.Translate(expression)).Compile()(value).Should().BeTrue();
    }

    [TestMethod]
    public void ConstantConversionsAndStaticMembersAreEvaluatedWithoutCompiling()
    {
        int minimum = 15;
        int? absent = null;
        var value = new Values { Large = 20, Day = DayOfWeek.Monday };
        Expression<Func<Values, bool>> wide = item => item.Large > (long)minimum;
        FilterExpression.Create<Values>(ExpressionToFilterNode.Translate(wide)).Compile()(value).Should().BeTrue();
        int day = 1;
        Expression<Func<Values, bool>> enumeration = item => item.Day == (DayOfWeek)day;
        FilterExpression.Create<Values>(ExpressionToFilterNode.Translate(enumeration)).Compile()(value).Should().BeTrue();
        Expression<Func<Item, bool>> nullable = item => item.Optional == (int?)absent;
        FilterExpression.Create<Item>(ExpressionToFilterNode.Translate(nullable)).Compile()(Items()[0]).Should().BeTrue();
        Expression<Func<Item, bool>> statics = item => item.Age > StaticMinimum && StaticFlag;
        new PocoFilterExecutionStrategy().Apply(Items(), ExpressionToFilterNode.Translate(statics)).Should().HaveCount(2);
        Expression<Func<Item, bool>> nullConversion = item => item.Name == (string?)(object?)null;
        new PocoFilterExecutionStrategy().Apply(Items(), ExpressionToFilterNode.Translate(nullConversion)).Should().ContainSingle();
    }

    [TestMethod]
    public void UnsupportedComparisonsAndCollectionSourcesAreRejected()
    {
        Func<Item, bool> callback = item => item.Active;
        IEnumerable<int>? absent = null;
        var set = new HashSet<int> { 10 };
        Expression<Func<Item, bool>>[] invalid =
        [
            item => item.Active & true,
            item => true == false,
            item => "constant".Contains(item.Name!),
            item => new List<Item>().Any(child => child.Active),
            item => item.Children.Any(callback),
            item => Enumerable.Contains(absent!, item.Age),
            item => new List<int> { 1 }.Contains(1),
            item => set.Contains(item.Age)
        ];
        // A folded Boolean constant is valid; all other entries exercise rejected syntax.
        foreach (var expression in invalid.Where(expression => expression.Body is not ConstantExpression))
        {
            Action translate = () => ExpressionToFilterNode.Translate(expression);
            translate.Should().Throw<NotSupportedException>();
        }
        var threshold = new Custom(1);
        Action custom = () => ExpressionToFilterNode.Translate<Values>(item => item.Number > threshold);
        custom.Should().Throw<NotSupportedException>();
        Action unknown = () => FilterExpression.Create<Item>(new UnknownFilter());
        unknown.Should().Throw<NotSupportedException>();
    }

    [TestMethod]
    public void SpanArrayMembershipIsPortable()
    {
        ParameterExpression item = Expression.Parameter(typeof(Item), "item");
        var conversion = typeof(Span<int>).GetMethod("op_Implicit", [typeof(int[])])!;
        var contains = typeof(MemoryExtensions).GetMethods().Single(method =>
            method.Name == "Contains" && method.IsGenericMethodDefinition && method.GetParameters().Length == 2 &&
            method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(Span<>)).MakeGenericMethod(typeof(int));
        Expression source = Expression.Call(conversion, Expression.Constant(new[] { 10, 30 }));
        var expression = Expression.Lambda<Func<Item, bool>>(
            Expression.Call(contains, source, Expression.Property(item, nameof(Item.Age))), item);
        new PocoFilterExecutionStrategy().Apply(Items(), ExpressionToFilterNode.Translate(expression)).Select(value => value.Age).Should().Equal(10, 30);
    }
}
