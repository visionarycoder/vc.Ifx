using System.Text;

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Ifx.Extensions;

namespace Ifx.Tests.Extensions;

[TestClass]
public sealed class TypeExtensionTests
{
    [TestMethod]
    [DataRow("true", true)]
    [DataRow("false", false)]
    [DataRow("invalid", false)]
    public void AsBoolean_WithStringInputs_ReturnsExpectedValue(string value, bool expected)
    {
        value.AsBoolean().Should().Be(expected);
    }

    [TestMethod]
    public void AsBoolean_CoversSupportedNumericAndFallbackCases()
    {
        object? nullValue = null;

        nullValue.AsBoolean().Should().BeFalse();
        true.AsBoolean().Should().BeTrue();
        false.AsBoolean().Should().BeFalse();
        "true".AsBoolean().Should().BeTrue();
        "false".AsBoolean().Should().BeFalse();
        "invalid".AsBoolean().Should().BeFalse();
        5.AsBoolean().Should().BeTrue();
        0.AsBoolean().Should().BeFalse();
        5L.AsBoolean().Should().BeTrue();
        0L.AsBoolean().Should().BeFalse();
        0.1d.AsBoolean().Should().BeTrue();
        0.0d.AsBoolean().Should().BeFalse();
        1.5m.AsBoolean().Should().BeTrue();
        0m.AsBoolean().Should().BeFalse();
        0.5f.AsBoolean().Should().BeTrue();
        0f.AsBoolean().Should().BeFalse();
        ((byte)1).AsBoolean().Should().BeTrue();
        ((byte)0).AsBoolean().Should().BeFalse();
        ((short)1).AsBoolean().Should().BeTrue();
        ((short)0).AsBoolean().Should().BeFalse();
        1u.AsBoolean().Should().BeTrue();
        0u.AsBoolean().Should().BeFalse();
        1UL.AsBoolean().Should().BeTrue();
        0UL.AsBoolean().Should().BeFalse();
        new object().AsBoolean().Should().BeFalse();
    }

    [TestMethod]
    public void AsInteger_CoversAllSwitchArmsAndRangeChecks()
    {
        object? nullValue = null;

        nullValue.AsInteger(42).Should().Be(42);
        123.AsInteger().Should().Be(123);
        true.AsInteger().Should().Be(1);
        false.AsInteger().Should().Be(0);
        "456".AsInteger().Should().Be(456);
        "invalid".AsInteger(99).Should().Be(99);
        123.7d.AsInteger().Should().Be(123);
        double.MaxValue.AsInteger(77).Should().Be(77);
        456.9m.AsInteger().Should().Be(456);
        decimal.MaxValue.AsInteger(78).Should().Be(78);
        789.3f.AsInteger().Should().Be(789);
        float.MaxValue.AsInteger(79).Should().Be(79);
        987654321L.AsInteger().Should().Be(987654321);
        ((long)int.MaxValue + 1).AsInteger(80).Should().Be(80);
        ((byte)12).AsInteger().Should().Be(12);
        ((short)13).AsInteger().Should().Be(13);
        14u.AsInteger().Should().Be(14);
        ((uint)int.MaxValue + 1).AsInteger(81).Should().Be(81);
        15UL.AsInteger().Should().Be(15);
        ((ulong)int.MaxValue + 1).AsInteger(82).Should().Be(82);
        new object().AsInteger(83).Should().Be(83);
    }

    [TestMethod]
    public void AsLong_CoversAllSwitchArmsAndRangeChecks()
    {
        object? nullValue = null;

        nullValue.AsLong(100L).Should().Be(100L);
        9876543210L.AsLong().Should().Be(9876543210L);
        123.AsLong().Should().Be(123L);
        true.AsLong().Should().Be(1L);
        false.AsLong().Should().Be(0L);
        "987654321".AsLong().Should().Be(987654321L);
        "invalid".AsLong(555L).Should().Be(555L);
        123.7d.AsLong().Should().Be(123L);
        double.MaxValue.AsLong(556L).Should().Be(556L);
        456.9m.AsLong().Should().Be(456L);
        decimal.MaxValue.AsLong(557L).Should().Be(557L);
        789.3f.AsLong().Should().Be(789L);
        float.PositiveInfinity.AsLong(558L).Should().Be(558L);
        ((byte)12).AsLong().Should().Be(12L);
        ((short)13).AsLong().Should().Be(13L);
        14u.AsLong().Should().Be(14L);
        15UL.AsLong().Should().Be(15L);
        ((ulong)long.MaxValue + 1UL).AsLong(559L).Should().Be(559L);
        new object().AsLong(560L).Should().Be(560L);
    }

