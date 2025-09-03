using Microblink.Proxy.Sample.Model;

namespace Microblink.Platform.Proxy.Sample;

public record class WorkflowInfoModel
{
    public required int StepCount { get; set; }
    public required int InteractiveStepCount { get; set; }
    public required bool HasConditionalInteractiveStep { get; set; }
    public required string[] InteractiveSteps { get; set; }
    public required string[] CompletedInteractiveSteps { get; set; }
    public required string CurrentStep { get; set; }
    public required int CurrentStepRetryCount { get; set; }
    public required int CurrentStepExecutionIndex { get; set; }

    public required WorkflowStepInfoModel[] Steps { get; set; }
    public required int CurrentStepId { get; set; }
    public required int[] PendingStepIds { get; set; }
    public required int[] CompletedStepIds { get; set; }
}
