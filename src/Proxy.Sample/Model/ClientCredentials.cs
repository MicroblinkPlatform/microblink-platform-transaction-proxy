using System;
using Microsoft.Extensions.Options;

namespace Microblink.Platform.Proxy.Sample;

public class ApiClientCredentials : IValidateOptions<ApiClientCredentials>
{
    public required Uri Address { get; set; }
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }

    public ValidateOptionsResult Validate(string? name, ApiClientCredentials options)
    {
        if (ValidateUrl(options.Address.AbsoluteUri))
            return ValidateOptionsResult.Fail($"Provided {nameof(Address)} doesn't match any Microblink URL. Format should be https://api.*.microblink.com/agent/. More information can be found https://github.com/MicroblinkPlatform/microblink-platform-transaction-proxy?tab=readme-ov-file#provisioning");

        if(string.IsNullOrWhiteSpace(options.ClientId))
            return ValidateOptionsResult.Fail($"{nameof(ClientId)} is required. More information can be found https://github.com/MicroblinkPlatform/microblink-platform-transaction-proxy?tab=readme-ov-file#provisioning\"");

        if(string.IsNullOrWhiteSpace(options.ClientSecret))
            return ValidateOptionsResult.Fail($"{nameof(ClientSecret)} is required. More information can be found https://github.com/MicroblinkPlatform/microblink-platform-transaction-proxy?tab=readme-ov-file#provisioning\"");

        return ValidateOptionsResult.Success;
    }

    private static bool ValidateUrl(string url)
        => url.EndsWith("/api")
            || url.EndsWith("/api/")
            || url.EndsWith("/agent");
}