    [TestMethod]
    public void AsDouble_CoversAllSwitchArmsAndFallbackCase()
    {
        object? nullValue = null;

        nullValue.AsDouble(1.5).Should().Be(1.5);
        123.456d.AsDouble().Should().Be(123.456d);
        42.AsDouble().Should().Be(42d);
        43L.AsDouble().Should().Be(43d);
        true.AsDouble().Should().Be(1d);
        false.AsDouble().Should().Be(0d);
        "987.654".AsDouble().Should().Be(987.654d);
        "invalid".AsDouble(2.5).Should().Be(2.5d);
        12.25m.AsDouble().Should().Be(12.25d);
        6.5f.AsDouble().Should().Be(6.5d);
        ((byte)7).AsDouble().Should().Be(7d);
        ((short)8).AsDouble().Should().Be(8d);
        9u.AsDouble().Should().Be(9d);
        10UL.AsDouble().Should().Be(10d);
        new object().AsDouble(11.5).Should().Be(11.5d);
    }

    [TestMethod]
    public void AsDecimal_CoversAllSwitchArmsOverflowAndFallbackCase()
    {
        object? nullValue = null;

        nullValue.AsDecimal(1.5m).Should().Be(1.5m);
        123.456m.AsDecimal().Should().Be(123.456m);
        true.AsDecimal().Should().Be(1m);
        false.AsDecimal().Should().Be(0m);
        "987.654".AsDecimal().Should().Be(987.654m);
        "invalid".AsDecimal(2.5m).Should().Be(2.5m);
        42.AsDecimal().Should().Be(42m);
        43L.AsDecimal().Should().Be(43m);
        12.25d.AsDecimal().Should().Be(12.25m);
        double.MaxValue.AsDecimal(3.5m).Should().Be(3.5m);
        6.5f.AsDecimal().Should().Be(6.5m);
        float.PositiveInfinity.AsDecimal(4.5m).Should().Be(4.5m);
        ((byte)7).AsDecimal().Should().Be(7m);
        ((short)8).AsDecimal().Should().Be(8m);
        9u.AsDecimal().Should().Be(9m);
        10UL.AsDecimal().Should().Be(10m);
        new object().AsDecimal(11.5m).Should().Be(11.5m);
    }

    [TestMethod]
    public void AsFloat_CoversAllSwitchArmsAndFallbackCase()
    {
        object? nullValue = null;

        nullValue.AsFloat(1.5f).Should().Be(1.5f);
        123.456f.AsFloat().Should().Be(123.456f);
        true.AsFloat().Should().Be(1f);
        false.AsFloat().Should().Be(0f);
        "987.654".AsFloat().Should().BeApproximately(987.654f, 0.001f);
        "invalid".AsFloat(2.5f).Should().Be(2.5f);
        42.AsFloat().Should().Be(42f);
        43L.AsFloat().Should().Be(43f);
        12.25d.AsFloat().Should().Be(12.25f);
        12.25m.AsFloat().Should().Be(12.25f);
        ((byte)7).AsFloat().Should().Be(7f);
        ((short)8).AsFloat().Should().Be(8f);
        9u.AsFloat().Should().Be(9f);
        10UL.AsFloat().Should().Be(10f);
        new object().AsFloat(11.5f).Should().Be(11.5f);
    }

    [TestMethod]
    public void AsString_CoversNullDirectAndToStringCases()
    {
        object? nullValue = null;

        nullValue.AsString("default").Should().Be("default");
        "test string".AsString().Should().Be("test string");
        123.AsString().Should().Be("123");
        true.AsString().Should().Be("True");
        new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc).AsString().Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public void AsDateTime_CoversAllSwitchArmsAndFallbackCase()
    {
        object? nullValue = null;
        DateTime defaultValue = new(2024, 1, 1);
        DateTime expectedFromMilliseconds = DateTimeOffset.FromUnixTimeMilliseconds(1_700_000_000_000L).DateTime;
        DateTime expectedFromSeconds = DateTimeOffset.FromUnixTimeSeconds(1_700_000_000).DateTime;
        DateTimeOffset offset = new(new DateTime(2024, 6, 15, 8, 30, 0, DateTimeKind.Utc));

        nullValue.AsDateTime(defaultValue).Should().Be(defaultValue);
        new DateTime(2024, 6, 15).AsDateTime().Should().Be(new DateTime(2024, 6, 15));
        offset.AsDateTime().Should().Be(offset.DateTime);
        "2024-01-01".AsDateTime().Should().Be(new DateTime(2024, 1, 1));
        "invalid date".AsDateTime(defaultValue).Should().Be(defaultValue);
        1_700_000_000_000L.AsDateTime().Should().Be(expectedFromMilliseconds);
        253402300800000L.AsDateTime(defaultValue).Should().Be(defaultValue);
        1_700_000_000.AsDateTime().Should().Be(expectedFromSeconds);
        new object().AsDateTime(defaultValue).Should().Be(defaultValue);
    }

