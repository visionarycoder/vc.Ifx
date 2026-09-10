using VisionaryCoder.Framework.Primitives;

namespace VisionaryCoder.Framework.Tests.Primitives;

[TestClass]
public class MoneyTests
{
    #region Construction

    [TestMethod]
    [DataRow(0, "usd", "USD")]
    [DataRow(12.34, " USD ", "USD")]
    [DataRow(-12.34, "cad", "CAD")]
    public void Constructor_WithValidValues_ShouldCreateMoney(double amount, string currency, string expectedCurrency)
    {
        decimal expectedAmount = Convert.ToDecimal(amount);

        var money = new Money(expectedAmount, currency);

        money.Amount.Should().Be(expectedAmount);
        money.Currency.Should().Be(expectedCurrency);
        money.Value.Should().Be(expectedAmount);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("US")]
    [DataRow("USDA")]
    [DataRow("U1D")]
    public void Constructor_WithInvalidCurrency_ShouldThrowArgumentException(string? currency)
    {
        Action act = () => _ = new Money(1m, currency!);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Currency value must be a three-letter code*")
            .WithParameterName("currency");
    }

    [TestMethod]
    public void Create_WithValidValues_ShouldCreateMoney()
    {
        var money = Money.Create(12.34m, "usd");

        money.Amount.Should().Be(12.34m);
        money.Currency.Should().Be("USD");
    }

    [TestMethod]
    public void Zero_WithValidCurrency_ShouldCreateZeroMoney()
    {
        var money = Money.Zero("usd");

        money.Amount.Should().Be(0m);
        money.Currency.Should().Be("USD");
    }

    #endregion

    #region Parsing

    [TestMethod]
    public void Parse_WithValidText_ShouldReturnMoney()
    {
        var money = Money.Parse("usd 12.34");

        money.Amount.Should().Be(12.34m);
        money.Currency.Should().Be("USD");
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("USD")]
    [DataRow("USD 12.34 extra")]
    [DataRow("US 12.34")]
    [DataRow("USD abc")]
    public void Parse_WithInvalidText_ShouldThrowFormatException(string? text)
    {
        Action act = () => _ = Money.Parse(text);

        act.Should().Throw<FormatException>()
            .WithMessage("*Invalid money value*");
    }

    [TestMethod]
    public void TryParse_WithValidText_ShouldReturnTrue()
    {
        bool result = Money.TryParse(" usd 12.34 ", out Money? money);

        result.Should().BeTrue();
        money.Should().NotBeNull();
        money!.Amount.Should().Be(12.34m);
        money.Currency.Should().Be("USD");
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("USD")]
    [DataRow("USD 12.34 extra")]
    [DataRow("US 12.34")]
    [DataRow("USD abc")]
    public void TryParse_WithInvalidText_ShouldReturnFalse(string? text)
    {
        bool result = Money.TryParse(text, out Money? money);

        result.Should().BeFalse();
        money.Should().BeNull();
    }

    #endregion

    #region Arithmetic

    [TestMethod]
    public void Add_WithSameCurrency_ShouldReturnSum()
    {
        var left = new Money(10m, "USD");
        var right = new Money(2.50m, "usd");

        Money result = left.Add(right);

        result.Amount.Should().Be(12.50m);
        result.Currency.Should().Be("USD");
    }

    [TestMethod]
    public void Add_WithNullMoney_ShouldThrowArgumentNullException()
    {
        var money = new Money(10m, "USD");

        Action act = () => _ = money.Add(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("other");
    }

    [TestMethod]
    public void Add_WithDifferentCurrency_ShouldThrowInvalidOperationException()
    {
        var left = new Money(10m, "USD");
        var right = new Money(2.50m, "CAD");

        Action act = () => _ = left.Add(right);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*same currency*");
    }

    [TestMethod]
    public void Subtract_WithSameCurrency_ShouldReturnDifference()
    {
        var left = new Money(10m, "USD");
        var right = new Money(2.50m, "usd");

        Money result = left.Subtract(right);

        result.Amount.Should().Be(7.50m);
        result.Currency.Should().Be("USD");
    }

    [TestMethod]
    public void Subtract_WithNullMoney_ShouldThrowArgumentNullException()
    {
        var money = new Money(10m, "USD");

        Action act = () => _ = money.Subtract(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("other");
    }

    [TestMethod]
    public void Subtract_WithDifferentCurrency_ShouldThrowInvalidOperationException()
    {
        var left = new Money(10m, "USD");
        var right = new Money(2.50m, "CAD");

        Action act = () => _ = left.Subtract(right);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*same currency*");
    }

    #endregion

    #region Conversion and metadata

    [TestMethod]
    public void ExplicitDecimalConversion_ShouldReturnAmount()
    {
        var money = new Money(12.34m, "USD");

        decimal amount = (decimal)money;

        amount.Should().Be(12.34m);
    }

    [TestMethod]
    public void ToString_ShouldReturnInvariantMoneyText()
    {
        var money = new Money(12.34m, "usd");

        money.ToString().Should().Be("USD 12.34");
    }

    [TestMethod]
    public void IPrimitiveValue_ShouldExposeValueMetadata()
    {
        IPrimitiveValue primitive = new Money(12.34m, "USD");

        primitive.ValueType.Should().Be(typeof(decimal));
        primitive.BoxedValue.Should().Be(12.34m);
    }

    [TestMethod]
    public void IPrimitiveValueOfT_ShouldExposeTypedValue()
    {
        IPrimitiveValue<decimal> primitive = new Money(12.34m, "USD");

        primitive.Value.Should().Be(12.34m);
    }

    #endregion

    #region Equality

    [TestMethod]
    public void Equals_WithSameAmountAndCurrency_ShouldReturnTrue()
    {
        var left = new Money(12.34m, "usd");
        var right = new Money(12.34m, "USD");

        left.Equals(right).Should().BeTrue();
        left.Equals((object)right).Should().BeTrue();
        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [TestMethod]
    public void Equals_WithDifferentAmountOrCurrency_ShouldReturnFalse()
    {
        var money = new Money(12.34m, "USD");
        var differentAmount = new Money(56.78m, "USD");
        var differentCurrency = new Money(12.34m, "CAD");

        money.Equals(differentAmount).Should().BeFalse();
        money.Equals(differentCurrency).Should().BeFalse();
        money.Equals((Money?)null).Should().BeFalse();
        object.Equals(money, "USD 12.34").Should().BeFalse();
        (money == differentAmount).Should().BeFalse();
        (money != differentAmount).Should().BeTrue();
    }

    [TestMethod]
    public void EqualityOperators_WithNullValues_ShouldReturnExpectedResults()
    {
        Money? left = null;
        Money? right = null;
        var money = new Money(12.34m, "USD");

        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
        (left == money).Should().BeFalse();
        (left != money).Should().BeTrue();
    }

    #endregion
}
