// NetDuid NuGet package smoke tests.
//
// This program is a consumer of the *packed NuGet package* (not a project reference).
// Its only job is to prove that each target-framework asset (netstandard2.0 via net48,
// net8.0, net9.0, net10.0) loads correctly and that the public API behaves at runtime.
//
// Run via smoketests/run-smoke-tests.{sh,ps1} — those scripts pack the library first.

using System;
using System.Collections.Generic;
using System.Linq;
using NetDuid;

var tfm = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
Console.WriteLine($"=== NetDuid Smoke Tests ({tfm}) ===");
Console.WriteLine();

var passed = 0;
var failed = 0;

void Check(string name, Action test)
{
    try
    {
        test();
        Console.WriteLine($"  [PASS] {name}");
        passed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  [FAIL] {name}: {ex.Message}");
        failed++;
    }
}

void Require(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

void RequireEqual<T>(T expected, T actual, string field)
{
    if (!Equals(expected, actual))
        throw new InvalidOperationException($"{field}: expected '{expected}', got '{actual}'");
}

// ---------------------------------------------------------------------------
// Duid — all type variants
// ---------------------------------------------------------------------------
Console.WriteLine("Duid (all types)");

var llTimeBytes = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x11, 0x22, 0x33, 0x44, 0x55 };
var vendorBytes = new byte[] { 0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x01, 0x02, 0x03 };
var linkLayerBytes = new byte[] { 0x00, 0x03, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05 };
var uuidBytes = new byte[]
{
    0x00,
    0x04,
    0x01,
    0x02,
    0x03,
    0x04,
    0x05,
    0x06,
    0x07,
    0x08,
    0x09,
    0x0a,
    0x0b,
    0x0c,
    0x0d,
    0x0e,
    0x0f,
    0x10,
};

Check(
    "LinkLayerPlusTime type detection",
    () =>
    {
        var duid = new Duid(llTimeBytes);
        RequireEqual(DuidType.LinkLayerPlusTime, duid.Type, "Type");
    }
);

Check(
    "VendorAssigned type detection",
    () =>
    {
        var duid = new Duid(vendorBytes);
        RequireEqual(DuidType.VendorAssigned, duid.Type, "Type");
    }
);

Check(
    "LinkLayer type detection",
    () =>
    {
        var duid = new Duid(linkLayerBytes);
        RequireEqual(DuidType.LinkLayer, duid.Type, "Type");
    }
);

Check(
    "Uuid type detection",
    () =>
    {
        var duid = new Duid(uuidBytes);
        RequireEqual(DuidType.Uuid, duid.Type, "Type");
    }
);

Check(
    "Undefined type for unknown type code",
    () =>
    {
        var duid = new Duid(new byte[] { 0xFF, 0xFE, 0x01, 0x02 });
        RequireEqual(DuidType.Undefined, duid.Type, "Type");
    }
);

// ---------------------------------------------------------------------------
// Parse / TryParse
// ---------------------------------------------------------------------------
Console.WriteLine("\nParse / TryParse");

var colonString = "00:01:00:00:00:00:00:01:00:11:22:33:44:55";
var dashString = "00-01-00-00-00-00-00-01-00-11-22-33-44-55";
var spaceString = "00 01 00 00 00 00 00 01 00 11 22 33 44 55";
var noDelimString = "0001000000000001001122334455";

Check(
    "Parse colon-delimited",
    () =>
    {
        var duid = Duid.Parse(colonString);
        RequireEqual(DuidType.LinkLayerPlusTime, duid.Type, "Type");
        RequireEqual(14, duid.GetBytes().Count, "byte count");
    }
);

Check(
    "Parse dash-delimited",
    () =>
    {
        var duid = Duid.Parse(dashString);
        RequireEqual(DuidType.LinkLayerPlusTime, duid.Type, "Type");
    }
);

Check(
    "Parse space-delimited",
    () =>
    {
        var duid = Duid.Parse(spaceString);
        RequireEqual(DuidType.LinkLayerPlusTime, duid.Type, "Type");
    }
);

