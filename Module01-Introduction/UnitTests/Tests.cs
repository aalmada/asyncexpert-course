using Dotnetos.AsyncExpert.Homework.Module01.Benchmark;
using System;
using Xunit;

namespace Dotnetos.AsyncExpert.Homework.Module01.UnitTests
{
    public class Tests
    {
        public static TheoryData<ulong, ulong> FibonacciData =>
            new TheoryData<ulong, ulong>
            {
                { 1, 1 },
                { 2, 1 },
                { 3, 2 },
                { 4, 3 },
                { 5, 5 },
                { 6, 8 },
                { 7, 13 },
                { 8, 21 },
                { 9, 34 },
            };

        [Theory]
        [MemberData(nameof(FibonacciData))]
        public void Recursive(ulong value, ulong expected)
        {
            var fibonnaci = new FibonacciCalc();
            var result = fibonnaci.Recursive(value);
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(FibonacciData))]
        public void RecursiveWithMemoization(ulong value, ulong expected)
        {
            var fibonnaci = new FibonacciCalc();
            var result = fibonnaci.RecursiveWithMemoization(value);
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(FibonacciData))]
        public void RecursiveWithMemoizationPool(int value, ulong expected)
        {
            var fibonnaci = new FibonacciCalc();
            var result = fibonnaci.RecursiveWithMemoizationPool(value);
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(FibonacciData))]
        public void Iterative(ulong value, ulong expected)
        {
            var fibonnaci = new FibonacciCalc();
            var result = fibonnaci.Iterative(value);
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(FibonacciData))]
        public void TailRecursive(ulong value, ulong expected)
        {
            var fibonnaci = new FibonacciCalc();
            var result = fibonnaci.TailRecursive(value);
            Assert.Equal(expected, result);
        }
    }
}
