using System.Globalization;
using VisionaryCoder.Framework.Extensions;

namespace VisionaryCoder.Framework.Tests.Aggregator;

[TestClass]
public sealed class TypeConversionContractTests
{
    [TestMethod]
    public void BooleanConversionsRetainTheirDifferentNumericScopes()
    {
        Verify(value => value.AsBoolean(), (null, false), (true, true), (false, false),
            ("true", true), ("false", false), ("bad", false), (1, true), (0, false),
            (1L, true), (0L, false), (1d, true), (double.Epsilon, false),
            (double.NaN, false), (1m, true), (0m, false), (new object(), false));
        Verify<bool?>(value => value.AsBooleanOrNull(), (null, null), (true, true),
            (false, false), ("true", true), ("false", false), ("bad", null),
            (1, true), (0, false), (1L, null));
    }

    [TestMethod]
    public void IntegerConversionsRejectOverflowIncludingRoundedFloatBoundary()
    {
        Verify(value => value.AsInteger(-9), (null, -9), (2, 2), (true, 1), (false, 0),
            ("2", 2), ("bad", -9), (2.5d, 2), (double.NaN, -9), (double.MaxValue, -9),
            (2.5m, 2), (decimal.MaxValue, -9), (2L, 2), (long.MaxValue, -9),
            (long.MinValue, -9), (2.5f, 2), ((float)int.MaxValue, -9), ((byte)2, 2),
            ((short)2, 2), (2U, 2), (uint.MaxValue, -9), (new object(), -9));
        Verify<int?>(value => value.AsIntegerOrNull(), (null, null), (2, 2), ("2", 2),
            ("bad", null), (2.5d, 2), (double.MinValue, null), (double.MaxValue, null),
            (2.5m, 2), (decimal.MinValue, null), (decimal.MaxValue, null), (2L, 2),
            (long.MinValue, null), (long.MaxValue, null), (2.5f, 2),
            (float.MinValue, null), ((float)int.MaxValue, null), (2U, 2),
            (uint.MaxValue, null), (true, null), ((double)int.MinValue, int.MinValue),
            ((double)int.MaxValue, int.MaxValue), ((float)int.MinValue, int.MinValue));
    }

    [TestMethod]
    public void LongConversionsRejectTheRoundedTwoToThe63Boundary()
    {
        Verify(value => value.AsLong(-9), (null, -9L), (2L, 2L), (2, 2L), ("2", 2L),
            ("bad", -9L), (2.5d, 2L), (double.NaN, -9L), (double.MaxValue, -9L),
            (2.5m, 2L), (decimal.MaxValue, -9L), (2.5f, 2L), (float.MaxValue, -9L),
            (2U, 2L), (2UL, 2L), (ulong.MaxValue, -9L), (true, -9L));
        Verify<long?>(value => value.AsLongOrNull(), (null, null), (2L, 2L),
            (true, 1L), (false, 0L), ("2", 2L), ("bad", null), (2.5d, 2L),
            (double.MinValue, null), ((double)long.MaxValue, null), (2.5m, 2L),
            (decimal.MinValue, null), (decimal.MaxValue, null), (2.5f, 2L),
            (float.MinValue, null), ((float)long.MaxValue, null), (2UL, 2L),
            (ulong.MaxValue, null), (2, null), ((double)long.MinValue, long.MinValue),
            ((decimal)long.MaxValue, long.MaxValue), ((float)long.MinValue, long.MinValue));
    }

