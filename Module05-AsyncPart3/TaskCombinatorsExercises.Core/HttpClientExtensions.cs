using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TaskCombinatorsExercises.Core
{
    public static class HttpClientExtensions
    {
        /*
         Write cancellable async method with timeout handling, that concurrently tries to get data from
         provided urls (first wins and its response is returned, rest is __cancelled__).
         
         Tips:
         * consider using HttpClient.GetAsync (as it is cancellable)
         * consider using Task.WhenAny
         * you may use urls like for testing https://postman-echo.com/delay/3
         * you should have problem with tasks cancellation -
            - how to merge tokens of operations (timeouts) with the provided token? 
            - Tip: you can link tokens with the help of CancellationTokenSource.CreateLinkedTokenSource(token)
         */
        public static async Task<string> ConcurrentDownloadAsync(this HttpClient httpClient,
            string[] urls, int millisecondsTimeout, CancellationToken token)
        {
            // allow cancellation from external and time out tokens
            using var timeOutTokenSource = new CancellationTokenSource(millisecondsTimeout);
            using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, timeOutTokenSource.Token);

            // get the first task to complete
            var tasks = urls.Select(url => httpClient.GetAsync(url, cancellationTokenSource.Token));
            var completedTask = await Task.WhenAny(tasks).ConfigureAwait(false);

            // cancel all other tasks
            cancellationTokenSource.Cancel();

            // get the response message
            // throws TaskCanceledException if task was cancelled
            var responseMessage = await completedTask.ConfigureAwait(false);

            // get the content string
            _ = responseMessage.EnsureSuccessStatusCode();
            return await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}
