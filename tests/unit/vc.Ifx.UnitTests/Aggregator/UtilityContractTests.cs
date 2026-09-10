using VisionaryCoder.Framework.Extensions;

namespace VisionaryCoder.Framework.Tests.Aggregator;

[TestClass]
public sealed class UtilityContractTests
{
    [TestMethod]
    public void BatchesRemainIndependentAfterOuterEnumeratorIsDisposed()
    {
        IEnumerable<int>[] batches = Enumerable.Range(1, 7).Batch(3).ToArray();
        CollectionAssert.AreEqual(new[] { 7 }, batches[2].ToArray());
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, batches[0].ToArray());
        CollectionAssert.AreEqual(new[] { 4, 5, 6 }, batches[1].ToArray());
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, batches[0].ToArray());
    }

    [TestMethod]
    public void DictionaryFactoriesOnlyRunForTheSelectedBranch()
    {
        IDictionary<string, int> values = new Dictionary<string, int> { ["existing"] = 1 };
        Assert.AreEqual(2, values.AddOrUpdate("existing", key => throw new AssertFailedException(), (key, value) => value + 1));
        Assert.AreEqual(7, values.AddOrUpdate("new", key => 7, (key, value) => throw new AssertFailedException()));
        Assert.AreEqual(7, values["new"]);
        Assert.ThrowsExactly<ArgumentNullException>(() => values.AddOrUpdate("existing", key => 1, null!));
    }

    [TestMethod]
    public void PropertyDictionarySkipsIndexersAndNonpublicGetters()
    {
        var values = new PropertyFixture().ToDictionary();
        Assert.AreEqual(1, values.Count);
        Assert.AreEqual("visible", values[nameof(PropertyFixture.Name)]);
    }

    [TestMethod]
    public void DuplicateDetectionStopsAtFirstDuplicate()
    {
        Assert.IsTrue(Source().ContainsDuplicates());
        static IEnumerable<int> Source()
        {
            yield return 1;
            yield return 1;
            throw new AssertFailedException("Unneeded elements were enumerated.");
        }
    }

    private sealed class PropertyFixture
    {
        public string Name => "visible";
        public string this[int index] => "indexed";
        public string WriteOnly { set { } }
        public string PrivateGetter { private get; set; } = "hidden";
    }
}
