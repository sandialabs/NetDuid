using System.Runtime.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

#pragma warning disable SYSLIB0050

namespace NetDuid.Benchmarks;

[SimpleJob(RuntimeMoniker.Net48)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class DuidMiscBenchmarks
{
    private static readonly byte[] UndefinedDuid = [0x00, 0x05, 0x01];

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

    private static readonly byte[] VendorAssignedDuid = [0x00, 0x02, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06];

    private static readonly byte[] LinkLayerDuid = [0x00, 0x03, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06];

    private static readonly byte[] UuidDuid =
    [
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
    ];

    private static readonly byte[] ThreeByteDuid = [0x00, 0x03, 0x01];

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

    [ParamsSource(nameof(DuidTypeBytesSource))]
    public byte[] DuidTypeBytes { get; set; } = null!;

    public static IEnumerable<object> DuidTypeBytesSource()
    {
        yield return UndefinedDuid;
        yield return LinkLayerPlusTimeBytes;
        yield return VendorAssignedDuid;
        yield return LinkLayerDuid;
        yield return UuidDuid;
    }

    private Duid _duid = null!;
    private Duid _threeByteDuid = null!;
    private Duid _midDuid = null!;
    private Duid _maxDuid = null!;
    private SerializationInfo _serializationInfo = null!;

    [GlobalSetup]
    public void Setup()
    {
        _duid = new Duid(DuidTypeBytes);
        _threeByteDuid = new Duid(ThreeByteDuid);
        _midDuid = new Duid(LinkLayerPlusTimeBytes);
        _maxDuid = new Duid(MaxDuid);
        _serializationInfo = new SerializationInfo(typeof(Duid), new FormatterConverter());
    }

    [Benchmark(Baseline = true)]
    public DuidType DuidType()
    {
        return _duid.Type;
    }

    [Benchmark]
    public IReadOnlyCollection<byte> GetBytesThreeByte()
    {
        return _threeByteDuid.GetBytes();
    }

    [Benchmark]
    public IReadOnlyCollection<byte> GetBytesMid()
    {
        return _midDuid.GetBytes();
    }

    [Benchmark]
    public IReadOnlyCollection<byte> GetBytesMax()
    {
        return _maxDuid.GetBytes();
    }

    [Benchmark]
    public void SerializeGetObjectData()
    {
        _duid.GetObjectData(_serializationInfo, new StreamingContext());
    }
}
