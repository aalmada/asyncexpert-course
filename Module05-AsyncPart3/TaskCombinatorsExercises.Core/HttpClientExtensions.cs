using System;
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
            using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);

            var timeoutTask = Task.Run(async () =>
            {
                await Task.Delay(millisecondsTimeout, cancellationTokenSource.Token).ConfigureAwait(false);
                cancellationTokenSource.Cancel();
                return default(HttpResponseMessage);
            }, cancellationTokenSource.Token);

            var tasks = urls.Select(url => httpClient.GetAsync(url, cancellationTokenSource.Token));

            var completedTask = await Task.WhenAny(EnumerableEx.Return(timeoutTask).Concat(tasks)).ConfigureAwait(false);
            if (completedTask == timeoutTask)
                throw new TaskCanceledException();

            var responseMessage = await completedTask.ConfigureAwait(false);
            return await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}
