namespace Microblink.Proxy.Sample.Model;

public record class WorkflowStepInfoModel
{
    public required int Id { get; set; }
    public required string Type { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}
