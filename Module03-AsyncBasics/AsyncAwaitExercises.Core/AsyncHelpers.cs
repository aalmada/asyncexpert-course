using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncAwaitExercises.Core
{
    public class AsyncHelpers
    {
        public static Task<string> GetStringWithRetries(HttpClient client, string url, int maxTries = 3, CancellationToken token = default)
        {
            // Create a method that will try to get a response from a given `url`, retrying `maxTries` number of times.
            // It should wait one second before the second try, and double the wait time before every successive retry
            // (so pauses before retries will be 1, 2, 4, 8, ... seconds).
            // * `maxTries` must be at least 2
            // * we retry if:
            //    * we get non-successful status code (outside of 200-299 range), or
            //    * HTTP call thrown an exception (like network connectivity or DNS issue)
            // * token should be able to cancel both HTTP call and the retry delay
            // * if all retries fails, the method should throw the exception of the last try
            // HINTS:
            // * `HttpClient.GetStringAsync` does not accept cancellation token (use `GetAsync` instead)
            // * you may use `EnsureSuccessStatusCode()` method

            static async Task<string> GetStringWithRetriesInternal(HttpClient client, string url, int maxTries, CancellationToken token)
            {
                var delay = 1000;
                for (var attempt = 1; true; attempt++)
                {
                    try
                    {
                        var response = await client.GetAsync(url, token);
                        _ = response.EnsureSuccessStatusCode();
                        return await response.Content.ReadAsStringAsync();
                    }
                    catch (HttpRequestException)
                    when (attempt == maxTries)
                    {
                        throw;
                    }
                    catch (HttpRequestException)
                    {
                        // do nothing
                    }

                    await Task.Delay(delay, token);
                    delay *= 2;
                }
            }

            if (maxTries < 2)
                return Task.FromException<string>(new ArgumentException("Must be at least 2", nameof(maxTries)));

            if (token.IsCancellationRequested)
                return Task.FromCanceled<string>(token);

            return GetStringWithRetriesInternal(client, url, maxTries, token);
        }

    }
}
