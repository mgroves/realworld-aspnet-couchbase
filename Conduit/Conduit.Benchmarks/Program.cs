using BenchmarkDotNet.Running;
using System;

namespace Conduit.Benchmarks
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //var summary = BenchmarkRunner.Run<SetVsListVsArray>();
            var summary = BenchmarkRunner.Run<ListPureSqlVsSqlPlusKv>();
        }
    }
}