Check(
    "Parse undelimited",
    () =>
    {
        var duid = Duid.Parse(noDelimString);
        RequireEqual(DuidType.LinkLayerPlusTime, duid.Type, "Type");
    }
);

Check(
    "Parse leading-zero-omitted pairs",
    () =>
    {
        var duid = Duid.Parse("1:2:A3:B4");
        RequireEqual(4, duid.GetBytes().Count, "byte count");
    }
);

Check(
    "Parse trims whitespace",
    () =>
    {
        var duid = Duid.Parse("  00:01:02:03  ");
        RequireEqual(4, duid.GetBytes().Count, "byte count");
    }
);

Check(
    "Parse is case-insensitive",
    () =>
    {
        var lower = Duid.Parse("ab:cd:ef:01:02:03");
        var upper = Duid.Parse("AB:CD:EF:01:02:03");
        Require(lower.Equals(upper), "case-insensitive parse yields equal DUIDs");
    }
);

Check(
    "TryParse success",
    () =>
    {
        Require(Duid.TryParse("00:01:00:01:02:03", out var duid), "TryParse returned true");
        Require(duid != null, "duid not null");
    }
);

Check(
    "TryParse failure on garbage",
    () =>
    {
        Require(!Duid.TryParse("not-a-duid", out _), "TryParse returned false");
    }
);

Check(
    "Mixed delimiters rejected",
    () =>
    {
        Require(!Duid.TryParse("01:02-A3:B4", out _), "mixed delimiters rejected");
    }
);

Check(
    "Parse rejects null",
    () =>
    {
        try
        {
            Duid.Parse(null!);
            throw new InvalidOperationException("should have thrown");
        }
        catch (ArgumentException)
        {
            // expected
        }
    }
);

Check(
    "Parse rejects empty string",
    () =>
    {
        try
        {
            Duid.Parse(string.Empty);
            throw new InvalidOperationException("should have thrown");
        }
        catch (ArgumentException)
        {
            // expected
        }
    }
);

// ---------------------------------------------------------------------------
// IFormattable / ToString
// ---------------------------------------------------------------------------
Console.WriteLine("\nIFormattable / ToString");

var hexDuid = Duid.Parse("ab:cd:ef:01:02:03");

Check(
    "ToString() parameterless",
    () =>
    {
        var s = hexDuid.ToString();
        RequireEqual("AB:CD:EF:01:02:03", s, "parameterless ToString");
    }
);

Check(
    "ToString default format (uppercase colon)",
    () =>
    {
        var s = hexDuid.ToString(null, null);
        RequireEqual("AB:CD:EF:01:02:03", s, "default format");
    }
);

Check(
    "ToString uppercase dash",
    () =>
    {
        var s = hexDuid.ToString("U-", null);
        RequireEqual("AB-CD-EF-01-02-03", s, "U- format");
    }
);

Check(
    "ToString uppercase no delimiter",
    () =>
    {
        var s = hexDuid.ToString("U", null);
        RequireEqual("ABCDEF010203", s, "U format");
    }
);

Check(
    "ToString lowercase colon",
    () =>
    {
        var s = hexDuid.ToString("L:", null);
        RequireEqual("ab:cd:ef:01:02:03", s, "L: format");
    }
);

Check(
    "ToString lowercase dash",
    () =>
    {
        var s = hexDuid.ToString("L-", null);
        RequireEqual("ab-cd-ef-01-02-03", s, "L- format");
    }
);

Check(
    "ToString lowercase no delimiter",
    () =>
    {
        var s = hexDuid.ToString("L", null);
        RequireEqual("abcdef010203", s, "L format");
    }
);

Check(
    "ToString empty format defaults to uppercase colon",
    () =>
    {
        var s = hexDuid.ToString("", null);
        RequireEqual("AB:CD:EF:01:02:03", s, "empty format");
    }
);

Check(
    "ToString invalid format throws FormatException",
    () =>
    {
        try
        {
            hexDuid.ToString("X", null);
            throw new InvalidOperationException("should have thrown");
        }
        catch (FormatException)
        {
            // expected
        }
    }
);

