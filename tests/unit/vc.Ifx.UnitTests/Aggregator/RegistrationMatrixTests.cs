using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VisionaryCoder.Framework.Data.Azure.Table;
using VisionaryCoder.Framework.Messaging.Azure.Queue;
using VisionaryCoder.Framework.Storage;
using VisionaryCoder.Framework.Storage.Azure.Blob;
using VisionaryCoder.Framework.Storage.Ftp;
using VisionaryCoder.Framework.Storage.Local;

namespace VisionaryCoder.Framework.Tests.Aggregator;

[TestClass]
public sealed class RegistrationMatrixTests
{
    [TestMethod]
    public void EveryNamedProviderCapturesItsOwnOptionsWithoutRebindingDefaults()
    {
        CheckRegistration<IStorageProvider, FtpStorageOptions>(StorageExtensions.AddFtpStorage,
            StorageExtensions.AddNamedFtpStorage, Ftp("default"), Ftp("first"), Ftp("second"));
        CheckRegistration<IStorageProvider, AzureBlobStorageOptions>(StorageExtensions.AddAzureBlobStorage,
            StorageExtensions.AddNamedAzureBlobStorage, Blob("default"), Blob("first"), Blob("second"));
        CheckRegistration<IQueueStorageProvider, AzureQueueStorageOptions>(StorageExtensions.AddAzureQueueStorage,
            StorageExtensions.AddNamedAzureQueueStorage, Queue("default"), Queue("first"), Queue("second"));
        CheckRegistration<ITableStorageProvider, AzureTableStorageOptions>(StorageExtensions.AddAzureTableStorage,
            StorageExtensions.AddNamedAzureTableStorage, Table("Default"), Table("First"), Table("Second"));
    }

    [TestMethod]
    public void AllRegistrationMethodsRejectNullAndBlankArgumentsBeforeMutation()
    {
        foreach (MethodInfo method in typeof(StorageExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            var parameters = method.GetParameters();
            bool named = parameters.Length == 3;
            object options = Options(parameters[^1].ParameterType);
            var services = new ServiceCollection();
            object?[] args = named ? [services, "valid", options] : [services, options];
            AssertGuard(method, args, 0, null, typeof(ArgumentNullException));
            AssertGuard(method, args, args.Length - 1, null, typeof(ArgumentNullException));
            if (named)
            {
                AssertGuard(method, args, 1, null, typeof(ArgumentNullException));
                AssertGuard(method, args, 1, " ", typeof(ArgumentException));
            }
            Assert.AreEqual(0, services.Count);
        }
    }

    [TestMethod]
    public void BuilderDescriptorsAndNamedConcreteResolutionUseMatchingOptions()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var builder = new StorageRegistrationBuilder(services);
        var ftp = Ftp("first");
        var replacement = Ftp("second");
        var blob = Blob("first");
        var queue = Queue("first");
        var table = Table("First");
        Assert.AreSame(builder, builder.AddLocal().AddFtp("ftp", ftp).AddFtp("ftp", replacement)
            .AddBlob("blob", blob).AddQueue("queue", queue).AddTable("table", table));
        using var provider = services.BuildServiceProvider();
        var descriptors = provider.GetRequiredService<IOptions<StorageFactoryOptions>>().Value;
        descriptors.Validate();
        Assert.AreEqual(5, descriptors.Implementations.Count);
        Assert.IsNull(descriptors.Implementations["local"].Options);
        Assert.IsInstanceOfType<LocalStorageProvider>(provider.GetRequiredService<LocalStorageProvider>());
        Assert.IsInstanceOfType<LocalStorageProvider>(provider.GetRequiredKeyedService<LocalStorageProvider>("local"));
        Assert.AreSame(ftp, CapturedOptions(provider.GetRequiredService<FtpStorageProvider>()));
        Assert.AreSame(replacement, descriptors.Implementations["ftp"].Options);
        Assert.AreSame(replacement, CapturedOptions(provider.GetRequiredKeyedService<FtpStorageProvider>("ftp")));
        Assert.AreSame(blob, CapturedOptions(provider.GetRequiredService<AzureBlobStorageProvider>()));
        Assert.AreSame(blob, CapturedOptions(provider.GetRequiredKeyedService<AzureBlobStorageProvider>("blob")));
        Assert.AreSame(queue, CapturedOptions(provider.GetRequiredService<IQueueStorageProvider>()));
        Assert.AreSame(queue, CapturedOptions(provider.GetRequiredKeyedService<AzureQueueStorageProvider>("queue")));
        Assert.AreSame(table, CapturedOptions(provider.GetRequiredService<ITableStorageProvider>()));
        Assert.AreSame(table, CapturedOptions(provider.GetRequiredKeyedService<AzureTableStorageProvider>("table")));
    }

