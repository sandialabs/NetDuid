using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace NetDuid.Benchmarks;

[SimpleJob(RuntimeMoniker.Net48)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class DuidFactoryBenchmarks
{
    private static readonly byte[] ThreeByteDuid = [0x00, 0x03, 0x01];

    private static readonly byte[] LinkLayerPlusTimeBytes =
    [
        0x00,
        0x01,
        0x00,
        0x00,
        0x00,
        0x00,
        0x00,
        0x01,
        0x00,
        0x11,
        0x22,
        0x33,
        0x44,
        0x55,
    ];

    private static readonly byte[] MaxDuid =
    [
        0x00,
        0x01,
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
        0x11,
        0x12,
        0x13,
        0x14,
        0x15,
        0x16,
        0x17,
        0x18,
        0x19,
        0x1a,
        0x1b,
        0x1c,
        0x1d,
        0x1e,
        0x1f,
        0x20,
        0x21,
        0x22,
        0x23,
        0x24,
        0x25,
        0x26,
        0x27,
        0x28,
        0x29,
        0x2a,
        0x2b,
        0x2c,
        0x2d,
        0x2e,
        0x2f,
        0x30,
        0x31,
        0x32,
        0x33,
        0x34,
        0x35,
        0x36,
        0x37,
        0x38,
        0x39,
        0x3a,
        0x3b,
        0x3c,
        0x3d,
        0x3e,
        0x3f,
        0x40,
        0x41,
        0x42,
        0x43,
        0x44,
        0x45,
        0x46,
        0x47,
        0x48,
        0x49,
        0x4a,
        0x4b,
        0x4c,
        0x4d,
        0x4e,
        0x4f,
        0x50,
        0x51,
        0x52,
        0x53,
        0x54,
        0x55,
        0x56,
        0x57,
        0x58,
        0x59,
        0x5a,
        0x5b,
        0x5c,
        0x5d,
        0x5e,
        0x5f,
        0x60,
        0x61,
        0x62,
        0x63,
        0x64,
        0x65,
        0x66,
        0x67,
        0x68,
        0x69,
        0x6a,
        0x6b,
        0x6c,
        0x6d,
        0x6e,
        0x6f,
        0x70,
        0x71,
        0x72,
        0x73,
        0x74,
        0x75,
        0x76,
        0x77,
        0x78,
        0x79,
        0x7a,
        0x7b,
        0x7c,
        0x7d,
        0x7e,
        0x7f,
        0x80,
        0x81,
    ];

    private const string ThreeByteUndelimited = "000301";
    private const string ThreeByteColonDelimited = "00:03:01";
    private const string ThreeByteDashDelimited = "00-03-01";
    private const string ThreeByteSpaceDelimited = "00 03 01";

    private const string MidUndelimited = "000100000000000100112233445566";
    private const string MidColonDelimited = "00:01:00:00:00:00:00:01:00:11:22:33:44:55";
    private const string MidDashDelimited = "00-01-00-00-00-00-00-01-00-11-22-33-44-55";
    private const string MidSpaceDelimited = "00 01 00 00 00 00 00 01 00 11 22 33 44 55";

    private const string MaxUndelimited =
        "00010102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f"
        + "202122232425262728292a2b2c2d2e2f303132333435363738393a3b3c3d3e3f"
        + "404142434445464748494a4b4c4d4e4f505152535455565758595a5b5c5d5e5f"
        + "606162636465666768696a6b6c6d6e6f707172737475767778797a7b7c7d7e7f"
        + "8081";

    private const string MaxColonDelimited =
        "00:01:01:02:03:04:05:06:07:08:09:0a:0b:0c:0d:0e:0f:10:11:12:13:14:15:16:17:18:19:1a:1b:1c:1d:1e:1f:"
        + "20:21:22:23:24:25:26:27:28:29:2a:2b:2c:2d:2e:2f:30:31:32:33:34:35:36:37:38:39:3a:3b:3c:3d:3e:3f:"
        + "40:41:42:43:44:45:46:47:48:49:4a:4b:4c:4d:4e:4f:50:51:52:53:54:55:56:57:58:59:5a:5b:5c:5d:5e:5f:"
        + "60:61:62:63:64:65:66:67:68:69:6a:6b:6c:6d:6e:6f:70:71:72:73:74:75:76:77:78:79:7a:7b:7c:7d:7e:7f:"
        + "80:81";

    private const string MaxDashDelimited =
        "00-01-01-02-03-04-05-06-07-08-09-0a-0b-0c-0d-0e-0f-10-11-12-13-14-15-16-17-18-19-1a-1b-1c-1d-1e-1f-"
        + "20-21-22-23-24-25-26-27-28-29-2a-2b-2c-2d-2e-2f-30-31-32-33-34-35-36-37-38-39-3a-3b-3c-3d-3e-3f-"
        + "40-41-42-43-44-45-46-47-48-49-4a-4b-4c-4d-4e-4f-50-51-52-53-54-55-56-57-58-59-5a-5b-5c-5d-5e-5f-"
        + "60-61-62-63-64-65-66-67-68-69-6a-6b-6c-6d-6e-6f-70-71-72-73-74-75-76-77-78-79-7a-7b-7c-7d-7e-7f-"
        + "80-81";

    private const string MaxSpaceDelimited =
        "00 01 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f 10 11 12 13 14 15 16 17 18 19 1a 1b 1c 1d 1e 1f "
        + "20 21 22 23 24 25 26 27 28 29 2a 2b 2c 2d 2e 2f 30 31 32 33 34 35 36 37 38 39 3a 3b 3c 3d 3e 3f "
        + "40 41 42 43 44 45 46 47 48 49 4a 4b 4c 4d 4e 4f 50 51 52 53 54 55 56 57 58 59 5a 5b 5c 5d 5e 5f "
        + "60 61 62 63 64 65 66 67 68 69 6a 6b 6c 6d 6e 6f 70 71 72 73 74 75 76 77 78 79 7a 7b 7c 7d 7e 7f "
        + "80 81";

    [ParamsSource(nameof(DuidBytesSource))]
    public byte[] DuidBytes { get; set; } = null!;

    public static IEnumerable<byte[]> DuidBytesSource()
    {
        yield return ThreeByteDuid;
        yield return LinkLayerPlusTimeBytes;
        yield return MaxDuid;
    }

    [Benchmark(Baseline = true)]
    public Duid CreateFromBytes()
    {
        return new Duid(DuidBytes);
    }

    [Benchmark]
    public Duid CreateFromList()
    {
        return new Duid(new List<byte>(DuidBytes));
    }

    [Benchmark]
    public Duid ParseUndelimited()
    {
        if (DuidBytes == ThreeByteDuid)
        {
            return Duid.Parse(ThreeByteUndelimited);
        }

        return DuidBytes == LinkLayerPlusTimeBytes ? Duid.Parse(MidUndelimited) : Duid.Parse(MaxUndelimited);
    }

    [Benchmark]
    public Duid ParseColonDelimited()
    {
        if (DuidBytes == ThreeByteDuid)
        {
            return Duid.Parse(ThreeByteColonDelimited);
        }

        return DuidBytes == LinkLayerPlusTimeBytes ? Duid.Parse(MidColonDelimited) : Duid.Parse(MaxColonDelimited);
    }

    [Benchmark]
    public Duid ParseDashDelimited()
    {
        if (DuidBytes == ThreeByteDuid)
        {
            return Duid.Parse(ThreeByteDashDelimited);
        }

        return DuidBytes == LinkLayerPlusTimeBytes ? Duid.Parse(MidDashDelimited) : Duid.Parse(MaxDashDelimited);
    }

    [Benchmark]
    public Duid ParseSpaceDelimited()
    {
        if (DuidBytes == ThreeByteDuid)
        {
            return Duid.Parse(ThreeByteSpaceDelimited);
        }

        return DuidBytes == LinkLayerPlusTimeBytes ? Duid.Parse(MidSpaceDelimited) : Duid.Parse(MaxSpaceDelimited);
    }

    [Benchmark]
    public bool TryParse()
    {
        if (DuidBytes == ThreeByteDuid)
        {
            return Duid.TryParse(ThreeByteColonDelimited, out _);
        }

        return DuidBytes == LinkLayerPlusTimeBytes
            ? Duid.TryParse(MidColonDelimited, out _)
            : Duid.TryParse(MaxColonDelimited, out _);
    }

    [Benchmark]
    public bool TryParseFailure()
    {
        return Duid.TryParse("not-a-duid", out _);
    }
}
