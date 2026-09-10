using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VisionaryCoder.Framework.Primitives;
using VisionaryCoder.Framework.Primitives.Data.EFCore;
using VisionaryCoder.Framework.Primitives.Web.AspNetCore;

namespace VisionaryCoder.Framework.Tests.Primitives;

[TestClass]
public sealed class PrimitiveIntegrationTests
{
    [TestMethod]
    public async Task BinderPreservesInputAndReportsInvalidIdentifiers()
    {
        var binder = new EntityIdModelBinder();
        var context = Context(typeof(EntityId<TestUser, int>), "42");
        await binder.BindModelAsync(context);
        context.Result.Model.Should().Be(new EntityId<TestUser, int>(42));
        context.ModelState["id"]!.AttemptedValue.Should().Be("42");

        foreach (string text in new[] { "", "  ", "not-an-id", "2147483648" })
        {
            context = Context(typeof(EntityId<TestUser, int>), text);
            await binder.BindModelAsync(context);
            context.Result.IsModelSet.Should().BeFalse();
            context.ModelState.IsValid.Should().BeFalse();
            context.ModelState["id"]!.Errors.Single().ErrorMessage.Should().Be("The identifier is invalid.");
        }

        context = Context(typeof(EntityId<TestUser, int>), null);
        await binder.BindModelAsync(context);
        context.Result.IsModelSet.Should().BeFalse();
        context.ModelState.Should().BeEmpty();

        context = Context(typeof(EntityId<TestUser, decimal>), "42");
        await binder.BindModelAsync(context);
        context.ModelState.IsValid.Should().BeFalse();
    }

    [TestMethod]
    public async Task BinderAndProviderValidateModelTypes()
    {
        var binder = new EntityIdModelBinder();
        var provider = new EntityIdModelBinderProvider();
        Action nullProvider = () => provider.GetBinder(null!);
        nullProvider.Should().Throw<ArgumentNullException>();
        Func<Task> nullBinder = () => binder.BindModelAsync(null!);
        await nullBinder.Should().ThrowAsync<ArgumentNullException>();

        foreach (Type type in new[] { typeof(int), typeof(List<int>), typeof(EntityId<,>) })
        {
            provider.GetBinder(new ProviderContext(type)).Should().BeNull();
            Func<Task> invalid = () => binder.BindModelAsync(Context(type, "42"));
            await invalid.Should().ThrowAsync<ArgumentException>();
        }
        provider.GetBinder(new ProviderContext(typeof(EntityId<TestUser, int>)))
            .Should().BeOfType<EntityIdModelBinder>();
    }

    [TestMethod]
    public void EfConversionPreservesRawKeysAndConfiguresProperty()
    {
        var converter = new EntityIdValueConverter<TestUser, int>();
        converter.ConvertToProvider(new EntityId<TestUser, int>(42)).Should().Be(42);
        converter.ConvertFromProvider(0).Should().Be(new EntityId<TestUser, int>(0));

        var model = new ModelBuilder();
        PropertyBuilder<EntityId<TestUser, int>> property = model.Entity<Row>().Property(x => x.Id);
        property.UseEntityId().Should().BeSameAs(property);
        property.Metadata.GetValueConverter().Should().BeOfType<EntityIdValueConverter<TestUser, int>>();
        Action missing = () => EntityIdModelBuilderExtensions.UseEntityId<TestUser, int>(null!);
        missing.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void JsonFactoryRejectsUnusableTypesAndHonorsScalarOptions()
    {
        var factory = new EntityIdJsonConverterFactory();
        var options = new JsonSerializerOptions { NumberHandling = JsonNumberHandling.AllowReadingFromString };
        options.Converters.Add(factory);
        foreach (Type type in new[] { typeof(int), typeof(List<int>), typeof(EntityId<,>) })
        {
            factory.CanConvert(type).Should().BeFalse();
            Action create = () => factory.CreateConverter(type, options);
            create.Should().Throw<ArgumentException>();
        }
        Action nullType = () => factory.CanConvert(null!);
        nullType.Should().Throw<ArgumentNullException>();
        Action nullOptions = () => factory.CreateConverter(typeof(EntityId<TestUser, int>), null!);
        nullOptions.Should().Throw<ArgumentNullException>();
        Action unsupportedWrite = () => JsonSerializer.Serialize(new EntityId<TestUser, decimal>(1m), options);
        unsupportedWrite.Should().Throw<NotSupportedException>();
        Action unsupportedRead = () => JsonSerializer.Deserialize<EntityId<TestUser, decimal>>("1", options);
        unsupportedRead.Should().Throw<NotSupportedException>();
        JsonSerializer.Deserialize<EntityId<TestUser, int>>("\"42\"", options).Value.Should().Be(42);
        JsonSerializer.Deserialize<EntityId<TestUser, int>>("0", options).Value.Should().Be(0);
        Action nullNumeric = () => JsonSerializer.Deserialize<EntityId<TestUser, int>>("null", options);
        nullNumeric.Should().Throw<JsonException>();
        JsonSerializer.Serialize(default(EntityId<TestUser, string>), options).Should().Be("null");
    }

    [TestMethod]
    public void IdFormattingIsInvariantAndDefaultStringsRemainEmpty()
    {
        CultureInfo saved = CultureInfo.CurrentCulture;
        try
        {
            var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            culture.NumberFormat.NegativeSign = "negative";
            CultureInfo.CurrentCulture = culture;
            new EntityId<TestUser, int>(-42).ToString().Should().Be("-42");
            default(EntityId<TestUser, string>).ToString().Should().BeEmpty();
            new EntityId<TestUser, NullText>(new NullText()).ToString().Should().BeEmpty();
        }
        finally
        {
            CultureInfo.CurrentCulture = saved;
        }
    }

    private static DefaultModelBindingContext Context(Type type, string? text)
    {
        return new DefaultModelBindingContext
        {
            ModelName = "id",
            ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(type),
            ModelState = new ModelStateDictionary(),
            ValueProvider = new SuppliedValue(text)
        };
    }

    private sealed class SuppliedValue(string? text) : IValueProvider
    {
        public bool ContainsPrefix(string prefix) => prefix == "id";
        public ValueProviderResult GetValue(string key) => text is null ? ValueProviderResult.None : new ValueProviderResult(text);
    }

    private sealed class ProviderContext(Type type) : ModelBinderProviderContext
    {
        public override ModelMetadata Metadata => MetadataProvider.GetMetadataForType(type);
        public override IModelMetadataProvider MetadataProvider { get; } = new EmptyModelMetadataProvider();
        public override BindingInfo BindingInfo { get; } = new();
        public override IModelBinder CreateBinder(ModelMetadata metadata) => throw new NotSupportedException();
    }

    private sealed class NullText
    {
        public override string ToString() => null!;
    }

    private sealed class Row
    {
        public EntityId<TestUser, int> Id { get; set; }
    }
}
