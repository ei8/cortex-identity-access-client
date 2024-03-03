using Microsoft.AspNetCore.Http.Extensions;
using neurUL.Common.Domain.Model;
using neurUL.Common.Http;
using NLog;
using Polly;
using Polly.Retry;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.IdentityAccess.Client.In
{
    public class HttpPermitClient : IPermitClient
    {
        private readonly IRequestProvider requestProvider;
        private static readonly string PermitsPathTemplate = "identityaccess/permits";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static AsyncRetryPolicy ExponentialRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                (ex, _) => HttpPermitClient.Logger.Error(ex, "Error occurred while communicating with ei8 Cortex Identity Access. " + ex.InnerException?.Message)
            );

        public HttpPermitClient(IRequestProvider requestProvider = null)
        {
            this.requestProvider = requestProvider ?? Locator.Current.GetService<IRequestProvider>();
        }

        public async Task CreateNeuronPermitAsync(string outBaseUrl, Guid neuronId, Guid userNeuronId, DateTime? expirationDate = null, CancellationToken token = default) =>
            await HttpPermitClient.ExponentialRetryPolicy.ExecuteAsync(
               async () => await this.CreateNeuronPermitInternalAsync(outBaseUrl, neuronId, userNeuronId, expirationDate, token).ConfigureAwait(false));

        private async Task CreateNeuronPermitInternalAsync(string baseUrl, Guid neuronId, Guid userNeuronId, DateTime? expirationDate = null, CancellationToken token = default)
        {
            await this.requestProvider.PostAsync(
               $"{baseUrl}{HttpPermitClient.PermitsPathTemplate}/neurons",
               data: new { 
                   UserNeuronId = userNeuronId.ToString(),
                   NeuronId = neuronId.ToString(),
                   ExpirationDate = expirationDate
               }
               );
        }

        public async Task<IEnumerable<Guid>> GetNeuronIdsByUserNeuronIdsAsync(string baseUrl, IEnumerable<Guid> userNeuronIds, IEnumerable<Guid> filterNeuronIds = null, CancellationToken token = default) =>
            await HttpPermitClient.ExponentialRetryPolicy.ExecuteAsync(
                   async () => await this.GetNeuronIdsByUserNeuronIdsInternalAsync(baseUrl, userNeuronIds, filterNeuronIds, token).ConfigureAwait(false));

        private async Task<IEnumerable<Guid>> GetNeuronIdsByUserNeuronIdsInternalAsync(string baseUrl, IEnumerable<Guid> userNeuronIds, IEnumerable<Guid> filterNeuronIds = null, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentNotNull(userNeuronIds, nameof(userNeuronIds));
            AssertionConcern.AssertArgumentValid(un => un.Any(), userNeuronIds, "'userNeuronIds' must not be empty.", nameof(userNeuronIds));

            var qb = new QueryBuilder();

            qb.Add("userneuronid", userNeuronIds.Select(uni => uni.ToString()));

            if (filterNeuronIds != null && filterNeuronIds.Any())
                qb.Add("neuronid", filterNeuronIds.Select(fni => fni.ToString()));

            var queryString = qb.Any() ? "?" + qb.ToString() : string.Empty;

            return await this.requestProvider.GetAsync<IEnumerable<Guid>>(
               $"{baseUrl}{HttpPermitClient.PermitsPathTemplate}/neuronids{queryString}", 
               token: token
               );
        }

    }
}
