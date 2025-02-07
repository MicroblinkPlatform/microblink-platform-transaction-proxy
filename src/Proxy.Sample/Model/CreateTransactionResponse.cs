using System;

namespace Microblink.Platform.Proxy.Sample;

public class CreateTransactionResponse
{
    public required string TransactionId { get; set; }
    public required string WorkflowId { get; set; }
    public required string WorkflowVersionId { get; set; }
    public required string OrganizationId { get; set; }
    public required string EphemeralKey { get; set; }
    public required string Authorization { get; set; }
    public required WorkflowInfoModel WorkflowInfo { get; set; }
    public required DateTime CreatedOn { get; set; }
    public required DateTime ModifiedOn { get; set; }

    public required string ProcessingStatus { get; set; }
    public string? VerificationStatus { get; set; }

    public string[]? Warnings { get; set; }

    public string? EdgeApiUrl { get; set; }
}