// ---------------------------------------------------------------------------
// IParsable<Duid> (NET7+ only)
// ---------------------------------------------------------------------------
#if NET7_0_OR_GREATER
Console.WriteLine("\nIParsable<Duid>");

Check(
    "Parse with IFormatProvider",
    () =>
    {
        var duid = Duid.Parse("ab:cd:ef:01:02:03", (IFormatProvider)null);
        RequireEqual(6, duid.GetBytes().Count, "byte count");
    }
);

Check(
    "TryParse with IFormatProvider",
    () =>
    {
        Require(Duid.TryParse("ab:cd:ef:01:02:03", (IFormatProvider)null, out var duid), "TryParse succeeded");
        Require(duid != null, "duid not null");
    }
);

Check(
    "TryParse with IFormatProvider failure",
    () =>
    {
        Require(!Duid.TryParse("garbage", (IFormatProvider)null, out _), "garbage returns false");
    }
);
#endif

// ---------------------------------------------------------------------------
// Equality
// ---------------------------------------------------------------------------
Console.WriteLine("\nEquality");

var a = new Duid(llTimeBytes);
var b = new Duid(llTimeBytes);
var c = new Duid(uuidBytes);

Check(
    "Equals same bytes",
    () =>
    {
        Require(a.Equals(b), "a.Equals(b)");
        Require(b.Equals(a), "b.Equals(a)");
    }
);

Check(
    "Equals different bytes",
    () =>
    {
        Require(!a.Equals(c), "a.Equals(c) is false");
    }
);

Check(
    "Equals null",
    () =>
    {
        Require(!a.Equals(null), "a.Equals(null) is false");
    }
);

Check(
    "operator ==",
    () =>
    {
        Require(a == b, "a == b");
        Require(!(a == c), "a != c");
    }
);

Check(
    "operator !=",
    () =>
    {
        Require(a != c, "a != c");
        Require(!(a != b), "not (a != b)");
    }
);

Check(
    "GetHashCode equal for equal DUIDs",
    () =>
    {
        RequireEqual(a.GetHashCode(), b.GetHashCode(), "hash codes");
    }
);

// ---------------------------------------------------------------------------
// Comparison
// ---------------------------------------------------------------------------
Console.WriteLine("\nComparison");

var small = new Duid(new byte[] { 0x00, 0x01, 0x00, 0x01 });
var medium = new Duid(new byte[] { 0x00, 0x01, 0x00, 0x01, 0x02 });
var large = new Duid(new byte[] { 0x00, 0x01, 0x00, 0x01, 0x02, 0x03 });

Check(
    "CompareTo shorter < longer",
    () =>
    {
        Require(small.CompareTo(medium) < 0, "small < medium");
        Require(medium.CompareTo(small) > 0, "medium > small");
    }
);

Check(
    "CompareTo equal lengths",
    () =>
    {
        var x = new Duid(new byte[] { 0x00, 0x01, 0x00, 0x01, 0x00 });
        var y = new Duid(new byte[] { 0x00, 0x01, 0x00, 0x01, 0x01 });
        Require(x.CompareTo(y) < 0, "x < y");
        Require(y.CompareTo(x) > 0, "y > x");
        Require(x.CompareTo(x) == 0, "x == x");
    }
);

Check(
    "operator < and >",
    () =>
    {
        Require(small < medium, "small < medium");
        Require(medium > small, "medium > small");
        Require(!(small > medium), "not (small > medium)");
    }
);

Check(
    "operator <= and >=",
    () =>
    {
        var small2 = new Duid(new byte[] { 0x00, 0x01, 0x00, 0x01 });
        Require(small <= medium, "small <= medium");
        Require(small2 <= small, "small <= small");
        Require(medium >= small, "medium >= small");
        Require(small2 >= small, "small >= small");
    }
);

// ---------------------------------------------------------------------------
// Null operator semantics
// ---------------------------------------------------------------------------
Console.WriteLine("\nNull operator semantics");

Duid nonNull = new Duid(new byte[] { 0x00, 0x01, 0x02 });
Duid nullDuid = null;

