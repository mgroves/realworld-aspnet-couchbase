using BenchmarkDotNet.Running;

namespace Conduit.Benchmarks
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<SetVsListVsArray>();
        }
    }
}