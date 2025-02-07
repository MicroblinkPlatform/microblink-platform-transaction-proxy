using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microblink.Platform.Proxy.Sample;

public interface IAuthService
{
    Task<SecurityToken> GetAccessToken(CancellationToken ct);
    Uri Address { get; }
}
