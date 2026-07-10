using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace NetDuid.Benchmarks;

[SimpleJob(RuntimeMoniker.Net48)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class DuidComparisonBenchmarks
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

    private Duid _smaller = null!;
    private Duid _larger = null!;
    private Duid _sameLengthLarger = null!;
    private object _boxedLarger = null!;

    [GlobalSetup]
    public void Setup()
    {
        _smaller = new Duid(LinkLayerPlusTimeBytes);
        _larger = new Duid(UuidBytes);
        _sameLengthLarger = new Duid(SameLengthDifferentBytes);
        _boxedLarger = new Duid(UuidBytes);
    }

    [Benchmark(Baseline = true)]
    public int CompareTo()
    {
        return _smaller.CompareTo(_larger);
    }

    [Benchmark]
    public int CompareToSameLength()
    {
        return _smaller.CompareTo(_sameLengthLarger);
    }

    [Benchmark]
    public bool OperatorLessThan()
    {
        return _smaller < _larger;
    }

    [Benchmark]
    public bool OperatorLessThanOrEqual()
    {
        return _smaller <= _larger;
    }

    [Benchmark]
    public bool OperatorGreaterThan()
    {
        return _larger > _smaller;
    }

    [Benchmark]
    public bool OperatorGreaterThanOrEqual()
    {
        return _larger >= _smaller;
    }
}
