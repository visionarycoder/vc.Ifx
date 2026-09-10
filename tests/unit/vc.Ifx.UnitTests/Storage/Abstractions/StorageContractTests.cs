using System.Reflection;
using VisionaryCoder.Framework.Storage;

namespace VisionaryCoder.Framework.Tests.Storage.Abstractions;

[TestClass]
public sealed class StorageContractTests
{
    [TestMethod]
    public void ObjectRequestsPreserveOpaquePathsAndValueEquality()
    {
        const string path = " Folder\\Object *? ";
        var request = new StorageObjectRequest(path);

        Assert.AreEqual(path, request.Path);
        Assert.AreEqual(request, new StorageObjectRequest(path));
        Assert.AreEqual(request.GetHashCode(), new StorageObjectRequest(path).GetHashCode());
        Assert.AreNotEqual(request, new StorageObjectRequest("other"));
        Assert.AreEqual(request, request with { });
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" \t\r\n")]
    public void AllObjectRecordsRejectInvalidPaths(string? path)
    {
        using var stream = new MemoryStream();
        Assert.Throws<ArgumentException>(() => new StorageObjectRequest(path!));
        Assert.Throws<ArgumentException>(() => new StorageWriteRequest(path!, stream));
        Assert.Throws<ArgumentException>(() => new StorageObjectMetadata(path!));
    }

    [TestMethod]
    public void WriteRequestDefaultsPreserveStreamOwnershipAndPosition()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        stream.Position = 1;

        var request = new StorageWriteRequest("object", stream);
        var createOnly = new StorageWriteRequest("object", stream, overwrite: false);

        Assert.AreEqual("object", request.Path);
        Assert.AreSame(stream, request.Content);
        Assert.IsTrue(request.Overwrite);
        Assert.IsFalse(createOnly.Overwrite);
        Assert.AreEqual(1L, stream.Position);
        Assert.IsTrue(stream.CanRead);
        Assert.AreEqual(request, new StorageWriteRequest("object", stream));
        Assert.AreEqual(request.GetHashCode(), new StorageWriteRequest("object", stream).GetHashCode());
        Assert.AreNotEqual(request, createOnly);
        using var otherStream = new MemoryStream([1, 2, 3]);
        Assert.AreNotEqual(request, new StorageWriteRequest("object", otherStream));
        Assert.AreSame(stream, (request with { }).Content);
    }

    [TestMethod]
    public void WriteRequestRejectsNullAndUnreadableStreams()
    {
        Assert.Throws<ArgumentNullException>(() => new StorageWriteRequest("object", null!));
        var stream = new MemoryStream();
        stream.Dispose();

        ArgumentException exception = Assert.Throws<ArgumentException>(() => new StorageWriteRequest("object", stream));
        Assert.AreEqual("content", exception.ParamName);
    }

    [TestMethod]
    public void WriteRequestAcceptsNonSeekableStreamsWithoutReading()
    {
        using var stream = new NonSeekableStream();
        var request = new StorageWriteRequest("object", stream);

        Assert.AreSame(stream, request.Content);
        Assert.IsFalse(request.Content.CanSeek);
    }

    [TestMethod]
    public void ListRequestDefaultsAndLiteralPrefixesAreStable()
    {
        Assert.AreEqual("", new StorageListRequest().Prefix);
        Assert.AreEqual(new StorageListRequest(), new StorageListRequest(""));
        Assert.Throws<ArgumentNullException>(() => new StorageListRequest(null!));
        var request = new StorageListRequest(" A\\*? ");
        Assert.AreEqual(" A\\*? ", request.Prefix);
        Assert.AreEqual(request, request with { });
        Assert.AreEqual(request.GetHashCode(), new StorageListRequest(request.Prefix).GetHashCode());
        Assert.AreNotEqual(request, new StorageListRequest("a"));
    }

    [TestMethod]
    public void MetadataDefaultsDistinguishUnknownAndEmptyContent()
    {
        var metadata = new StorageObjectMetadata("object");
        Assert.AreEqual("object", metadata.Path);
        Assert.IsNull(metadata.Length);
        Assert.IsNull(metadata.LastModified);
        Assert.IsNull(metadata.ContentType);
        Assert.IsNull(metadata.Version);
        Assert.AreEqual(0L, new StorageObjectMetadata("object", 0).Length);
        Assert.AreNotEqual(metadata, new StorageObjectMetadata("object", 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new StorageObjectMetadata("object", -1));
    }

    [TestMethod]
    public void MetadataPreservesFactsAndValueEquality()
    {
        var timestamp = new DateTimeOffset(2026, 9, 9, 12, 0, 0, TimeSpan.FromHours(-7));
        var metadata = new StorageObjectMetadata("A/object", long.MaxValue, timestamp, "text/plain", "\"version\"");
        var same = new StorageObjectMetadata("A/object", long.MaxValue, timestamp, "text/plain", "\"version\"");
        Assert.AreEqual(long.MaxValue, metadata.Length);
        Assert.AreEqual(timestamp, metadata.LastModified);
        Assert.AreEqual("text/plain", metadata.ContentType);
        Assert.AreEqual("\"version\"", metadata.Version);
        Assert.AreEqual(metadata, same);
        Assert.AreEqual(metadata.GetHashCode(), same.GetHashCode());
        Assert.AreEqual(metadata, metadata with { });
        Assert.AreNotEqual(metadata, new StorageObjectMetadata("a/object", long.MaxValue, timestamp, "text/plain", "\"version\""));
        Assert.AreNotEqual(metadata, new StorageObjectMetadata("A/object", 0, timestamp, "text/plain", "\"version\""));
        Assert.AreNotEqual(metadata, new StorageObjectMetadata("A/object", long.MaxValue, null, "text/plain", "\"version\""));
        Assert.AreNotEqual(metadata, new StorageObjectMetadata("A/object", long.MaxValue, timestamp, null, "\"version\""));
        Assert.AreNotEqual(metadata, new StorageObjectMetadata("A/object", long.MaxValue, timestamp, "text/plain", null));
    }

    [TestMethod]
    public void PortableContractRemainsIndependentAndCancellationAware()
    {
        Type contract = typeof(IObjectStorageProvider);
        Assert.AreEqual(0, contract.GetInterfaces().Length);
        MethodInfo[] methods = contract.GetMethods().Where(method => !method.IsSpecialName).ToArray();
        Assert.AreEqual(5, methods.Length);
        foreach (MethodInfo method in methods)
        {
            Assert.IsTrue(method.IsAbstract);
            ParameterInfo[] parameters = method.GetParameters();
            Assert.AreEqual(2, parameters.Length);
            Assert.AreEqual(typeof(CancellationToken), parameters[1].ParameterType);
            Assert.IsTrue(parameters[1].HasDefaultValue);
            Assert.AreEqual(typeof(StorageObjectRequest).Assembly, parameters[0].ParameterType.Assembly);
        }

        Assert.AreEqual(typeof(Task<Stream>), contract.GetMethod("OpenReadAsync")!.ReturnType);
        Assert.AreEqual(typeof(Task<StorageObjectMetadata>), contract.GetMethod("WriteAsync")!.ReturnType);
        Assert.AreEqual(typeof(Task<StorageObjectMetadata>), contract.GetMethod("GetMetadataAsync")!.ReturnType);
        Assert.AreEqual(typeof(Task), contract.GetMethod("DeleteAsync")!.ReturnType);
        Assert.AreEqual(typeof(IAsyncEnumerable<StorageObjectMetadata>), contract.GetMethod("ListAsync")!.ReturnType);
    }

    [TestMethod]
    public void CapabilitiesHaveDistinctStableFlags()
    {
        Assert.AreEqual(0, (int)StorageCapabilities.None);
        Assert.AreEqual(1, (int)StorageCapabilities.Read);
        Assert.AreEqual(2, (int)StorageCapabilities.Write);
        Assert.AreEqual(4, (int)StorageCapabilities.Delete);
        Assert.AreEqual(8, (int)StorageCapabilities.Metadata);
        Assert.AreEqual(16, (int)StorageCapabilities.List);
        Assert.AreEqual(32, (int)StorageCapabilities.CreateOnly);
        Assert.AreEqual(7, Enum.GetValues<StorageCapabilities>().Length);
        Assert.IsTrue(typeof(StorageCapabilities).IsDefined(typeof(FlagsAttribute)));
    }

    private sealed class NonSeekableStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => throw new NotSupportedException();
        public override int Read(byte[] buffer, int offset, int count) => throw new InvalidOperationException("Construction must not read content.");
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
