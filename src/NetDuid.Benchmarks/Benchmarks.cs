using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NetDuid;

namespace NetDuid.Benchmarks
{
    public class DuidBenchmarks
    {
        private static readonly byte[] LinkLayerPlusTimeBytes = new byte[]
        {
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
        };

        private static readonly byte[] UuidBytes = new byte[]
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

        private readonly Duid _duid = new(LinkLayerPlusTimeBytes);

        [Benchmark]
        public Duid CreateFromBytes() => new(LinkLayerPlusTimeBytes);

        [Benchmark]
        public Duid ParseUndelimited() => Duid.Parse("000100000000000100112233445566");

        [Benchmark]
        public Duid ParseDelimited() => Duid.Parse("00:01:00:00:00:00:00:01:00:11:22:33:44:55");

        [Benchmark]
        public string FormatDefault() => _duid.ToString(null, null);

        [Benchmark]
        public string FormatLowerCase() => _duid.ToString("L:", null);

        [Benchmark]
        public int GetHashCodeBench() => _duid.GetHashCode();

        [Benchmark]
        public bool Equality() => _duid.Equals(new Duid(LinkLayerPlusTimeBytes));

        [Benchmark]
        public int CompareTo() => _duid.CompareTo(new Duid(UuidBytes));

        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<DuidBenchmarks>(args: args);
        }
    }
}
