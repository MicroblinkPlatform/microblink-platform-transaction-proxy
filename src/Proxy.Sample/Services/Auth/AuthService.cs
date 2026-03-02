using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microblink.Platform.Proxy.Sample;

public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<AuthService> _logger;
    private readonly ApiClientCredentials _options;
    private readonly string _authority;

    const string IdHttpClientName = "TokenManagerClient:Idp";
    const string Audience = "idv-api";
    const string UsAuthority = "https://account.platform.microblink.com"; 
    const string EuAuthority = "https://account.platform.eu.microblink.com";

    private static readonly ConcurrentDictionary<string, IDiscoveryCache> _discoveryCaches = [];


    public AuthService(
        ILogger<AuthService> logger,
        IOptions<ApiClientCredentials> options,
        IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _options = options.Value;
        _clientFactory = clientFactory;

        _authority = (_options.Address!.Host?.Contains("api.eu.platform.microblink.com", StringComparison.OrdinalIgnoreCase) ?? false)
           ? EuAuthority
           : UsAuthority;
    }

    public Uri Address => _options.Address;

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
            if (!_discoveryCaches.TryGetValue(_authority, out var cache))
            {
                cache = new DiscoveryCache(_authority, _clientFactory.CreateClient);
                _discoveryCaches.TryAdd(_authority, cache);
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