    [TestMethod]
    public void FloatingAndDecimalConversionsRetainSelectiveLegacyInputs()
    {
        Verify(value => value.AsDouble(-9), (null, -9d), (2d, 2d), (2, 2d), (2L, 2d),
            (true, 1d), (false, 0d), ("2", 2d), ("bad", -9d), (2m, 2d),
            (2f, 2d), (2UL, 2d), (new object(), -9d));
        Verify(value => value.AsDecimal(-9), (null, -9m), (2m, 2m), (true, 1m),
            (false, 0m), ("2", 2m), ("bad", -9m), (2d, 2m), (2f, 2m), (2, -9m),
            (double.NaN, -9m), (double.MaxValue, -9m), (float.PositiveInfinity, -9m));
        Verify(value => value.AsFloat(-9), (null, -9f), (true, 1f), (false, 0f),
            ("2", 2f), ("bad", -9f), (2d, 2f), (2m, 2f), (2f, -9f));
        Verify<double?>(value => value.AsDoubleOrNull(), (null, null), (2d, 2d),
            ("2", 2d), ("bad", null), (2, null));
        Verify<decimal?>(value => value.AsDecimalOrNull(), (null, null), (2m, 2m),
            ("2", 2m), ("bad", null), (2, null));
        Verify<float?>(value => value.AsFloatOrNull(), (null, null), (2f, 2f),
            ("2", 2f), ("bad", null), (2d, 2f), (double.MinValue, null),
            (double.MaxValue, null), (2m, 2f), (2, null));
    }

    [TestMethod]
    public void NarrowConversionsTestBothRangeEndsAndUnsupportedIdentityQuirks()
    {
        Verify(value => value.AsByte(9), (null, (byte)9), (2, (byte)2), (-1, (byte)9),
            (256, (byte)9), (true, (byte)1), (false, (byte)0), ("2", (byte)2),
            ("bad", (byte)9), (2d, (byte)2), (-1d, (byte)9), (256d, (byte)9),
            (2m, (byte)2), (-1m, (byte)9), (256m, (byte)9), ((byte)2, (byte)9));
        Verify<byte?>(value => value.AsByteOrNull(), (null, null), ((byte)2, 2),
            (2, 2), (-1, null), (256, null), ("2", 2), ("bad", null),
            (2d, 2), (-1d, null), (256d, null), (2m, 2), (-1m, null), (256m, null), (true, null));
        Verify(value => value.AsShort(9), (null, (short)9), (2, (short)2),
            (-32769, (short)9), (32768, (short)9), (true, (short)1), (false, (short)0),
            ("2", (short)2), ("bad", (short)9), (2d, (short)2), (-32769d, (short)9),
            (32768d, (short)9), (2m, (short)2), (-32769m, (short)9),
            (32768m, (short)9), ((short)2, (short)9));
        Verify<short?>(value => value.AsShortOrNull(), (null, null), ((short)2, 2),
            (2, 2), (-32769, null), (32768, null), ("2", 2), ("bad", null),
            (2d, 2), (-32769d, null), (32768d, null), (2m, 2),
            (-32769m, null), (32768m, null), (true, null));
        Verify(value => value.AsChar('x'), (null, 'x'), ('a', 'a'), ("abc", 'a'),
            ("", 'x'), (65, 'A'), (-1, 'x'), (65536, 'x'), ((byte)65, 'A'), (true, 'x'));
        Verify<char?>(value => value.AsCharOrNull(), (null, null), ('a', 'a'),
            ("abc", 'a'), ("", null), (65, 'A'), (-1, null), (65536, null), (true, null));
    }

    [TestMethod]
    public void DatesUseSecondsForIntMillisecondsForLongAndFallbackOutsideUnixRange()
    {
        var date = new DateTime(2026, 9, 9, 0, 0, 0, DateTimeKind.Utc);
        var offset = new DateTimeOffset(date);
        Verify(value => value.AsDateTime(date), (null, date), (date, date),
            ("2026-09-09", date.Date), ("bad", date), (0L, DateTime.UnixEpoch),
            (0, DateTime.UnixEpoch), (long.MinValue, date), (long.MaxValue, date), (true, date));
        Verify(value => value.AsDateTimeOffset(offset), (null, offset), (offset, offset),
            (date, offset), ("2026-09-09T00:00:00Z", offset), ("bad", offset),
            (0L, DateTimeOffset.UnixEpoch), (0, DateTimeOffset.UnixEpoch),
            (long.MinValue, offset), (long.MaxValue, offset), (true, offset));
        Verify<DateTime?>(value => value.AsDateTimeOrNull(), (null, null), (date, date),
            (offset, date), ("2026-09-09", date.Date), ("bad", null), (true, null));
        Verify<DateTimeOffset?>(value => value.AsDateTimeOffsetOrNull(), (null, null),
            (offset, offset), ("2026-09-09T00:00:00Z", offset), ("bad", null), (date, null));
        Assert.AreEqual(DateTimeOffset.MinValue, (-62135596800000L).AsDateTimeOffset());
        Assert.AreEqual(253402300799999L, 253402300799999L.AsDateTimeOffset().ToUnixTimeMilliseconds());
    }

