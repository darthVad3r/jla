namespace Jla.Contracts;

public sealed record PlanProposal(
    IReadOnlyList<ProposedCriterion> SuccessCriteria,
    IReadOnlyList<OpenQuestion> OpenQuestions,
    IReadOnlyList<ProposedTask> Tasks,
    IReadOnlyList<CriterionMapping> RequesterMappings,   // exactly one per requester criterion
    IReadOnlyList<string> MissingCapabilities,      // the LLM's claim, verified against the registry
    string Rationale, string? RevisionReason);

public sealed record ProposedTask(
    string TaskKey, string Title, string CapabilityId,
    IReadOnlyList<InputBinding> Inputs, IReadOnlyList<string> After,
    IReadOnlyList<string> RequestedOperations,
    IReadOnlyList<AcceptanceCriterion> Acceptance,
    IReadOnlyList<string> ProducesCriteria,
    string? PreferredAgentId, string Rationale);

// Not defined in the design doc's contracts. Origin is derived by PlanValidator from RequesterCriterionIds, never set by the LLM.
public sealed record ProposedCriterion(
    string Id, string Description, AcceptanceCriterion Check,
    IReadOnlyList<string> RequesterCriterionIds);   // empty means a derived criterion
