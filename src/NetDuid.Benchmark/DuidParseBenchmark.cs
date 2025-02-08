using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace NetDuid.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net48)]
    [SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net70)]
    [SimpleJob(RuntimeMoniker.Net80)]
    [SimpleJob(RuntimeMoniker.Net90)]
    [MemoryDiagnoser]
    [MarkdownExporter]
    public class DuidParseBenchmark
    {
        [ParamsSource(nameof(DuidStringSource))]
        public string DuidString { get; set; }

        public IEnumerable<string> DuidStringSource()
        {
            var random = new Random(4); // 130 "random" bytes (https://xkcd.com/221/)
            var duidBytes = new byte[130];
            random.NextBytes(duidBytes);

            var delimiters = new string[] { ":", "-", " ", string.Empty };

            foreach (var delimiter in delimiters)
            {
                var duidString = BytesAsString(duidBytes, delimiter);

                yield return duidString;
                yield return duidString.ToUpperInvariant();

                var bytesAsStringNoLeadingZero = BytesAsStringNoLeadingZero(duidBytes, delimiter);

                yield return bytesAsStringNoLeadingZero;
                yield return bytesAsStringNoLeadingZero.ToUpperInvariant();
            }

#pragma warning disable IDE0062 // Convert local method to static
            string BytesAsString(byte[] input, string delimiter = null)
#pragma warning restore IDE0062
            {
                return BitConverter.ToString(input).Replace("-", delimiter ?? string.Empty);
            }

#pragma warning disable IDE0062 // Convert local method to static
            string BytesAsStringNoLeadingZero(byte[] input, string delimiter = null)
            {
#pragma warning restore IDE0062
                return BytesAsString(input, delimiter).Replace(delimiter + 0, delimiter ?? string.Empty);
            }
        }

        [Benchmark]
        public void BenchmarkParse()
        {
            _ = Duid.Parse(DuidString);
        }
    }
}