    [TestMethod]
    public void BuilderRejectsInvalidArgumentsWithoutRecordingConfiguration()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new StorageRegistrationBuilder(null!));
        var services = new ServiceCollection();
        var builder = new StorageRegistrationBuilder(services);
        Assert.ThrowsExactly<ArgumentException>(() => builder.AddLocal(" "));
        Assert.ThrowsExactly<ArgumentNullException>(() => builder.AddFtp("ftp", null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => builder.AddBlob("blob", null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => builder.AddQueue("queue", null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => builder.AddTable("table", null!));
        Assert.ThrowsExactly<ArgumentException>(() => builder.AddFtp(" ", Ftp("first")));
        Assert.ThrowsExactly<ArgumentException>(() => builder.AddBlob(" ", Blob("first")));
        Assert.ThrowsExactly<ArgumentException>(() => builder.AddQueue(" ", Queue("first")));
        Assert.ThrowsExactly<ArgumentException>(() => builder.AddTable(" ", Table("First")));
        Assert.AreEqual(0, services.Count);
    }

    [TestMethod]
    public async Task PortableCompositionResolvesOneLegacyServiceThenUsesItsOptionalContract()
    {
        string root = Path.Combine(Path.GetTempPath(), "ifx-portable-" + Guid.NewGuid().ToString("N"));
        try
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddNamedLocalStorage("archive", new LocalStorageOptions { RootPath = root });
            using var container = services.BuildServiceProvider();
            Assert.IsNull(container.GetKeyedService<IObjectStorageProvider>("archive"));
            IStorageProvider legacy = container.GetRequiredKeyedService<IStorageProvider>("archive");
            var portable = (IObjectStorageProvider)legacy;
            Assert.AreSame<object>(legacy, portable);
            using var content = new MemoryStream([1, 2, 3]);
            await portable.WriteAsync(new StorageWriteRequest("value", content));
            Assert.IsTrue(content.CanRead);
            Assert.AreEqual(3L, (await portable.GetMetadataAsync(new StorageObjectRequest("value")))!.Length);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    private static void CheckRegistration<TProvider, TOptions>(Func<IServiceCollection, TOptions, IServiceCollection> add,
        Func<IServiceCollection, string, TOptions, IServiceCollection> named, TOptions defaultOptions, TOptions first, TOptions second)
        where TProvider : class where TOptions : class
    {
        var services = new ServiceCollection();
        services.AddLogging();
        Assert.AreSame(services, add(services, defaultOptions));
        Assert.AreSame(services, named(services, "first", first));
        named(services, "second", second);
        named(services, "first", second);
        using var provider = services.BuildServiceProvider();
        Assert.AreSame(defaultOptions, CapturedOptions(provider.GetRequiredService<TProvider>()));
        var resolved = provider.GetRequiredKeyedService<TProvider>("first");
        Assert.AreSame(first, CapturedOptions(resolved));
        Assert.AreSame(second, CapturedOptions(provider.GetRequiredKeyedService<TProvider>("second")));
        Assert.AreNotSame(resolved, provider.GetRequiredKeyedService<TProvider>("first"));
    }

    // Inspect only the captured configuration; activation must not perform network I/O.
    private static object? CapturedOptions(object provider) => provider.GetType()
        .GetField("options", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(provider);

    private static void AssertGuard(MethodInfo method, object?[] args, int index, object? replacement, Type exception)
    {
        object?[] changed = (object?[])args.Clone();
        changed[index] = replacement;
        var thrown = Assert.ThrowsExactly<TargetInvocationException>(() => method.Invoke(null, changed));
        Assert.AreEqual(exception, thrown.InnerException!.GetType(), method.Name);
    }

    private static object Options(Type type) => type.Name switch
    {
        nameof(LocalStorageOptions) => new LocalStorageOptions(),
        nameof(FtpStorageOptions) => Ftp("first"),
        nameof(AzureBlobStorageOptions) => Blob("first"),
        nameof(AzureQueueStorageOptions) => Queue("first"),
        nameof(AzureTableStorageOptions) => Table("First"),
        _ => throw new AssertFailedException(type.ToString())
    };

    private static FtpStorageOptions Ftp(string name) => new() { Host = name + ".invalid", Username = name, Password = "test-only" };
    private static AzureBlobStorageOptions Blob(string name) => new() { ConnectionString = "UseDevelopmentStorage=true", ContainerName = name };
    private static AzureQueueStorageOptions Queue(string name) => new() { ConnectionString = "UseDevelopmentStorage=true", QueueName = name };
    private static AzureTableStorageOptions Table(string name) => new() { ConnectionString = "UseDevelopmentStorage=true", TableName = name };
}
