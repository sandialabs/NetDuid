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
// Link-Layer Plus Time DUID (type code 0x0001)
// ---------------------------------------------------------------------------
Console.WriteLine("Link-Layer Plus Time DUID");

var llTimeBytes = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x11, 0x22, 0x33, 0x44, 0x55 };
var llTimeString = "00:01:00:00:00:00:00:01:00:11:22:33:44:55";
var llTimeStringDash = "00-01-00-00-00-00-00-01-00-11-22-33-44-55";
var llTimeStringNoDelim = "000100000000000100112233445566";

Check(
    "Create from bytes",
    () =>
    {
        var duid = new Duid(llTimeBytes);
        RequireEqual(DuidType.LinkLayerPlusTime, duid.Type, "Type");
    }
);

Check(
    "Parse colon-delimited string",
    () =>
    {
        var duid = Duid.Parse(llTimeString);
        RequireEqual(DuidType.LinkLayerPlusTime, duid.Type, "Type");
        RequireEqual(14, duid.GetBytes().Count, "byte count");
    }
);

Check(
    "Parse dash-delimited string",
    () =>
    {
        var duid = Duid.Parse(llTimeStringDash);
        RequireEqual(DuidType.LinkLayerPlusTime, duid.Type, "Type");
    }
);

Check(
    "Parse undelimited string",
    () =>
    {
        var duid = Duid.Parse(llTimeStringNoDelim);
        RequireEqual(DuidType.LinkLayerPlusTime, duid.Type, "Type");
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

// ---------------------------------------------------------------------------
// Vendor-Assigned DUID (type code 0x0002)
// ---------------------------------------------------------------------------
Console.WriteLine("\nVendor-Assigned DUID");

Check(
    "VendorAssigned type detection",
    () =>
    {
        var duid = new Duid(new byte[] { 0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x01, 0x02, 0x03 });
        RequireEqual(DuidType.VendorAssigned, duid.Type, "Type");
    }
);

// ---------------------------------------------------------------------------
// Link-Layer DUID (type code 0x0003)
// ---------------------------------------------------------------------------
Console.WriteLine("\nLink-Layer DUID");

Check(
    "LinkLayer type detection",
    () =>
    {
        var duid = new Duid(new byte[] { 0x00, 0x03, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05 });
        RequireEqual(DuidType.LinkLayer, duid.Type, "Type");
    }
);

// ---------------------------------------------------------------------------
// UUID DUID (type code 0x0004) per RFC 6355
// ---------------------------------------------------------------------------
Console.WriteLine("\nUUID DUID");

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
    "UUID type detection",
    () =>
    {
        var duid = new Duid(uuidBytes);
        RequireEqual(DuidType.Uuid, duid.Type, "Type");
    }
);

// ---------------------------------------------------------------------------
// Undefined DUID (unknown type codes, or single byte)
// ---------------------------------------------------------------------------
Console.WriteLine("\nUndefined DUID");

Check(
    "Unknown type code returns Undefined",
    () =>
    {
        var duid = new Duid(new byte[] { 0xFF, 0xFE, 0x01, 0x02 });
        RequireEqual(DuidType.Undefined, duid.Type, "Type");
    }
);

// ---------------------------------------------------------------------------
// ToString / IFormattable
// ---------------------------------------------------------------------------
Console.WriteLine("\nIFormattable / ToString");

var hexDuid = Duid.Parse("ab:cd:ef:01:02:03");

Check(
    "ToString default (uppercase colon)",
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
    "ToString uppercase no delimiter",
    () =>
    {
        var s = hexDuid.ToString("U", null);
        RequireEqual("ABCDEF010203", s, "U format");
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

// ---------------------------------------------------------------------------
// Equality and HashCode
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
        var duid = new Duid(llTimeBytes);
        var bytes = duid.GetBytes();
        var first = bytes.First();
        llTimeBytes[0] = 0xFF;
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
    "Parse rejects null",
    () =>
    {
        try
        {
            Duid.Parse(null);
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
            Duid.Parse("");
            throw new InvalidOperationException("should have thrown");
        }
        catch (ArgumentException)
        {
            // expected
        }
    }
);

Check(
    "Constructor rejects null bytes",
    () =>
    {
        try
        {
            _ = new Duid(null);
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

Check(
    "TryParse returns false for garbage",
    () =>
    {
        Require(!Duid.TryParse("ZZZ", out _), "garbage input");
    }
);

// ---------------------------------------------------------------------------
// Summary
// ---------------------------------------------------------------------------
Console.WriteLine();
Console.WriteLine($"Results: {passed} passed, {failed} failed.");

if (failed > 0)
    Environment.Exit(1);
