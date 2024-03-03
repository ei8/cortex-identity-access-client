using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.IdentityAccess.Client.In
{
    public interface IPermitClient
    {
        Task CreateNeuronPermitAsync(string baseUrl, Guid neuronId, Guid userNeuronId, DateTime? expirationDate = null, CancellationToken token = default);

        Task<IEnumerable<Guid>> GetNeuronIdsByUserNeuronIdsAsync(string baseUrl, IEnumerable<Guid> userNeuronIds, IEnumerable<Guid> filterNeuronIds = null, CancellationToken token = default);
    }
}
