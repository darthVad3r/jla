namespace Jla.Contracts;

public sealed record AgentDefinition(
    string AgentId, string Version, string Description,
    IReadOnlyList<CapabilityDeclaration> Capabilities,
    IReadOnlyList<string> DeclaredOperations,       // catalog IDs. Metadata, not permission.
    AgentConstraints Constraints,
    string ImplementationKey,
    string ContractVersion);                        // compatibility with the Contracts schema version

public sealed record CapabilityDeclaration(
    string CapabilityId, string Description, JsonElement InputSchema, JsonElement OutputSchema,
    IReadOnlyList<AcceptanceCriterion> DefaultAcceptance);   // platform minimums, always added to the plan

public sealed record AgentConstraints(int MaxTaskSeconds, bool UsesLlm, decimal EstimatedCostUsd);

public enum ApprovalClass { ReadOnly = 0, ReversibleOrInternal = 1, Consequential = 2, Unclassified = 3 }

public sealed record CatalogOperation(
    string OperationId, string Description, ApprovalClass Class,
    JsonElement ParameterSchema,
    IReadOnlyList<string> MaterialParameters,       // JSON pointers that an approval binds to
    string? TargetsPointer);                        // pointer to the target resource ids

public sealed record OperationCatalog(int Version, IReadOnlyList<CatalogOperation> Operations);
