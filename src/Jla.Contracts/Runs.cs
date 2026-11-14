namespace Jla.Contracts;

public enum RunState { Active, AwaitingHuman, Completed, Escalated, Cancelled }
public enum TaskState { Pending, Ready, Running, AwaitingApproval, Completed, Failed, Cancelled, Superseded }
public enum AcceptanceStatus { NotEvaluated, Accepted, Incomplete, Rejected }
public enum ReportedStatus { Succeeded, Failed, Partial, Blocked }

public sealed record ExecutionLimits(                 // defaults are the charter's proposed values
    int MaxTasksPerPlan = 15, int MaxReplans = 3, int MaxRetriesPerTask = 2,
    int MaxTaskSeconds = 120, int MaxObjectiveMinutes = 30, int MaxLlmCalls = 50,
    decimal MaxCostUsd = 2.00m,                       // reservation-enforced budget with bounded exposure (section 5)
    decimal ReservationMargin = 0.20m, int HumanWaitMinutes = 60, int ApprovalExpiryMinutes = 15);

public sealed record Run(
    RunId Id, TenantId TenantId, RunState State, int StateVersion,
    int CurrentPlanVersion, int ReplanCount, int CatalogVersion,
    ExecutionLimits Limits, RunUsage Usage,
    Guid? OpenHumanRequestId, TerminalOutcome? Terminal);

public sealed record TerminalOutcome(RunState State, string ReasonCode, string Summary, DateTimeOffset At);

public sealed record TaskRecord(
    Guid TaskId, RunId RunId, int IntroducedInPlanVersion, string TaskKey,
    TaskState State, AcceptanceStatus Acceptance, int Attempt, int RowVersion);

public sealed record TaskResult(
    Guid TaskId, int Attempt, ReportedStatus Status, JsonElement Output,
    IReadOnlyList<EvidenceRef> Evidence,
    IReadOnlyList<string> UnresolvedItems,            // machine-readable gaps
    IReadOnlyList<ResultIssue> Issues,                // uncertainty and errors
    IReadOnlyList<RecommendedStep> Recommendations,   // information only, never executed
    bool Retryable, ResourceUsage Usage);

public sealed record EvidenceRef(string Source, string ItemId);
public sealed record ResultIssue(string Code, string Message, bool IsUncertainty);
public sealed record RecommendedStep(string Description, string? SuggestedCapability);
public sealed record ResourceUsage(int LlmCalls, int InputTokens, int OutputTokens, decimal CostUsd, int ElapsedMs);

// Not defined in the design doc's contracts. Mirrors the runs columns spent_usd, llm_blocked, active_ms and the call count.
public sealed record RunUsage(decimal SpentUsd, int LlmCalls, int ActiveMs, bool LlmBlocked);
