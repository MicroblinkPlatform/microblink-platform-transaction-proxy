using System;

namespace Microblink.Platform.Proxy.Sample;

public class ApiClientCredentials
{
    public required Uri Address { get; set; }
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
}
