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

        public async Task CreateAccessRequestAsync(string outBaseUrl, Guid neuronId, string userNeuronId, CancellationToken token = default) =>
            await HttpAccessRequestClient.ExponentialRetryPolicy.ExecuteAsync(
               async () => await this.CreateAccessRequestInternalAsync(outBaseUrl, neuronId, userNeuronId, token).ConfigureAwait(false));
        public async Task CreateAccessRequestInternalAsync(string baseUrl, Guid neuronId, string userNeuronId, CancellationToken token = default)
        {
            await this.requestProvider.PostAsync(
               $"{baseUrl}accessRequest/neuron/{neuronId}",
               data: new { UserNeuronId = userNeuronId }
               );
        }
    }
}