    [TestMethod]
    public void GuidAndSequenceConversionsPreserveOwnershipAndRejectInvalidShapes()
    {
        var guid = Guid.Parse("c427cb46-0418-43da-8d9c-657df3da5385");
        Verify(value => value.AsGuid(), (null, Guid.Empty), (guid, guid),
            (guid.ToString(), guid), ("bad", Guid.Empty), (guid.ToByteArray(), guid),
            (new byte[1], Guid.Empty), (true, Guid.Empty));
        Verify<Guid?>(value => value.AsGuidOrNull(), (null, null), (guid, guid),
            (guid.ToString(), guid), ("bad", null), (guid.ToByteArray(), guid),
            (new byte[1], null), (true, null));
        byte[] bytes = [1, 2];
        Assert.AreSame(bytes, bytes.AsByteArray());
        CollectionAssert.AreEqual(new byte[] { 65 }, "A".AsByteArray());
        CollectionAssert.AreEqual(guid.ToByteArray(), guid.AsByteArray());
        Assert.IsNull(((object?)null).AsByteArray());
        Assert.IsNull(true.AsByteArray());
        int[] values = [1, 2];
        Assert.AreSame(values, values.AsList<int[], int>());
        CollectionAssert.AreEqual(values, values.ToList().AsList<List<int>, int>());
        CollectionAssert.AreEqual(new[] { 'a', 'b' }, "ab".AsList<string, char>());
        Assert.IsNull(((object?)null).AsList<object?, int>());
        Assert.IsNull(true.AsList<bool, int>());
    }

    [TestMethod]
    public void EnumNumericsUseUnderlyingRangeNotBoxedPrimitiveIdentityOrTruncation()
    {
        Verify(value => value.AsEnum<object?, IntChoice>(IntChoice.One),
            (null, IntChoice.One), (IntChoice.Two, IntChoice.Two), ("two", IntChoice.Two),
            ("bad", IntChoice.One), (2, IntChoice.Two), (9, IntChoice.One),
            ((byte)2, IntChoice.Two), ((byte)9, IntChoice.One), ((short)2, IntChoice.Two),
            ((short)9, IntChoice.One), (2L, IntChoice.One));
        Verify<IntChoice?>(value => value.AsEnumOrNull<object?, IntChoice>(),
            (null, null), (IntChoice.Two, IntChoice.Two), ("two", IntChoice.Two),
            ("bad", null), (2, IntChoice.Two), (9, null), ((byte)2, IntChoice.Two),
            ((byte)9, null), ((short)2, IntChoice.Two), ((short)9, null), (2L, null));
        Assert.AreEqual(ByteChoice.One, 1.AsEnum<int, ByteChoice>());
        Assert.IsNull(257.AsEnumOrNull<int, ByteChoice>());
        Assert.IsNull((-1).AsEnumOrNull<int, ByteChoice>());
        Assert.AreEqual((IntChoice)99, "99".AsEnum<string, IntChoice>());
    }

    [TestMethod]
    public void TimeSpanInvalidNumericInputUsesFallback()
    {
        var span = TimeSpan.FromSeconds(1);
        Verify(value => value.AsTimeSpan(span), (null, span), (span, span),
            ("00:00:01", span), ("bad", span), (1L, TimeSpan.FromTicks(1)),
            (1, TimeSpan.FromMilliseconds(1)), (1d, TimeSpan.FromMilliseconds(1)),
            (double.NaN, span), (double.MaxValue, span), (double.NegativeInfinity, span), (true, span));
        Verify<TimeSpan?>(value => value.AsTimeSpanOrNull(), (null, null), (span, span),
            ("00:00:01", span), ("bad", null), (true, null));
    }

