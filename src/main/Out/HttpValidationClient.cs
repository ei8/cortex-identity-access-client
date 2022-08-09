using NLog;
using neurUL.Common.Http;
using Polly;
using Splat;
using System;
using System.Threading;
using System.Threading.Tasks;
using ei8.Cortex.IdentityAccess.Common;
using System.Collections.Generic;
using System.Linq;
using Polly.Retry;

namespace ei8.Cortex.IdentityAccess.Client.Out
{
    public class HttpValidationClient : IValidationClient
    {
        private readonly IRequestProvider requestProvider;

        private static AsyncRetryPolicy exponentialRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                (ex, _) => HttpValidationClient.logger.Error(ex, "Error occurred while communicating with ei8 Cortex Identity Access. " + ex.InnerException?.Message)
            );
        private static readonly string ValidationsPathTemplate = "identityaccess/validations";
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public HttpValidationClient(IRequestProvider requestProvider = null)
        {
            this.requestProvider = requestProvider ?? Locator.Current.GetService<IRequestProvider>();
        }

        public async Task<ActionValidationResult> CreateNeuron(string outBaseUrl, Guid neuronId, Guid? regionId, string userId, CancellationToken token = default) =>
           await HttpValidationClient.exponentialRetryPolicy.ExecuteAsync(
               async () => await this.CreateNeuronInternal(outBaseUrl, neuronId, regionId, userId, token).ConfigureAwait(false));

        private async Task<ActionValidationResult> CreateNeuronInternal(string outBaseUrl, Guid neuronId, Guid? regionId, string userId, CancellationToken token = default)
        {
            var data = new
            {
                NeuronId = neuronId,
                RegionId = regionId,
                UserId = userId
            };

            return await this.requestProvider.PostAsync<ActionValidationResult>(
               $"{outBaseUrl}{HttpValidationClient.ValidationsPathTemplate}/createneuron",
               data,
               token: token
               );
        }

        public async Task<ActionValidationResult> UpdateNeuron(string outBaseUrl, Guid neuronId, string userId, CancellationToken token = default) =>
           await HttpValidationClient.exponentialRetryPolicy.ExecuteAsync(
               async () => await this.UpdateNeuronInternal(outBaseUrl, neuronId, userId, token).ConfigureAwait(false));
        private async Task<ActionValidationResult> UpdateNeuronInternal(string outBaseUrl, Guid neuronId, string userId, CancellationToken token = default)
        {
            var data = new
            {
                NeuronId = neuronId,
                UserId = userId
            };

            return await this.requestProvider.PostAsync<ActionValidationResult>(
               $"{outBaseUrl}{HttpValidationClient.ValidationsPathTemplate}/updateneuron",
               data,
               token: token
               );
        }

        public async Task<ActionValidationResult> ReadNeurons(string outBaseUrl, IEnumerable<Guid> neuronIds, string userId, CancellationToken token = default) =>
           await HttpValidationClient.exponentialRetryPolicy.ExecuteAsync(
               async () => await this.ReadNeuronsInternal(outBaseUrl, neuronIds, userId, token).ConfigureAwait(false));

        private async Task<ActionValidationResult> ReadNeuronsInternal(string outBaseUrl, IEnumerable<Guid> neuronIds, string userId, CancellationToken token = default)
        {
            var data = new
            {
                NeuronIds = neuronIds.ToArray(),
                UserId = userId
            };

            return await this.requestProvider.PostAsync<ActionValidationResult>(
               $"{outBaseUrl}{HttpValidationClient.ValidationsPathTemplate}/readneurons",
               data,
               token: token
               );
        }
    }
}
