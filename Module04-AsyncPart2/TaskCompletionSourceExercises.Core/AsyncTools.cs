using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TaskCompletionSourceExercises.Core
{
    public class AsyncTools
    {
        public static Task<string> RunProgramAsync(string path, string args = "")
        {
            var taskCompletionSource = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                },
                EnableRaisingEvents = true,
            };
            process.Exited += async (_, _) =>
            {
                if (process.ExitCode == 0)
                    _ = taskCompletionSource.TrySetResult(await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false));
                else
                    _ = taskCompletionSource.TrySetException(new Exception(await process.StandardError.ReadToEndAsync().ConfigureAwait(false)));

                process.Dispose();
            };
            _ = process.Start();
            return taskCompletionSource.Task;
        }
    }
}