    [TestMethod]
    public void ReferenceConversionsRetainIdentityAndToStringFailurePolicy()
    {
        var value = new object();
        Assert.AreSame(value, value.AsTypeOrNull<object, object>());
        Assert.AreEqual("42", 42.AsTypeOrNull<int, string>());
        Assert.IsNull(value.AsTypeOrNull<object, IDisposable>());
        Assert.IsNull(((object?)null).AsTypeOrNull<object, string>());
        Assert.IsNull(new ThrowingText().AsTypeOrNull<ThrowingText, string>());
        Assert.AreEqual("fallback", new NullText().AsString("fallback"));
        Assert.AreEqual("fallback", ((object?)null).AsString("fallback"));
        Assert.AreEqual("42", 42.AsString());
        Verify<string?>(item => item.AsStringOrNull(), (null, null), ("abc", "abc"), (42, "42"));
        Assert.IsTrue(value.IsOfType<object>());
        Assert.IsFalse(value.IsOfType<string>());
        Assert.AreEqual(typeof(int), typeof(int?).GetUnderlyingType());
        Assert.AreEqual(typeof(int), typeof(int).GetUnderlyingType());
        Assert.ThrowsExactly<ArgumentNullException>(() => ((Type)null!).GetUnderlyingType());
    }

    [TestMethod]
    public void ValueTypeDispatcherCoversEachSupportedTypeAndCustomStructIdentity()
    {
        Assert.AreEqual(true, "true".AsValueTypeOrNull<string, bool>());
        Assert.AreEqual(1, "1".AsValueTypeOrNull<string, int>());
        Assert.AreEqual(1L, "1".AsValueTypeOrNull<string, long>());
        Assert.AreEqual(1d, "1".AsValueTypeOrNull<string, double>());
        Assert.AreEqual(1m, "1".AsValueTypeOrNull<string, decimal>());
        Assert.AreEqual(1f, "1".AsValueTypeOrNull<string, float>());
        Assert.AreEqual(new DateTime(2026, 9, 9), "2026-09-09".AsValueTypeOrNull<string, DateTime>());
        Assert.AreEqual(Guid.Empty, Guid.Empty.ToString().AsValueTypeOrNull<string, Guid>());
        Assert.AreEqual((byte)1, "1".AsValueTypeOrNull<string, byte>());
        Assert.AreEqual((short)1, "1".AsValueTypeOrNull<string, short>());
        Assert.AreEqual('a', "a".AsValueTypeOrNull<string, char>());
        Assert.AreEqual(TimeSpan.Zero, "00:00:00".AsValueTypeOrNull<string, TimeSpan>());
        Assert.AreEqual((1, 2), (1, 2).AsValueTypeOrNull<(int, int), (int, int)>());
        Assert.IsNull("no".AsValueTypeOrNull<string, (int, int)>());
        Assert.IsNull("no".AsValueTypeOrNull<string, int>());
    }

    [TestMethod]
    public void ParsingIsInvariantRatherThanAmbientCulture()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            Assert.AreEqual(1.5m, "1.5".AsDecimal());
            Assert.AreEqual(1500, "1,500".AsInteger());
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    private static void Verify<T>(Func<object?, T> convert, params (object? Input, T Expected)[] cases)
    {
        foreach (var item in cases)
            Assert.AreEqual(item.Expected, convert(item.Input), $"Input type: {item.Input?.GetType()}; value: {item.Input}");
    }

    private enum IntChoice { One = 1, Two = 2 }
    private enum ByteChoice : byte { One = 1 }
    private sealed class ThrowingText { public override string ToString() => throw new InvalidOperationException(); }
    private sealed class NullText { public override string ToString() => null!; }
}
