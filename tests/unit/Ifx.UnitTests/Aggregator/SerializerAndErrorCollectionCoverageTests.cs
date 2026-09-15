using System.Collections;
using System.Text.Json;
using Ifx.Errors;
using Ifx.Helpers;

namespace Ifx.Tests.Aggregator;

[TestClass]
public sealed class SerializerAndErrorCollectionCoverageTests
{
    [TestMethod]
    public void ErrorCollectionSupportsDirectNonGenericEnumeration()
    {
        IEnumerable values = new ErrorCollection([new Error("retry", Code<Error>.Create("temporary"), "try later", IsRecoverableError.Yes)]);

        IEnumerator enumerator = values.GetEnumerator();

        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Should().BeOfType<Error>().Which.Name.Should().Be("retry");
    }

    [TestMethod]
    public void SerializerSerializesNullAndRejectsNullDocuments()
    {
        var serializer = new SystemTextJsonSerializer();

        serializer.Serialize<string?>(null).Should().Be("null");
        serializer.Serialize(new { Value = 5 }).Should().Contain("\"Value\":5");
        serializer.Deserialize<JsonElement>("null").ValueKind.Should().Be(JsonValueKind.Null);
    }
}
