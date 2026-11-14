using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;

namespace Jla.Contracts;

/// <summary>
/// Single source of truth for the JSON Schemas generated from the contract types.
/// The schemas are committed under /schemas, and a drift test fails when they stop matching the types.
/// </summary>
public static class ContractSchemas
{
    public const string ContractVersion = "1.0";

    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
        RespectNullableAnnotations = true,
        TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver(),
    };

    /// <summary>File name (without extension) to contract type, for the documents that cross a trust boundary or are persisted.</summary>
    public static IReadOnlyDictionary<string, Type> Roots { get; } = new SortedDictionary<string, Type>(StringComparer.Ordinal)
    {
        ["agent-definition"] = typeof(AgentDefinition),
        ["approval"] = typeof(Approval),
        ["audit-event"] = typeof(AuditEvent),
        ["criterion-verdict"] = typeof(CriterionVerdict),
        ["governance-decision"] = typeof(GovernanceDecision),
        ["human-request"] = typeof(HumanRequest),
        ["human-response"] = typeof(HumanResponse),
        ["llm-request"] = typeof(LlmRequest),
        ["llm-response"] = typeof(LlmResponse),
        ["objective"] = typeof(Objective),
        ["operation-catalog"] = typeof(OperationCatalog),
        ["plan"] = typeof(Plan),
        ["plan-proposal"] = typeof(PlanProposal),
        ["run"] = typeof(Run),
        ["task-result"] = typeof(TaskResult),
    };

    public static string Export(Type type)
    {
        var node = JsonSchemaExporter.GetJsonSchemaAsNode(Options, type, new JsonSchemaExporterOptions
        {
            TreatNullObliviousAsNonNullable = true,
        });
        if (node is JsonObject o)
        {
            o["$comment"] = $"Generated from {type.Name}, contract version {ContractVersion}. Do not edit by hand.";
        }
        return node.ToJsonString(Options).ReplaceLineEndings("\n") + "\n";
    }
}
