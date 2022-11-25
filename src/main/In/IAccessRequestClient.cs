using System;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.IdentityAccess.Client.In
{
    public interface IAccessRequestClient
    {
        Task CreateAccessRequestAsync(string baseUrl, Guid neuronId, string userId, CancellationToken token = default);
    }
}
