using BenchmarkDotNet.Running;

namespace NetDuid.Benchmark
{
    internal class Program
    {
        // dotnet run -c Release --framework net9.0 -- --job short
        private static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
