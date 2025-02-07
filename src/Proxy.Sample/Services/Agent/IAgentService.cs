using System.Threading;
using System.Threading.Tasks;

namespace Microblink.Platform.Proxy.Sample;

public interface IAgentService
{
    Task<CreateTransactionResponse> StartTransaction(CreateTransactionRequest request, CancellationToken ct);
}
