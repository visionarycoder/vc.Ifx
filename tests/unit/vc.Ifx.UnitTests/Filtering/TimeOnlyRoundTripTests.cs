using System.Linq.Expressions;
using System.Text.Json;
using VisionaryCoder.Framework.Filtering;
using VisionaryCoder.Framework.Filtering.Abstractions;

namespace VisionaryCoder.Framework.Tests.Filtering;

[TestClass]
public sealed class TimeOnlyRoundTripTests
{
    [TestMethod]
    public void MembershipPreservesSecondsFractionalTicksAndNulls()
    {
        TimeOnly precise = new TimeOnly(10, 30, 45, 123).Add(TimeSpan.FromTicks(4567));
        TimeOnly minute = new(10, 30);
        List<TimeOnly> allowed = [precise];
        AssertEquivalent<TimeOnly>(value => allowed.Contains(value), [precise, minute]);
        List<TimeOnly?> nullable = [precise, null];
        AssertEquivalent<TimeOnly?>(value => nullable.Contains(value), [precise, minute, null]);
        AssertEquivalent<TimeRow>(row => row.Times.Contains(precise),
            [new([precise]), new([minute]), new([])]);
        AssertEquivalent<TimeRow>(row => row.NullableTimes.Contains(precise),
            [new([precise]), new([minute]), new([])]);
    }

    private static void AssertEquivalent<T>(Expression<Func<T, bool>> original, T[] values)
    {
        FilterNode node = ExpressionToFilterNode.Translate(original);
        string json = JsonSerializer.Serialize<FilterNode>(node);
        FilterNode restored = JsonSerializer.Deserialize<FilterNode>(json)!;
        Func<T, bool> expected = original.Compile();
        Func<T, bool> portable = FilterExpression.Create<T>(node).Compile();
        Func<T, bool> roundTrip = FilterExpression.Create<T>(restored).Compile();
        foreach (T value in values)
        {
            Assert.AreEqual(expected(value), portable(value), "Portable predicate changed membership.");
            Assert.AreEqual(expected(value), roundTrip(value), "JSON round trip changed membership.");
        }
    }

    private sealed class TimeRow(List<TimeOnly> times)
    {
        public List<TimeOnly> Times { get; } = times;
        public List<TimeOnly?> NullableTimes { get; } = times.Select(value => (TimeOnly?)value).Append(null).ToList();
    }
}
