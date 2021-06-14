using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ei8.Cortex.IdentityAccess.Common;

namespace ei8.Cortex.IdentityAccess.Client.Out
{
    public interface IValidationClient
    {
        Task<ActionValidationResult> CreateNeuron(string outBaseUrl, Guid neuronId, Guid? regionId, string userId, CancellationToken token = default);

        Task<ActionValidationResult> UpdateNeuron(string outBaseUrl, Guid neuronId, string userId, CancellationToken token = default);

        Task<ActionValidationResult> ReadNeurons(string outBaseUrl, IEnumerable<Guid> neuronIds, string userId, CancellationToken token = default);
    }
}
