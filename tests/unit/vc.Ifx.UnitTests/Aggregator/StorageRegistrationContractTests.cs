using Microsoft.Extensions.DependencyInjection;
using VisionaryCoder.Framework.Storage;
using VisionaryCoder.Framework.Storage.Local;

namespace VisionaryCoder.Framework.Tests.Aggregator;

[TestClass]
public sealed class StorageRegistrationContractTests
{
    [TestMethod]
    public async Task NamedRootsAreIndependentAndDoNotChangeTheDefaultProvider()
    {
        string root = Path.Combine(Path.GetTempPath(), "ifx-registration-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddLocalStorage(new LocalStorageOptions { RootPath = Path.Combine(root, "default") });
            services.AddNamedLocalStorage("first", new LocalStorageOptions { RootPath = Path.Combine(root, "first") });
            services.AddNamedLocalStorage("second", new LocalStorageOptions { RootPath = Path.Combine(root, "second") });
            services.AddNamedLocalStorage("first", new LocalStorageOptions { RootPath = Path.Combine(root, "ignored") });
            using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
            await Write((IObjectStorageProvider)provider.GetRequiredService<IStorageProvider>(), 1);
            await Write((IObjectStorageProvider)provider.GetRequiredKeyedService<IStorageProvider>("first"), 2);
            await Write((IObjectStorageProvider)provider.GetRequiredKeyedService<IStorageProvider>("second"), 3);
            CollectionAssert.AreEqual(new byte[] { 1 }, File.ReadAllBytes(Path.Combine(root, "default/value")));
            CollectionAssert.AreEqual(new byte[] { 2 }, File.ReadAllBytes(Path.Combine(root, "first/value")));
            CollectionAssert.AreEqual(new byte[] { 3 }, File.ReadAllBytes(Path.Combine(root, "second/value")));
            Assert.IsFalse(Directory.Exists(Path.Combine(root, "ignored")));
        }
        finally
        {
            Directory.Delete(root, true);
        }

        static async Task Write(IObjectStorageProvider provider, byte value)
        {
            using var content = new MemoryStream([value]);
            await provider.WriteAsync(new StorageWriteRequest("value", content));
        }
    }
}
