namespace Jla.Contracts;

public enum DecisionKind { Permit, Deny, RequireApproval }

public sealed record GovernanceDecision(
    DecisionKind Kind, string ReasonCode, ApprovalClass EffectiveClass,
    int PinnedCatalogVersion, int CurrentCatalogVersion, ProposedAction? Action);

public interface IAgent { Task<TaskResult> ExecuteAsync(AgentTaskContext ctx, CancellationToken ct); }

public sealed record AgentTaskContext(
    Guid TaskId, string CapabilityId, JsonElement Input,
    IOperationInvoker Operations, IReasoning Reasoning);

public interface IOperationInvoker {
    Task<OperationResponse> InvokeAsync(string operationId, JsonElement args, CancellationToken ct); }

public interface IReasoning {      // task-scoped LLM access; routed through LlmGateway
    Task<JsonElement> CompleteAsync(string instructions, JsonElement untrustedData,
                                    JsonElement outputSchema, CancellationToken ct); }

public sealed record OperationResponse(bool Ok, JsonElement Data, string? ErrorCode);

// Implemented by Jla.Simulation. Called only by the tool broker, which supplies the tenant.
public interface IOperationHandler {
    string OperationId { get; }
    HandlerTraits Traits { get; }      // does it dedupe on the idempotency key? can it look up an outcome by key?
    Task<OperationOutcome> ExecuteAsync(OperationCall call, CancellationToken ct);
    Task<OutcomeLookup> LookupAsync(TenantId tenant, string idempotencyKey, CancellationToken ct); }

public sealed record OperationCall(TenantId TenantId, JsonElement Arguments, string? IdempotencyKey);
public sealed record HandlerTraits(bool SupportsIdempotencyKey, bool SupportsOutcomeLookup);

// Not defined in the design doc's contracts. Shapes follow the execute and ambiguous-outcome steps of section 5.
public enum OperationOutcomeKind { Succeeded, Failed }
public sealed record OperationOutcome(OperationOutcomeKind Kind, JsonElement Data, string? ErrorCode);

public enum LookupResult { Executed, NotExecuted, Indeterminate }
public sealed record OutcomeLookup(LookupResult Result, JsonElement? StoredOutcome);   // StoredOutcome set when Executed
