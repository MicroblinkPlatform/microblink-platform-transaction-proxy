using System;

namespace Microblink.Proxy.Sample.Model;

public class CreateTransactionConsentModel
{
    public string? UserId { get; set; }
    public string? Note { get; set; }
    public DateTime? GivenOn { get; set; }
    public bool? IsProcessingStoringAllowed { get; set; }
    public bool? IsTrainingAllowed { get; set; }
}
