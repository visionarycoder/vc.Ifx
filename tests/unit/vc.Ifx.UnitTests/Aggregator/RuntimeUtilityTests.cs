using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using vc.Ifx.Time;
using VisionaryCoder.Framework.Extensions;
using VisionaryCoder.Framework.Providers;
using VisionaryCoder.Framework.Storage;
using Wa.Wsdot.Fin.Idl.Ifx.Generics;

namespace VisionaryCoder.Framework.Tests.Aggregator;

[TestClass]
public sealed class RuntimeUtilityTests
{
    [TestMethod]
    public void ClocksReturnDeterministicEmptyOrCurrentUtcValues()
    {
        Assert.AreEqual(DateTimeOffset.MinValue, new NullClock().UtcNow);
        DateTimeOffset before = DateTimeOffset.UtcNow;
        DateTimeOffset now = new SystemClock().UtcNow;
        Assert.IsTrue(before <= now && now <= DateTimeOffset.UtcNow);
        Assert.AreEqual(TimeSpan.Zero, now.Offset);
    }

    [TestMethod]
    public void FrameworkMetadataFallbacksHandleAssembliesWithoutVersionAttributesOrFiles()
    {
        var assembly = new Mock<Assembly>();
        assembly.Setup(item => item.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), It.IsAny<bool>()))
            .Returns(Array.Empty<Attribute>());
        assembly.Setup(item => item.GetName()).Returns(new AssemblyName { Version = new Version(1, 2, 3) });
        Assert.AreEqual("1.2.3", Private<string>("GetVersion", assembly.Object));
        assembly.Setup(item => item.GetName()).Returns(new AssemblyName());
        Assert.AreEqual("0.0.0", Private<string>("GetVersion", assembly.Object));
        assembly.Setup(item => item.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), It.IsAny<bool>()))
            .Returns(new Attribute[] { new AssemblyInformationalVersionAttribute("release+sha") });
        Assert.AreEqual("release+sha", Private<string>("GetVersion", assembly.Object));
        Assert.AreEqual(DateTimeOffset.MinValue, Private<DateTimeOffset>("GetCompilationTime", ""));
        Assert.AreEqual(DateTimeOffset.MinValue, Private<DateTimeOffset>("GetCompilationTime",
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.dll")));
        string location = typeof(FrameworkInfoProvider).Assembly.Location;
        Assert.AreEqual(new DateTimeOffset(File.GetCreationTimeUtc(location)), new FrameworkInfoProvider().CompiledAt);
    }

    [TestMethod]
    public void CollectionAndEnumerableOverloadsExecuteEveryItemAndValidateGuards()
    {
        ICollection<int> values = new List<int>();
        values.AddRange([1, 2]);
        values.AddRange([]);
        CollectionAssert.AreEqual(new[] { 1, 2 }, values.ToArray());
        Assert.ThrowsExactly<ArgumentNullException>(() => VisionaryCoder.Framework.Extensions.CollectionExtensions.AddRange<int>(null!, []));
        Assert.ThrowsExactly<ArgumentNullException>(() => values.AddRange(null!));
        var observed = new List<int>();
        ((IEnumerable<int>)values).ForEach(observed.Add);
        Array.Empty<int>().ForEach(observed.Add);
        CollectionAssert.AreEqual(new[] { 1, 2 }, observed);
        Assert.ThrowsExactly<ArgumentNullException>(() => EnumerableExtensions.ForEach<int>(null!, observed.Add));
        Assert.ThrowsExactly<ArgumentNullException>(() => values.ForEach((Action<int>)null!));
        Assert.IsTrue(EnumerableExtensions.IsNullOrEmpty<int>(null));
        Assert.IsTrue(EnumerableExtensions.IsNullOrEmpty(Array.Empty<int>()));
        Assert.IsFalse(EnumerableExtensions.IsNullOrEmpty(new[] { 0 }));
        var legacy = new GenericReadOnlyCollection<int>();
        legacy.Add(7);
        legacy.AddRange([8, 9]);
        CollectionAssert.AreEqual(new[] { 7, 8, 9 }, legacy.ToArray());
        var enumerator = ((System.Collections.IEnumerable)legacy).GetEnumerator();
        var boxed = new List<object>();
        while (enumerator.MoveNext()) boxed.Add(enumerator.Current);
        CollectionAssert.AreEqual(new object[] { 7, 8, 9 }, boxed);
    }

    [TestMethod]
    public async Task FileEnumerationOverloadsRetainPatternsAndHonorCancellationEvenWhenEmpty()
    {
        string root = Path.Combine(Path.GetTempPath(), "ifx-runtime-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var storage = new StorageService(NullLogger<StorageService>.Instance);
            string file = Path.Combine(root, "value.txt");
            await File.WriteAllTextAsync(file, "content");
            Assert.AreEqual("content", await storage.ReadAllTextAsync(file, CancellationToken.None));
            CollectionAssert.AreEqual(new[] { file }, await Collect(storage.EnumerateFilesAsync(root, "*.txt")));
            CollectionAssert.AreEqual(new[] { file }, await Collect(storage.EnumerateFilesAsync(root, "*.txt", CancellationToken.None)));
            CollectionAssert.AreEqual(new[] { file }, await Collect(storage.EnumerateFilesAsync(root)));
            Assert.AreEqual(0, (await Collect(storage.EnumerateFilesAsync(root, "*.absent"))).Length);
            Assert.AreEqual(0, (await Collect(storage.EnumerateFilesAsync(root, "*.absent", CancellationToken.None))).Length);
            storage.DeleteDirectory(Path.Combine(root, "absent"));
            await storage.DeleteDirectoryAsync(Path.Combine(root, "absent"));
            Assert.IsFalse(Directory.Exists(Path.Combine(root, "absent")));
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => Collect(storage.EnumerateFilesAsync(root, "*.absent", cancellation.Token)));
            using var during = new CancellationTokenSource();
            await using var iterator = storage.EnumerateFilesAsync(root, "*", during.Token).GetAsyncEnumerator();
            Assert.IsTrue(await iterator.MoveNextAsync());
            await File.WriteAllTextAsync(Path.Combine(root, "other.txt"), "second");
            // A separate enumerator sees both files deterministically before cancellation.
            await using var next = storage.EnumerateFilesAsync(root, "*", during.Token).GetAsyncEnumerator();
            Assert.IsTrue(await next.MoveNextAsync());
            during.Cancel();
            await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () => await next.MoveNextAsync());
        }
        finally { Directory.Delete(root, true); }
    }

    private static T Private<T>(string name, object argument) => (T)typeof(FrameworkInfoProvider)
        .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static, [argument is Assembly ? typeof(Assembly) : typeof(string)])!
        .Invoke(null, [argument])!;

    private static async Task<string[]> Collect(IAsyncEnumerable<string> source)
    {
        var values = new List<string>();
        await foreach (string item in source) values.Add(item);
        return values.ToArray();
    }
}
