using System;

namespace Microblink.Platform.Proxy.Sample;

public class SecurityToken
{
    public string? AccessToken { get; set; }
    public int ExpiresIn { get; set; }
    public DateTime IssuedOn { get; set; }
}
