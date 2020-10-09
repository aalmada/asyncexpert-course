using System;
using System.Buffers;
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
            if (n == 1 || n == 2)
                return 1;

            fixed (ulong* lookup = new ulong[n + 1])
            {
                lookup[1] = 1;
                lookup[2] = 1;
                return RecursiveWithMemoization(n, lookup);
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public unsafe ulong RecursiveWithMemoizationPool(int n)
        {
            if (n == 1 || n == 2)
                return 1;

            var pool = ArrayPool<ulong>.Shared;
            var lookup = pool.Rent(n + 1);
            try
            {
                Array.Clear(lookup, 3, n - 2);
                fixed (ulong* lookupPtr = lookup)
                {
                    lookupPtr[1] = 1;
                    lookupPtr[2] = 1;
                    return RecursiveWithMemoization((ulong)n, lookupPtr);
                }
            }
            finally
            {
                pool.Return(lookup);
            }
        }

        static unsafe ulong RecursiveWithMemoization(ulong n, ulong* lookup)
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

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public ulong Iterative(ulong n)
        {
            var (x, y, z) = (1ul, 1ul, 1ul);
            for (var i = 2ul; i < n; ++i)
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