Check(
    "null == null is true",
    () =>
    {
        Require(nullDuid == null, "null == null");
    }
);

Check(
    "null != non-null is true",
    () =>
    {
        Require(nullDuid != nonNull, "null != nonNull");
    }
);

Check(
    "non-null > null is true",
    () =>
    {
        Require(nonNull > nullDuid, "nonNull > null");
    }
);

Check(
    "null < non-null is true",
    () =>
    {
        Require(nullDuid < nonNull, "null < nonNull");
    }
);

Check(
    "null >= null is true",
    () =>
    {
        Require(nullDuid >= null, "null >= null");
    }
);

Check(
    "null <= null is true",
    () =>
    {
        Require(nullDuid <= null, "null <= null");
    }
);

Check(
    "non-null >= null is true",
    () =>
    {
        Require(nonNull >= nullDuid, "nonNull >= null");
    }
);

Check(
    "null <= non-null is true",
    () =>
    {
        Require(nullDuid <= nonNull, "null <= nonNull");
    }
);

// ---------------------------------------------------------------------------
// GetBytes
// ---------------------------------------------------------------------------
Console.WriteLine("\nGetBytes");

Check(
    "GetBytes returns correct bytes",
    () =>
    {
        var duid = new Duid(llTimeBytes);
        var bytes = duid.GetBytes();
        RequireEqual(llTimeBytes.Length, bytes.Count, "count");
        Require(bytes.SequenceEqual(llTimeBytes), "content matches");
    }
);

Check(
    "GetBytes is read-only snapshot",
    () =>
    {
        var original = new byte[] { 0x00, 0x01, 0x02 };
        var duid = new Duid(original);
        var bytes = duid.GetBytes();
        var first = bytes.First();
        original[0] = 0xFF;
        Require(bytes.First() == first, "GetBytes returns a snapshot, not a live view");
    }
);

// ---------------------------------------------------------------------------
// Edge Cases
// ---------------------------------------------------------------------------
Console.WriteLine("\nEdge Cases");

Check(
    "Minimum valid DUID (3 bytes)",
    () =>
    {
        var duid = new Duid(new byte[] { 0x00, 0x01, 0x00 });
        RequireEqual(DuidType.LinkLayerPlusTime, duid.Type, "Type");
        RequireEqual(3, duid.GetBytes().Count, "count");
    }
);

Check(
    "Maximum valid DUID (130 bytes)",
    () =>
    {
        var bytes = new byte[130];
        bytes[0] = 0x00;
        bytes[1] = 0x04;
        var duid = new Duid(bytes);
        RequireEqual(DuidType.Uuid, duid.Type, "Type");
        RequireEqual(130, duid.GetBytes().Count, "count");
    }
);

Check(
    "Constructor rejects null bytes",
    () =>
    {
        try
        {
            _ = new Duid(null!);
            throw new InvalidOperationException("should have thrown");
        }
        catch (ArgumentNullException)
        {
            // expected
        }
    }
);

Check(
    "Constructor rejects empty bytes",
    () =>
    {
        try
        {
            _ = new Duid(Array.Empty<byte>());
            throw new InvalidOperationException("should have thrown");
        }
        catch (ArgumentException)
        {
            // expected
        }
    }
);

Check(
    "Constructor rejects less than 3 bytes",
    () =>
    {
        try
        {
            _ = new Duid(new byte[] { 0x00, 0x01 });
            throw new InvalidOperationException("should have thrown");
        }
        catch (ArgumentException)
        {
            // expected
        }
    }
);

Check(
    "Constructor rejects more than 130 bytes",
    () =>
    {
        try
        {
            _ = new Duid(new byte[131]);
            throw new InvalidOperationException("should have thrown");
        }
        catch (ArgumentException)
        {
            // expected
        }
    }
);

// ---------------------------------------------------------------------------
// Summary
// ---------------------------------------------------------------------------
Console.WriteLine();
Console.WriteLine($"Results: {passed} passed, {failed} failed.");

if (failed > 0)
    Environment.Exit(1);
