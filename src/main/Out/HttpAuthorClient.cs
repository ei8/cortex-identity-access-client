using ei8.Cortex.IdentityAccess.Common;
using neurUL.Common.Domain.Model;
using neurUL.Common.Http;
using NLog;
using Polly;
using Polly.Retry;
using Splat;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.IdentityAccess.Client.Out
{
    public class HttpAuthorClient : IAuthorClient
    {
        private readonly IRequestProvider requestProvider;

        private static AsyncRetryPolicy exponentialRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                (ex, _) => HttpAuthorClient.logger.Error(ex, "Error occurred while communicating with ei8 Cortex Identity Access. " + ex.InnerException?.Message)
            );
        private static readonly string AuthorsPathTemplate = "identityaccess/authors";
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public HttpAuthorClient(IRequestProvider requestProvider = null)
        {
            var rp = requestProvider ?? Locator.Current.GetService<IRequestProvider>();
            AssertionConcern.AssertArgumentNotNull(rp, nameof(requestProvider));

            this.requestProvider = rp;
        }

        public async Task<AuthorInfo> GetAuthor(string outBaseUrl, string userId, CancellationToken token = default) =>
            await HttpAuthorClient.exponentialRetryPolicy.ExecuteAsync(
                    async () => await this.GetAuthorInternal(outBaseUrl, userId, token).ConfigureAwait(false));

        private async Task<AuthorInfo> GetAuthorInternal(string outBaseUrl, string userId, CancellationToken token = default(CancellationToken))
        {
            return await requestProvider.GetAsync<AuthorInfo>(
                           $"{outBaseUrl}{HttpAuthorClient.AuthorsPathTemplate}?userid={userId}",
                           token: token
                           );
        }
    }
}
