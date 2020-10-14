using System;
using System.Threading;

namespace ThreadPoolExercises.Core
{
    public class ThreadingHelpers
    {
        public static void ExecuteOnThread(Action action, int repeats, CancellationToken token = default, Action<Exception>? errorAction = null)
        {
            // * Create a thread and execute there `action` given number of `repeats` - waiting for the execution!
            //   HINT: you may use `Join` to wait until created Thread finishes
            // * In a loop, check whether `token` is not cancelled
            // * If an `action` throws and exception (or token has been cancelled) - `errorAction` should be invoked (if provided)

            var thread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    for (var counter = repeats; counter != 0; --counter)
                    {
                        token.ThrowIfCancellationRequested();
                        action();
                    }
                }
                catch (Exception ex)
                {
                    if (errorAction is object)
                        errorAction(ex);
                }
            }));

            thread.Start();
            thread.Join();
        }

        public static void ExecuteOnThreadPool(Action action, int repeats, CancellationToken token = default, Action<Exception>? errorAction = null)
        {
            // * Queue work item to a thread pool that executes `action` given number of `repeats` - waiting for the execution!
            //   HINT: you may use `AutoResetEvent` to wait until the queued work item finishes
            // * In a loop, check whether `token` is not cancelled
            // * If an `action` throws and exception (or token has been cancelled) - `errorAction` should be invoked (if provided)

            var autoResetEvent = new AutoResetEvent(false);

            _ = ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    for (var counter = repeats; counter != 0; --counter)
                    {
                        token.ThrowIfCancellationRequested();
                        action();
                    }
                }
                catch(Exception ex)
                {
                    if (errorAction is object)
                        errorAction(ex);
                }
                finally
                {
                    _ = autoResetEvent.Set();
                }
            });

            _ = autoResetEvent.WaitOne();
        }
    }
}
