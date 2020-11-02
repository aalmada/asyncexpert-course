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
            // this helper method removes the need to materialize the collection, requiring just one enumerator allocation
            static IEnumerable<Task<HttpResponseMessage>> GetTasks(HttpClient httpClient, string[] urls, CancellationToken token, Task<HttpResponseMessage> timeoutTask)
            {
                for (var index = 0; index < urls.Length; index++)
                    yield return httpClient.GetAsync(urls[index], token);

                yield return timeoutTask; // should this be the last task to start?
            }

            // allow cancellation from external and internal tokens
            using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);

            // a timeout Task<HttpResponseMessage> so that it can be concatenated with other tasks
            var timeoutTask = Task.Run(async () =>
            {
                await Task.Delay(millisecondsTimeout, cancellationTokenSource.Token).ConfigureAwait(false);
                return default(HttpResponseMessage);
            }, cancellationTokenSource.Token);

            // get the first task to complete
            var tasks = GetTasks(httpClient, urls, cancellationTokenSource.Token, timeoutTask);
            var completedTask = await Task.WhenAny(tasks).ConfigureAwait(false);

            // cancel all other tasks
            cancellationTokenSource.Cancel(); 

            // check if it was explicitly cancelled or timeoutTask was the first to complete
            if (token.IsCancellationRequested || completedTask == timeoutTask)
                throw new TaskCanceledException();

            // get the result string
            var responseMessage = await completedTask.ConfigureAwait(false);
            return await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}
