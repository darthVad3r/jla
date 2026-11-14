namespace Jla.Contracts;

public readonly record struct TenantId(Guid Value);
public readonly record struct RunId(Guid Value);

// Built only by the host's authentication layer. Never read from a request body or LLM output.
public sealed record RequesterContext(TenantId TenantId, string PrincipalId, IReadOnlySet<string> Roles);

public sealed record Objective(
    Guid ObjectiveId, RequesterContext Requester,
    string Text,                                    // authorized intent within the requester's tenant, role, and permissions
    ObjectiveConstraints Constraints,
    IReadOnlyList<RequesterCriterion> SuccessCriteria,   // stable IDs assigned by RunFactory
    IReadOnlyList<EmbeddedContent> EmbeddedContent,      // documents, quoted messages, records supplied alongside: untrusted data
    string LimitsProfile);

// Requester-supplied. Can only tighten platform limits and policy, never loosen or extend them.
public sealed record ObjectiveConstraints(
    decimal? BudgetUsd, DateTimeOffset? Deadline, IReadOnlyList<string> ProhibitedOperations);

// Validated interpretation. Built from an LLM proposal, never trusted as-is.
public sealed record Interpretation(
    IReadOnlyList<SuccessCriterion> SuccessCriteria,
    IReadOnlyList<OpenQuestion> OpenQuestions,
    IReadOnlyList<CriterionMapping> RequesterMappings);   // exactly one per requester criterion

public sealed record SuccessCriterion(
    string Id, string Description,
    AcceptanceCriterion Check,                      // evaluated over accepted task outputs
    CriterionOrigin Origin,                         // Requester or Derived
    IReadOnlyList<string> RequesterCriterionIds,    // non-empty when Origin is Requester
    IReadOnlyList<string> ProducedByTasks);         // validator requires at least one

public sealed record OpenQuestion(string Id, string Field, string Question, IReadOnlyList<string>? Options);

public sealed record RequesterCriterion(string Id, string Text);   // the LLM cannot create, edit, or drop one
public sealed record EmbeddedContent(string Label, string Content);

public enum CriterionOrigin { Requester, Derived }
public enum MappingStatus { Mapped, NeedsClarification }

public sealed record CriterionMapping(
    string RequesterCriterionId, MappingStatus Status,
    IReadOnlyList<string> SuccessCriterionIds,      // Mapped: one or more Origin = Requester criteria
    string? OpenQuestionId);                        // NeedsClarification: the question that resolves it

public enum CriterionVerdictKind { Met, NotMet, CannotEvaluate }

// Produced by the platform's verbatim verification, never by the planner.
public sealed record CriterionVerdict(
    string RequesterCriterionId, CriterionVerdictKind Verdict,
    IReadOnlyList<EvidenceRef> Evidence, string Reason);

public sealed record Plan(
    RunId RunId, int Version, IReadOnlyList<PlannedTask> Tasks,
    string Rationale, string? RevisionReason, DateTimeOffset CreatedAt);

public sealed record PlannedTask(
    string TaskKey, string Title, string CapabilityId,
    string AgentId, string AgentVersion,            // chosen by AgentSelector, not by the LLM
    IReadOnlyList<InputBinding> Inputs,
    IReadOnlyList<string> After,                    // control-only dependencies
    IReadOnlyList<string> GrantedOperations,        // computed by the validator, subset of the agent's declared operations
    IReadOnlyList<AcceptanceCriterion> Acceptance,
    TaskLimits Limits, string Rationale,
    bool Retained);                                 // carried over unchanged from an earlier plan version

public enum BindingKind { Literal, FromTask, AskHuman }
public enum ValueProvenance { Requester, HumanClarification }

public sealed record InputBinding(
    string Field, BindingKind Kind,
    JsonElement? Value, ValueProvenance? Provenance,                      // Literal
    string? SourceTaskKey, string? SourcePointer, bool AllowIncomplete,   // FromTask
    string? QuestionId);                                                  // AskHuman

public enum CriterionKind { Schema, FieldPresent, MinCount, Coverage, EvidenceResolves, Judged }

public sealed record AcceptanceCriterion(
    string Id, CriterionKind Kind, string Description,
    JsonElement Parameters,        // shape fixed per Kind, schema-validated
    bool Required,                 // failure means Rejected
    bool IsCompleteness);          // failure with reported gaps means Incomplete

public sealed record TaskLimits(int MaxRetries, int MaxSeconds);
