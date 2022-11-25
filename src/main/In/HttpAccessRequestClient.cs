using neurUL.Common.Http;
using NLog;
using Polly;
using Polly.Retry;
using Splat;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.IdentityAccess.Client.In
{
    public class HttpAccessRequestClient : IAccessRequestClient
    {
        private readonly IRequestProvider requestProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static AsyncRetryPolicy ExponentialRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                (ex, _) => HttpAccessRequestClient.Logger.Error(ex, "Error occurred while communicating with ei8 Cortex Identity Access. " + ex.InnerException?.Message)
            );

        public HttpAccessRequestClient(IRequestProvider requestProvider = null)
        {
            this.requestProvider = requestProvider ?? Locator.Current.GetService<IRequestProvider>();
        }

        public async Task CreateAccessRequestAsync(string baseUrl, Guid neuronId, string userId, CancellationToken token = default)
        {
            var requestUrl = $"{baseUrl}/accessRequest/neuron/{neuronId}";

            await HttpAccessRequestClient.ExponentialRetryPolicy
                    .ExecuteAsync(async () =>
                    {
                        await requestProvider.PostAsync(requestUrl, data: new { UserId = userId });
                    });
        }
    }
}
