using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microblink.Platform.Proxy.Sample;

public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<AuthService> _logger;
    private readonly ApiClientCredentials _options;

    const string IdHttpClientName = "TokenManagerClient:Idp";
    const string Audience = "idv-api";
    const string Authority = "https://account.platform.microblink.com";

    private static readonly ConcurrentDictionary<string, IDiscoveryCache> _discoveryCaches = [];


    public AuthService(
        ILogger<AuthService> logger,
        IOptions<ApiClientCredentials> options,
        IHttpClientFactory clientFactory,
        IDistributedCache? tokenCache
        )
    {
        _logger = logger;
        _options = options.Value;
        _clientFactory = clientFactory;
    }

    public Uri Address => _options.Address;
    public string ProxyAuthority => Authority;


    public async Task<SecurityToken> GetAccessToken(CancellationToken ct)
    {
        var token = await GetClientCredentialToken(ct);

        if (token == null || token.AccessToken == null)
        {
            var exceptionMessage = "Failed to obtain access token. Verify service account configuration!";
            _logger.LogError(exceptionMessage);
            throw new Exception(exceptionMessage);
        }
            

        return token;
    }

    private async Task<SecurityToken?> GetClientCredentialToken(CancellationToken ct)
    {
        var request = new TokenRequest
        {
            GrantType = OidcConstants.GrantTypes.ClientCredentials,
            ClientId = _options.ClientId,
            ClientSecret = _options.ClientSecret
        };

        request.Parameters.Add(OidcConstants.TokenRequest.Audience, Audience);

        return await GetToken(request, ct);
    }


    private async Task<SecurityToken?> GetToken(TokenRequest request, CancellationToken ct)
    {
        SecurityToken? token = null;

        var disco = await GetDiscoveryDocument();

        using var client = _clientFactory.CreateClient(IdHttpClientName);

        request.Address = disco.TokenEndpoint;

        var tokenResponse = await client.RequestTokenAsync(request, ct);

        if (tokenResponse != null)
        {
            if (tokenResponse.IsError)
                _logger.LogError("Failed to resolve access token: {ErrorMessage}.", tokenResponse.Error);
            else
                token = new SecurityToken
                {
                    AccessToken = tokenResponse.AccessToken,
                    IssuedOn = DateTime.UtcNow,
                    ExpiresIn = tokenResponse.ExpiresIn
                };
        }
        else
        {
            _logger.LogError("Failed to resolve access token. Received an empty response.");
        }

        return token;
    }

    private IDiscoveryCache DiscoveryCache
    {
        get
        {
            if (!_discoveryCaches.TryGetValue(Authority, out var cache))
            {
                cache = new DiscoveryCache(ProxyAuthority, _clientFactory.CreateClient);
                _discoveryCaches.TryAdd(Authority, cache);
            }
            return cache;
        }
    }

    private async Task<DiscoveryDocumentResponse> GetDiscoveryDocument()
    {
        var disco = await DiscoveryCache.GetAsync();
        if (disco.IsError)
        {
            var ex = new Exception("Failed to get discovery document: " + disco.Error);
            _logger.LogError(ex, "Failed to get discovery document.");
            throw ex;
        }
        return disco;
    }
}
