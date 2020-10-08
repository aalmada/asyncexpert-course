using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace Dotnetos.AsyncExpert.Homework.Module01.Benchmark
{
    [DisassemblyDiagnoser(exportCombinedDisassemblyReport: true)]
    [MemoryDiagnoser]
    public class FibonacciCalc
    {
        // HOMEWORK:
        // 1. Write implementations for RecursiveWithMemoization and Iterative solutions
        // 2. Add MemoryDiagnoser to the benchmark
        // 3. Run with release configuration and compare results
        // 4. Open disassembler report and compare machine code
        // 
        // You can use the discussion panel to compare your results with other students

        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(Data))]
        public ulong Recursive(ulong n)
        {
            if (n == 1 || n == 2)
                return 1;

            return Recursive(n - 2) + Recursive(n - 1);
        }

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public unsafe ulong RecursiveWithMemoization(ulong n)
        {
            fixed (ulong* lookup = new ulong[n + 1])
            {
                lookup[1] = 1;
                lookup[2] = 1;
                return RecursiveWithMemoization(n, lookup);
            }

            static ulong RecursiveWithMemoization(ulong n, ulong* lookup)
            {
                var value = lookup[n];
                if (value == default)
                {
                    var penultimate = n - 1;
                    var antepenultimate = n - 2;

                    if (lookup[antepenultimate] == default)
                        lookup[antepenultimate] = RecursiveWithMemoization(antepenultimate - 2, lookup) + RecursiveWithMemoization(antepenultimate - 1, lookup);

                    if (lookup[penultimate] == default)
                        lookup[penultimate] = RecursiveWithMemoization(penultimate - 2, lookup) + RecursiveWithMemoization(penultimate - 1, lookup);

                    value = lookup[antepenultimate] + lookup[penultimate];
                    lookup[n] = value;
                }
                return value;
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public ulong Iterative(ulong n)
        {
            var (x, y, z) = (1UL, 1UL, 1UL);
            for (var i = 2UL; i < n; ++i)
            {
                x = y + z;
                z = y;
                y = x;
            }
            return x;
        }

        public IEnumerable<ulong> Data()
        {
            yield return 15;
            yield return 35;
        }
    }
}
