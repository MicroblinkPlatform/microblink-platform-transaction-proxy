using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microblink.Platform.Proxy.Sample;

public interface IAgentService
{
    Task<HttpResponseMessage> ProcessRequest(string url, HttpRequest request, CancellationToken ct);
}
