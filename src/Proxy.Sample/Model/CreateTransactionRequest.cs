using System.Collections.Generic;

namespace Microblink.Platform.Proxy.Sample;

public class CreateTransactionRequest
{
    public required string WorkflowId { get; set; }
    public required string Platform { get; set; }
    public required string SdkVersion { get; set; }
    public Dictionary<string, object>? FormValues { get; set; }
    public Dictionary<string, object>? PropertyBag { get; set; }
}