    [TestMethod]
    public void AsDateTimeOffset_CoversAllSwitchArmsAndFallbackCase()
    {
        object? nullValue = null;
        DateTimeOffset defaultValue = new(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        DateTime sourceDateTime = new(2024, 6, 15, 8, 30, 0, DateTimeKind.Utc);
        DateTimeOffset expectedFromMilliseconds = DateTimeOffset.FromUnixTimeMilliseconds(1_700_000_000_000L);
        DateTimeOffset expectedFromSeconds = DateTimeOffset.FromUnixTimeSeconds(1_700_000_000);

        nullValue.AsDateTimeOffset(defaultValue).Should().Be(defaultValue);
        expectedFromMilliseconds.AsDateTimeOffset().Should().Be(expectedFromMilliseconds);
        sourceDateTime.AsDateTimeOffset().Should().Be(new DateTimeOffset(sourceDateTime));
        "2024-01-01T00:00:00+00:00".AsDateTimeOffset().Should().Be(DateTimeOffset.Parse("2024-01-01T00:00:00+00:00"));
        "invalid".AsDateTimeOffset(defaultValue).Should().Be(defaultValue);
        1_700_000_000_000L.AsDateTimeOffset().Should().Be(expectedFromMilliseconds);
        253402300800000L.AsDateTimeOffset(defaultValue).Should().Be(defaultValue);
        1_700_000_000.AsDateTimeOffset().Should().Be(expectedFromSeconds);
        new object().AsDateTimeOffset(defaultValue).Should().Be(defaultValue);
    }

    [TestMethod]
    public void AsGuid_CoversAllSwitchArmsAndFallbackCase()
    {
        object? nullValue = null;
        Guid guid = Guid.Parse("11111111-2222-3333-4444-555555555555");
        Guid defaultValue = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        byte[] bytes = guid.ToByteArray();

        nullValue.AsGuid(defaultValue).Should().Be(defaultValue);
        guid.AsGuid().Should().Be(guid);
        guid.ToString().AsGuid().Should().Be(guid);
        "invalid-guid".AsGuid(defaultValue).Should().Be(defaultValue);
        bytes.AsGuid().Should().Be(guid);
        new byte[4].AsGuid(defaultValue).Should().Be(defaultValue);
        new object().AsGuid(defaultValue).Should().Be(defaultValue);
    }

    [TestMethod]
    public void AsByte_CoversAllSwitchArmsRangeChecksAndFallbackCase()
    {
        object? nullValue = null;

        nullValue.AsByte(11).Should().Be(11);
        ((byte)12).AsByte().Should().Be(12);
        13.AsByte().Should().Be(13);
        256.AsByte(14).Should().Be(14);
        15L.AsByte().Should().Be(15);
        (-1L).AsByte(16).Should().Be(16);
        true.AsByte().Should().Be(1);
        false.AsByte().Should().Be(0);
        "17".AsByte().Should().Be(17);
        "invalid".AsByte(18).Should().Be(18);
        19.8d.AsByte().Should().Be(19);
        256d.AsByte(20).Should().Be(20);
        21.8m.AsByte().Should().Be(21);
        256m.AsByte(22).Should().Be(22);
        23.8f.AsByte().Should().Be(23);
        256f.AsByte(24).Should().Be(24);
        ((short)25).AsByte().Should().Be(25);
        ((short)-1).AsByte(26).Should().Be(26);
        27u.AsByte().Should().Be(27);
        256u.AsByte(28).Should().Be(28);
        new object().AsByte(29).Should().Be(29);
    }

    [TestMethod]
    public void AsShort_CoversAllSwitchArmsRangeChecksAndFallbackCase()
    {
        object? nullValue = null;

        nullValue.AsShort(11).Should().Be(11);
        ((short)12).AsShort().Should().Be(12);
        13.AsShort().Should().Be(13);
        ((int)short.MaxValue + 1).AsShort(14).Should().Be(14);
        15L.AsShort().Should().Be(15);
        ((long)short.MaxValue + 1).AsShort(16).Should().Be(16);
        true.AsShort().Should().Be(1);
        false.AsShort().Should().Be(0);
        "17".AsShort().Should().Be(17);
        "invalid".AsShort(18).Should().Be(18);
        19.8d.AsShort().Should().Be(19);
        40000d.AsShort(20).Should().Be(20);
        21.8m.AsShort().Should().Be(21);
        40000m.AsShort(22).Should().Be(22);
        23.8f.AsShort().Should().Be(23);
        40000f.AsShort(24).Should().Be(24);
        ((byte)25).AsShort().Should().Be(25);
        26u.AsShort().Should().Be(26);
        ((uint)short.MaxValue + 1).AsShort(27).Should().Be(27);
        new object().AsShort(28).Should().Be(28);
    }

    [TestMethod]
    public void AsChar_CoversAllSwitchArmsAndFallbackCase()
    {
        object? nullValue = null;

        nullValue.AsChar('x').Should().Be('x');
        'a'.AsChar().Should().Be('a');
        "beta".AsChar().Should().Be('b');
        string.Empty.AsChar('z').Should().Be('z');
        67.AsChar().Should().Be('C');
        1114112.AsChar('q').Should().Be('q');
        ((byte)68).AsChar().Should().Be('D');
        new object().AsChar('r').Should().Be('r');
    }

    [TestMethod]
    public void AsByteArray_CoversAllSwitchArmsAndFallbackCase()
    {
        byte[] bytes = [1, 2, 3];
        Guid guid = Guid.Parse("11111111-2222-3333-4444-555555555555");
        object? nullValue = null;

        nullValue.AsByteArray().Should().BeNull();
        bytes.AsByteArray().Should().BeSameAs(bytes);
        "abc".AsByteArray().Should().Equal(Encoding.UTF8.GetBytes("abc"));
        guid.AsByteArray().Should().Equal(guid.ToByteArray());
        new object().AsByteArray().Should().BeNull();
    }

    [TestMethod]
    public void AsEnum_CoversAllSwitchArmsAndFallbackCase()
    {
        object? nullValue = null;

        nullValue.AsEnum<object?, SampleEnum>(SampleEnum.Two).Should().Be(SampleEnum.Two);
        SampleEnum.One.AsEnum<SampleEnum, SampleEnum>().Should().Be(SampleEnum.One);
        "one".AsEnum<string, SampleEnum>().Should().Be(SampleEnum.One);
        "invalid".AsEnum<string, SampleEnum>(SampleEnum.Two).Should().Be(SampleEnum.Two);
        1.AsEnum<int, SampleEnum>().Should().Be(SampleEnum.One);
        99.AsEnum<int, SampleEnum>(SampleEnum.Two).Should().Be(SampleEnum.Two);
        ((byte)2).AsEnum<byte, SampleEnum>().Should().Be(SampleEnum.Two);
        ((byte)99).AsEnum<byte, SampleEnum>(SampleEnum.One).Should().Be(SampleEnum.One);
        ((short)1).AsEnum<short, SampleEnum>().Should().Be(SampleEnum.One);
        ((short)99).AsEnum<short, SampleEnum>(SampleEnum.Two).Should().Be(SampleEnum.Two);
        new object().AsEnum<object, SampleEnum>(SampleEnum.One).Should().Be(SampleEnum.One);
    }

    [TestMethod]
    public void AsTimeSpan_CoversAllSwitchArmsHelperBranchesAndFallbackCase()
    {
        object? nullValue = null;
        TimeSpan defaultValue = TimeSpan.FromDays(1);

        nullValue.AsTimeSpan(defaultValue).Should().Be(defaultValue);
        TimeSpan.FromMinutes(5).AsTimeSpan().Should().Be(TimeSpan.FromMinutes(5));
        "00:01:30".AsTimeSpan().Should().Be(TimeSpan.FromSeconds(90));
        "invalid".AsTimeSpan(defaultValue).Should().Be(defaultValue);
        2_000L.AsTimeSpan().Should().Be(TimeSpan.FromTicks(2_000L));
        1500.AsTimeSpan().Should().Be(TimeSpan.FromMilliseconds(1500));
        250.5d.AsTimeSpan().Should().Be(TimeSpan.FromMilliseconds(250.5d));
        double.NaN.AsTimeSpan(defaultValue).Should().Be(defaultValue);
        double.MaxValue.AsTimeSpan(defaultValue).Should().Be(defaultValue);
        new object().AsTimeSpan(defaultValue).Should().Be(defaultValue);
    }

    [TestMethod]
    public void AsList_CoversAllSwitchArmsAndFallbackCase()
    {
        object? nullValue = null;
        int[] array = [1, 2, 3];
        List<int> list = [4, 5, 6];

        nullValue.AsList<object?, int>().Should().BeNull();
        array.AsList<int[], int>().Should().BeSameAs(array);
        list.AsList<List<int>, int>().Should().Equal(4, 5, 6);
        new object().AsList<object, int>().Should().BeNull();
    }

    [TestMethod]
    public void AsBooleanOrNull_CoversSupportedNumericAndFallbackCases()
    {
        object? nullValue = null;

        nullValue.AsBooleanOrNull().Should().BeNull();
        true.AsBooleanOrNull().Should().BeTrue();
        false.AsBooleanOrNull().Should().BeFalse();
        "true".AsBooleanOrNull().Should().BeTrue();
        "false".AsBooleanOrNull().Should().BeFalse();
        "invalid".AsBooleanOrNull().Should().BeNull();
        5.AsBooleanOrNull().Should().BeTrue();
        0.AsBooleanOrNull().Should().BeFalse();
        5L.AsBooleanOrNull().Should().BeTrue();
        0L.AsBooleanOrNull().Should().BeFalse();
        0.1d.AsBooleanOrNull().Should().BeTrue();
        0.0d.AsBooleanOrNull().Should().BeFalse();
        1.5m.AsBooleanOrNull().Should().BeTrue();
        0m.AsBooleanOrNull().Should().BeFalse();
        0.5f.AsBooleanOrNull().Should().BeTrue();
        0f.AsBooleanOrNull().Should().BeFalse();
        ((byte)1).AsBooleanOrNull().Should().BeTrue();
        ((byte)0).AsBooleanOrNull().Should().BeFalse();
        ((short)1).AsBooleanOrNull().Should().BeTrue();
        ((short)0).AsBooleanOrNull().Should().BeFalse();
        1u.AsBooleanOrNull().Should().BeTrue();
        0u.AsBooleanOrNull().Should().BeFalse();
        1UL.AsBooleanOrNull().Should().BeTrue();
        0UL.AsBooleanOrNull().Should().BeFalse();
        new object().AsBooleanOrNull().Should().BeNull();
    }

    [TestMethod]
    public void AsIntegerOrNull_CoversAllSwitchArmsAndRangeChecks()
    {
        object? nullValue = null;

        nullValue.AsIntegerOrNull().Should().BeNull();
        123.AsIntegerOrNull().Should().Be(123);
        true.AsIntegerOrNull().Should().Be(1);
        false.AsIntegerOrNull().Should().Be(0);
        "456".AsIntegerOrNull().Should().Be(456);
        "invalid".AsIntegerOrNull().Should().BeNull();
        123.7d.AsIntegerOrNull().Should().Be(123);
        double.MaxValue.AsIntegerOrNull().Should().BeNull();
        456.9m.AsIntegerOrNull().Should().Be(456);
        decimal.MaxValue.AsIntegerOrNull().Should().BeNull();
        789.3f.AsIntegerOrNull().Should().Be(789);
        float.MaxValue.AsIntegerOrNull().Should().BeNull();
        987654321L.AsIntegerOrNull().Should().Be(987654321);
        ((long)int.MaxValue + 1).AsIntegerOrNull().Should().BeNull();
        ((byte)12).AsIntegerOrNull().Should().Be(12);
        ((short)13).AsIntegerOrNull().Should().Be(13);
        14u.AsIntegerOrNull().Should().Be(14);
        ((uint)int.MaxValue + 1).AsIntegerOrNull().Should().BeNull();
        15UL.AsIntegerOrNull().Should().Be(15);
        ((ulong)int.MaxValue + 1).AsIntegerOrNull().Should().BeNull();
        new object().AsIntegerOrNull().Should().BeNull();
    }

    [TestMethod]
    public void AsLongOrNull_CoversAllSwitchArmsAndRangeChecks()
    {
        object? nullValue = null;

        nullValue.AsLongOrNull().Should().BeNull();
        9876543210L.AsLongOrNull().Should().Be(9876543210L);
        123.AsLongOrNull().Should().Be(123L);
        true.AsLongOrNull().Should().Be(1L);
        false.AsLongOrNull().Should().Be(0L);
        "987654321".AsLongOrNull().Should().Be(987654321L);
        "invalid".AsLongOrNull().Should().BeNull();
        123.7d.AsLongOrNull().Should().Be(123L);
        double.MaxValue.AsLongOrNull().Should().BeNull();
        456.9m.AsLongOrNull().Should().Be(456L);
        decimal.MaxValue.AsLongOrNull().Should().BeNull();
        789.3f.AsLongOrNull().Should().Be(789L);
        float.PositiveInfinity.AsLongOrNull().Should().BeNull();
        ((byte)12).AsLongOrNull().Should().Be(12L);
        ((short)13).AsLongOrNull().Should().Be(13L);
        14u.AsLongOrNull().Should().Be(14L);
        15UL.AsLongOrNull().Should().Be(15L);
        ((ulong)long.MaxValue + 1UL).AsLongOrNull().Should().BeNull();
        new object().AsLongOrNull().Should().BeNull();
    }

    [TestMethod]
    public void AsDoubleOrNull_CoversAllSwitchArmsAndFallbackCase()
    {
        object? nullValue = null;

        nullValue.AsDoubleOrNull().Should().BeNull();
        123.456d.AsDoubleOrNull().Should().Be(123.456d);
        42.AsDoubleOrNull().Should().Be(42d);
        43L.AsDoubleOrNull().Should().Be(43d);
        true.AsDoubleOrNull().Should().Be(1d);
        false.AsDoubleOrNull().Should().Be(0d);
        "987.654".AsDoubleOrNull().Should().Be(987.654d);
        "invalid".AsDoubleOrNull().Should().BeNull();
        12.25m.AsDoubleOrNull().Should().Be(12.25d);
        6.5f.AsDoubleOrNull().Should().Be(6.5d);
        ((byte)7).AsDoubleOrNull().Should().Be(7d);
        ((short)8).AsDoubleOrNull().Should().Be(8d);
        9u.AsDoubleOrNull().Should().Be(9d);
        10UL.AsDoubleOrNull().Should().Be(10d);
        new object().AsDoubleOrNull().Should().BeNull();
    }

    [TestMethod]
    public void AsDecimalOrNull_CoversAllSwitchArmsOverflowAndFallbackCase()
    {
        object? nullValue = null;

        nullValue.AsDecimalOrNull().Should().BeNull();
        123.456m.AsDecimalOrNull().Should().Be(123.456m);
        true.AsDecimalOrNull().Should().Be(1m);
        false.AsDecimalOrNull().Should().Be(0m);
        "987.654".AsDecimalOrNull().Should().Be(987.654m);
        "invalid".AsDecimalOrNull().Should().BeNull();
        42.AsDecimalOrNull().Should().Be(42m);
        43L.AsDecimalOrNull().Should().Be(43m);
        12.25d.AsDecimalOrNull().Should().Be(12.25m);
        double.MaxValue.AsDecimalOrNull().Should().BeNull();
        6.5f.AsDecimalOrNull().Should().Be(6.5m);
        float.PositiveInfinity.AsDecimalOrNull().Should().BeNull();
        ((byte)7).AsDecimalOrNull().Should().Be(7m);
        ((short)8).AsDecimalOrNull().Should().Be(8m);
        9u.AsDecimalOrNull().Should().Be(9m);
        10UL.AsDecimalOrNull().Should().Be(10m);
        new object().AsDecimalOrNull().Should().BeNull();
    }

    [TestMethod]
    public void AsFloatOrNull_CoversAllSwitchArmsAndRangeChecks()
    {
        object? nullValue = null;

        nullValue.AsFloatOrNull().Should().BeNull();
        123.456f.AsFloatOrNull().Should().Be(123.456f);
        true.AsFloatOrNull().Should().Be(1f);
        false.AsFloatOrNull().Should().Be(0f);
        float? floatResult = "987.654".AsFloatOrNull();
        floatResult.Should().NotBeNull();
        floatResult!.Value.Should().BeApproximately(987.654f, 0.001f);
        "invalid".AsFloatOrNull().Should().BeNull();
        42.AsFloatOrNull().Should().Be(42f);
        43L.AsFloatOrNull().Should().Be(43f);
        12.25d.AsFloatOrNull().Should().Be(12.25f);
        double.MaxValue.AsFloatOrNull().Should().BeNull();
        12.25m.AsFloatOrNull().Should().Be(12.25f);
        ((byte)7).AsFloatOrNull().Should().Be(7f);
        ((short)8).AsFloatOrNull().Should().Be(8f);
        9u.AsFloatOrNull().Should().Be(9f);
        10UL.AsFloatOrNull().Should().Be(10f);
        new object().AsFloatOrNull().Should().BeNull();
    }

    [TestMethod]
    public void AsStringOrNull_CoversNullDirectAndToStringCases()
    {
        object? nullValue = null;

        nullValue.AsStringOrNull().Should().BeNull();
        "text".AsStringOrNull().Should().Be("text");
        new CustomText("converted").AsStringOrNull().Should().Be("converted");
    }

    [TestMethod]
    public void AsDateTimeOrNull_CoversAllSwitchArmsAndFallbackCase()
    {
        object? nullValue = null;
        DateTimeOffset offset = new(new DateTime(2024, 6, 15, 8, 30, 0, DateTimeKind.Utc));

        nullValue.AsDateTimeOrNull().Should().BeNull();
        new DateTime(2024, 6, 15).AsDateTimeOrNull().Should().Be(new DateTime(2024, 6, 15));
        offset.AsDateTimeOrNull().Should().Be(offset.DateTime);
        "2024-01-01".AsDateTimeOrNull().Should().Be(new DateTime(2024, 1, 1));
        "invalid date".AsDateTimeOrNull().Should().BeNull();
        1_700_000_000_000L.AsDateTimeOrNull().Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1_700_000_000_000L).DateTime);
        253402300800000L.AsDateTimeOrNull().Should().BeNull();
        1_700_000_000.AsDateTimeOrNull().Should().Be(DateTimeOffset.FromUnixTimeSeconds(1_700_000_000).DateTime);
        new object().AsDateTimeOrNull().Should().BeNull();
    }

    [TestMethod]
    public void AsDateTimeOffsetOrNull_CoversAllSwitchArmsAndFallbackCase()
    {
        object? nullValue = null;
        DateTime sourceDateTime = new(2024, 6, 15, 8, 30, 0, DateTimeKind.Utc);
        DateTimeOffset expectedFromMilliseconds = DateTimeOffset.FromUnixTimeMilliseconds(1_700_000_000_000L);
        DateTimeOffset expectedFromSeconds = DateTimeOffset.FromUnixTimeSeconds(1_700_000_000);

        nullValue.AsDateTimeOffsetOrNull().Should().BeNull();
        expectedFromMilliseconds.AsDateTimeOffsetOrNull().Should().Be(expectedFromMilliseconds);
        sourceDateTime.AsDateTimeOffsetOrNull().Should().Be(new DateTimeOffset(sourceDateTime));
        "2024-01-01T00:00:00+00:00".AsDateTimeOffsetOrNull().Should().Be(DateTimeOffset.Parse("2024-01-01T00:00:00+00:00"));
        "invalid".AsDateTimeOffsetOrNull().Should().BeNull();
        1_700_000_000_000L.AsDateTimeOffsetOrNull().Should().Be(expectedFromMilliseconds);
        253402300800000L.AsDateTimeOffsetOrNull().Should().BeNull();
        1_700_000_000.AsDateTimeOffsetOrNull().Should().Be(expectedFromSeconds);
        new object().AsDateTimeOffsetOrNull().Should().BeNull();
    }

    [TestMethod]
    public void AsGuidOrNull_CoversAllSwitchArmsAndFallbackCase()
    {
        object? nullValue = null;
        Guid guid = Guid.Parse("11111111-2222-3333-4444-555555555555");

        nullValue.AsGuidOrNull().Should().BeNull();
        guid.AsGuidOrNull().Should().Be(guid);
        guid.ToString().AsGuidOrNull().Should().Be(guid);
        "invalid-guid".AsGuidOrNull().Should().BeNull();
        guid.ToByteArray().AsGuidOrNull().Should().Be(guid);
        new byte[4].AsGuidOrNull().Should().BeNull();
        new object().AsGuidOrNull().Should().BeNull();
    }

    [TestMethod]
    public void AsByteOrNull_CoversAllSwitchArmsRangeChecksAndFallbackCase()
    {
        object? nullValue = null;

        nullValue.AsByteOrNull().Should().BeNull();
        ((byte)12).AsByteOrNull().Should().Be(12);
        13.AsByteOrNull().Should().Be(13);
        256.AsByteOrNull().Should().BeNull();
        15L.AsByteOrNull().Should().Be(15);
        (-1L).AsByteOrNull().Should().BeNull();
        true.AsByteOrNull().Should().Be(1);
        false.AsByteOrNull().Should().Be(0);
        "17".AsByteOrNull().Should().Be(17);
        "invalid".AsByteOrNull().Should().BeNull();
        19.8d.AsByteOrNull().Should().Be(19);
        256d.AsByteOrNull().Should().BeNull();
        21.8m.AsByteOrNull().Should().Be(21);
        256m.AsByteOrNull().Should().BeNull();
        23.8f.AsByteOrNull().Should().Be(23);
        256f.AsByteOrNull().Should().BeNull();
        ((short)25).AsByteOrNull().Should().Be(25);
        ((short)-1).AsByteOrNull().Should().BeNull();
        27u.AsByteOrNull().Should().Be(27);
        256u.AsByteOrNull().Should().BeNull();
        new object().AsByteOrNull().Should().BeNull();
    }

    [TestMethod]
    public void AsShortOrNull_CoversAllSwitchArmsRangeChecksAndFallbackCase()
    {
        object? nullValue = null;

        nullValue.AsShortOrNull().Should().BeNull();
        ((short)12).AsShortOrNull().Should().Be(12);
        13.AsShortOrNull().Should().Be(13);
        ((int)short.MaxValue + 1).AsShortOrNull().Should().BeNull();
        15L.AsShortOrNull().Should().Be(15);
        ((long)short.MaxValue + 1).AsShortOrNull().Should().BeNull();
        true.AsShortOrNull().Should().Be(1);
        false.AsShortOrNull().Should().Be(0);
        "17".AsShortOrNull().Should().Be(17);
        "invalid".AsShortOrNull().Should().BeNull();
        19.8d.AsShortOrNull().Should().Be(19);
        40000d.AsShortOrNull().Should().BeNull();
        21.8m.AsShortOrNull().Should().Be(21);
        40000m.AsShortOrNull().Should().BeNull();
        23.8f.AsShortOrNull().Should().Be(23);
        40000f.AsShortOrNull().Should().BeNull();
        ((byte)25).AsShortOrNull().Should().Be(25);
        26u.AsShortOrNull().Should().Be(26);
        ((uint)short.MaxValue + 1).AsShortOrNull().Should().BeNull();
        new object().AsShortOrNull().Should().BeNull();
    }

    [TestMethod]
    public void AsCharOrNull_CoversAllSwitchArmsAndFallbackCase()
    {
        object? nullValue = null;

        nullValue.AsCharOrNull().Should().BeNull();
        'a'.AsCharOrNull().Should().Be('a');
        "beta".AsCharOrNull().Should().Be('b');
        string.Empty.AsCharOrNull().Should().BeNull();
        67.AsCharOrNull().Should().Be('C');
        1114112.AsCharOrNull().Should().BeNull();
        ((byte)68).AsCharOrNull().Should().Be('D');
        new object().AsCharOrNull().Should().BeNull();
    }

    [TestMethod]
    public void AsTimeSpanOrNull_CoversAllSwitchArmsHelperBranchesAndFallbackCase()
    {
        object? nullValue = null;

        nullValue.AsTimeSpanOrNull().Should().BeNull();
        TimeSpan.FromMinutes(5).AsTimeSpanOrNull().Should().Be(TimeSpan.FromMinutes(5));
        "00:01:30".AsTimeSpanOrNull().Should().Be(TimeSpan.FromSeconds(90));
        "invalid".AsTimeSpanOrNull().Should().BeNull();
        2_000L.AsTimeSpanOrNull().Should().Be(TimeSpan.FromTicks(2_000L));
        1500.AsTimeSpanOrNull().Should().Be(TimeSpan.FromMilliseconds(1500));
        250.5d.AsTimeSpanOrNull().Should().Be(TimeSpan.FromMilliseconds(250.5d));
        double.NaN.AsTimeSpanOrNull().Should().BeNull();
        double.MaxValue.AsTimeSpanOrNull().Should().BeNull();
        new object().AsTimeSpanOrNull().Should().BeNull();
    }

    [TestMethod]
    public void AsEnumOrNull_CoversAllSwitchArmsAndFallbackCase()
    {
        object? nullValue = null;

        nullValue.AsEnumOrNull<object?, SampleEnum>().Should().BeNull();
        SampleEnum.One.AsEnumOrNull<SampleEnum, SampleEnum>().Should().Be(SampleEnum.One);
        "one".AsEnumOrNull<string, SampleEnum>().Should().Be(SampleEnum.One);
        "invalid".AsEnumOrNull<string, SampleEnum>().Should().BeNull();
        1.AsEnumOrNull<int, SampleEnum>().Should().Be(SampleEnum.One);
        99.AsEnumOrNull<int, SampleEnum>().Should().BeNull();
        ((byte)2).AsEnumOrNull<byte, SampleEnum>().Should().Be(SampleEnum.Two);
        ((byte)99).AsEnumOrNull<byte, SampleEnum>().Should().BeNull();
        ((short)1).AsEnumOrNull<short, SampleEnum>().Should().Be(SampleEnum.One);
        ((short)99).AsEnumOrNull<short, SampleEnum>().Should().BeNull();
        new object().AsEnumOrNull<object, SampleEnum>().Should().BeNull();
    }

    [TestMethod]
    public void AsTypeOrNull_CoversDirectStringFallbackUnsupportedAndCatchCases()
    {
        "text".AsTypeOrNull<string, string>().Should().Be("text");
        123.AsTypeOrNull<int, string>().Should().Be("123");
        123.AsTypeOrNull<int, Uri>().Should().BeNull();
        new ThrowingToStringValue().AsTypeOrNull<ThrowingToStringValue, string>().Should().BeNull();
    }

    [TestMethod]
    public void AsValueTypeOrNull_CoversSupportedTargetsDirectFallbackAndUnsupportedCase()
    {
        "true".AsValueTypeOrNull<string, bool>().Should().BeTrue();
        "123".AsValueTypeOrNull<string, int>().Should().Be(123);
        "456".AsValueTypeOrNull<string, long>().Should().Be(456L);
        "7.5".AsValueTypeOrNull<string, double>().Should().Be(7.5d);
        "8.5".AsValueTypeOrNull<string, decimal>().Should().Be(8.5m);
        float? valueTypeFloatResult = "9.5".AsValueTypeOrNull<string, float>();
        valueTypeFloatResult.Should().NotBeNull();
        valueTypeFloatResult!.Value.Should().BeApproximately(9.5f, 0.001f);
        "2024-01-01".AsValueTypeOrNull<string, DateTime>().Should().Be(new DateTime(2024, 1, 1));
        "2024-01-01T00:00:00+00:00".AsValueTypeOrNull<string, DateTimeOffset>().Should().Be(DateTimeOffset.Parse("2024-01-01T00:00:00+00:00"));
        "11111111-2222-3333-4444-555555555555".AsValueTypeOrNull<string, Guid>().Should().Be(Guid.Parse("11111111-2222-3333-4444-555555555555"));
        "10".AsValueTypeOrNull<string, byte>().Should().Be((byte)10);
        "11".AsValueTypeOrNull<string, short>().Should().Be((short)11);
        "char".AsValueTypeOrNull<string, char>().Should().Be('c');
        "00:01:30".AsValueTypeOrNull<string, TimeSpan>().Should().Be(TimeSpan.FromSeconds(90));
        new SampleStruct(12).AsValueTypeOrNull<SampleStruct, SampleStruct>().Should().Be(new SampleStruct(12));
        new object().AsValueTypeOrNull<object, SampleStruct>().Should().BeNull();
    }

    [TestMethod]
    public void IsOfType_ReturnsExpectedResult()
    {
        object value = "text";

        value.IsOfType<string>().Should().BeTrue();
        value.IsOfType<int>().Should().BeFalse();
    }

    [TestMethod]
    public void GetUnderlyingType_ReturnsNullableInnerTypeOrOriginalType()
    {
        typeof(int?).GetUnderlyingType().Should().Be(typeof(int));
        typeof(string).GetUnderlyingType().Should().Be(typeof(string));
    }

    private enum SampleEnum
    {
        None = 0,
        One = 1,
        Two = 2
    }

    private readonly record struct SampleStruct(int Value);

    private sealed class CustomText(string value)
    {
        public override string ToString()
        {
            return value;
        }
    }

    private sealed class ThrowingToStringValue
    {
        public override string ToString()
        {
            throw new InvalidOperationException("boom");
        }
    }
}
