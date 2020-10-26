using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Runtime.CompilerServices;

namespace AwaitableExercises.Core
{
    public static class BoolExtensions
    {
        public static BoolAwaiter GetAwaiter(this bool value)
            => new BoolAwaiter(value);
    }

    public class BoolAwaiter : INotifyCompletion
    {
        readonly bool value;

        public BoolAwaiter(bool value)
            => this.value = value;

        public bool IsCompleted 
            => true;

        public void OnCompleted(Action continuation)
        { }

        public bool GetResult() 
            => value;
    }
}
