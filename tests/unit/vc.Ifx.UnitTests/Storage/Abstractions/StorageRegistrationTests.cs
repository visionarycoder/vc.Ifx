using VisionaryCoder.Framework.Storage;

namespace VisionaryCoder.Framework.Tests.Storage.Abstractions;

[TestClass]
public sealed class StorageRegistrationTests
{
    [TestMethod]
    public void EmptyRegistrationsAreValidAndIndependent()
    {
        var options = new StorageFactoryOptions();
        options.Validate();
        Assert.AreEqual(0, options.Implementations.Count);
        options.RegisterImplementation("object", typeof(object));
        Assert.AreEqual(0, new StorageFactoryOptions().Implementations.Count);
    }

    [TestMethod]
    public void RegistrationsRemainOrdinalAndLastRegistrationWins()
    {
        var options = new StorageFactoryOptions();
        var configuration = new object();
        options.RegisterImplementation("Store", typeof(object));
        options.RegisterImplementation("store", typeof(string), configuration);
        options.RegisterImplementation("Store", typeof(List<int>));

        options.Validate();

        Assert.AreEqual(2, options.Implementations.Count);
        Assert.AreEqual(typeof(List<int>), options.Implementations["Store"].ImplementationType);
        Assert.IsNull(options.Implementations["Store"].Options);
        Assert.AreSame(configuration, options.Implementations["store"].Options);
    }

    [TestMethod]
    public void ReadOnlyViewReflectsRegistrationsButRejectsMutation()
    {
        var options = new StorageFactoryOptions();
        IReadOnlyDictionary<string, StorageImplementation> view = options.Implementations;
        options.RegisterImplementation("store", typeof(object));
        Assert.AreSame(view, options.Implementations);
        Assert.AreEqual(1, view.Count);

        var dictionary = (IDictionary<string, StorageImplementation>)view;
        Assert.Throws<NotSupportedException>(() => dictionary.Add("other", new(typeof(object))));
        Assert.Throws<NotSupportedException>(() => dictionary["store"] = new(typeof(string)));
        Assert.Throws<NotSupportedException>(() => dictionary.Remove("store"));
        Assert.Throws<NotSupportedException>(() => dictionary.Clear());
        Assert.AreEqual(typeof(object), view["store"].ImplementationType);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" \t")]
    public void ValidationRejectsLegacyBlankNamesExplicitly(string name)
    {
        var options = new StorageFactoryOptions();
        options.RegisterImplementation(name, typeof(object));
        Assert.IsTrue(options.Implementations.ContainsKey(name));
        Assert.Throws<InvalidOperationException>(options.Validate);
    }

    [TestMethod]
    [DataRow(typeof(int))]
    [DataRow(typeof(IDisposable))]
    [DataRow(typeof(Stream))]
    [DataRow(typeof(List<>))]
    public void ValidationRejectsNonActivatableTypes(Type type)
    {
        var options = new StorageFactoryOptions();
        options.RegisterImplementation("store", type);
        Assert.Throws<InvalidOperationException>(options.Validate);
    }

    [TestMethod]
    public void NullRegistrationInputsDoNotReplaceExistingEntries()
    {
        var options = new StorageFactoryOptions();
        options.RegisterImplementation("store", typeof(object));
        Assert.Throws<ArgumentNullException>(() => options.RegisterImplementation(null!, typeof(object)));
        Assert.Throws<ArgumentNullException>(() => options.RegisterImplementation("store", null!));
        Assert.AreEqual(1, options.Implementations.Count);
        Assert.AreEqual(typeof(object), options.Implementations["store"].ImplementationType);
    }

    [TestMethod]
    public void DescriptorRetainsPositionalAndPassiveRecordBehavior()
    {
        var descriptor = new StorageImplementation(typeof(IDisposable));
        (Type type, object? options) = descriptor;
        Assert.AreEqual(typeof(IDisposable), type);
        Assert.IsNull(options);
        Assert.AreEqual(descriptor, new StorageImplementation(typeof(IDisposable)));
        var changed = descriptor with { ImplementationType = typeof(Stream), Options = "options" };
        Assert.AreEqual(typeof(Stream), changed.ImplementationType);
        Assert.AreEqual("options", changed.Options);
        Assert.AreNotEqual(descriptor, changed);
    }
}
