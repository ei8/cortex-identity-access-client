using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ei8.Cortex.IdentityAccess.Common;

namespace ei8.Cortex.IdentityAccess.Client.Out
{
    public interface IAuthorClient
    {
        Task<AuthorInfo> GetAuthor(string outBaseUrl, string userId, CancellationToken token = default);
    }
}
