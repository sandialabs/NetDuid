using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace NetDuid.Benchmarks;

[SimpleJob(RuntimeMoniker.Net48)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class DuidEqualityBenchmarks
{
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

    private static readonly byte[] UuidBytes =
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

    private static readonly byte[] SameLengthDifferentBytes =
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
        0xff,
    ];

    private Duid _duid = null!;
    private Duid _equalDuid = null!;
    private Duid _differentLengthDuid = null!;
    private Duid _sameLengthDifferentDuid = null!;
    private object _boxedDuid = null!;
    private object _nonDuidObject = null!;

    [GlobalSetup]
    public void Setup()
    {
        _duid = new Duid(LinkLayerPlusTimeBytes);
        _equalDuid = new Duid(LinkLayerPlusTimeBytes);
        _differentLengthDuid = new Duid(UuidBytes);
        _sameLengthDifferentDuid = new Duid(SameLengthDifferentBytes);
        _boxedDuid = new Duid(LinkLayerPlusTimeBytes);
        _nonDuidObject = "not a duid";
    }

    [Benchmark(Baseline = true)]
    public int GetHashCodeBench()
    {
        return _duid.GetHashCode();
    }

    [Benchmark]
    public int GetHashCodeDifferent()
    {
        return _sameLengthDifferentDuid.GetHashCode();
    }

    [Benchmark]
    public bool EqualsDuid()
    {
        return _duid.Equals(_equalDuid);
    }

    [Benchmark]
    public bool EqualsSameLengthDifferentBytes()
    {
        return _duid.Equals(_sameLengthDifferentDuid);
    }

    [Benchmark]
    public bool EqualsDifferentLength()
    {
        return _duid.Equals(_differentLengthDuid);
    }

    [Benchmark]
    public bool EqualsObject()
    {
        return _duid.Equals(_boxedDuid);
    }

    [Benchmark]
    public bool EqualsObjectNonDuid()
    {
        return _duid.Equals(_nonDuidObject);
    }

    [Benchmark]
    public bool OperatorEquals()
    {
        return _duid == _equalDuid;
    }

    [Benchmark]
    public bool OperatorNotEquals()
    {
        return _duid != _differentLengthDuid;
    }
}
