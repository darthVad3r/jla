namespace Jla.Contracts;

public enum HumanRequestType { Clarification, Approval, Escalation }
public enum HumanRequestStatus { Open, Answered, Expired, Superseded, Delivered }

public sealed record HumanRequest(
    Guid Id, RunId RunId, HumanRequestType Type, HumanRequestStatus Status,
    int RunStateVersion,                              // fencing token
    string Reason, IReadOnlyList<string> WorkCompleted, IReadOnlyList<string> OutstandingIssues,
    string RequiredFromHuman, IReadOnlyList<string>? Options,
    ProposedAction? Action,                           // Approval only
    DateTimeOffset ExpiresAt);

public sealed record ProposedAction(
    string OperationId, IReadOnlyList<string> Targets,
    JsonElement MaterialParameters, string ActionDigest);   // SHA-256 over tenant, operation, targets, material parameters

public enum ResponseKind { Answer, Approve, Decline }
public sealed record HumanResponse(
    Guid RequestId, string PrincipalId, ResponseKind Kind, JsonElement? Answer, string? ActionDigest);

public enum ApprovalStatus { Granted, Consumed, Declined, Expired, Invalidated }

public sealed record Approval(
    Guid Id, Guid HumanRequestId, RunId RunId, string ActionDigest,
    string ApproverPrincipalId, string ApproverAuthority,   // the policy rule that authorized the approver
    DateTimeOffset GrantedAt, DateTimeOffset ExpiresAt,
    string IdempotencyKey, ApprovalStatus Status);

public enum AuditEventType {
    ObjectiveCreated, PlanProposed, PlanValidated, PlanRejected, PlanRevised, AgentSelected,
    TaskDispatched, TaskStateChanged, TaskResultRecorded, AcceptanceEvaluated, ReplanDecision,
    GovernanceDecision, OperationExecuted, ApprovalRequested, ApprovalDecided,
    ClarificationRequested, HumanResponseRejected, CatalogTightened,
    LlmCallReserved, LlmCallSettled, LimitReached, RunTerminated }

public sealed record AuditEvent(
    long Seq, Guid EventId, TenantId TenantId, RunId? RunId, DateTimeOffset At,
    AuditEventType Type, string Actor, string? Subject, JsonElement Details, int? CatalogVersion);
