namespace Jla.Contracts;

public enum ModelTier { Reasoning, Fast }
// Guidance for the model only. Not a security control (design doc, section 6).
public enum SegmentTrust { PlatformInstruction, RequesterIntent, UntrustedData }

public sealed record LlmSegment(string Label, string Content, SegmentTrust Trust);

public sealed record LlmRequest(
    string Purpose,                        // "propose-plan", "judge-criterion", "agent:analyst"
    string SystemInstructions,             // platform-authored, trusted
    IReadOnlyList<LlmSegment> Segments,    // ordered input, each labelled PlatformInstruction, RequesterIntent, or UntrustedData (guidance for the model only)
    JsonElement OutputSchema,              // required
    ModelTier Tier, int MaxOutputTokens, double Temperature = 0);

public sealed record LlmUsage(int InputTokens, int OutputTokens, int CachedInputTokens);
public enum LlmStopReason { EndTurn, MaxTokens, Refusal, Other }

public sealed record LlmResponse(
    JsonElement Output, string RawText, LlmUsage Usage,
    string Provider, string Model, string ProviderRequestId, LlmStopReason StopReason);

public sealed record ModelInfo(string ModelId, decimal InputUsdPerMTok, decimal OutputUsdPerMTok, int MaxOutputTokens);

public enum LlmErrorKind { RateLimited, Transient, Timeout, InvalidRequest, Refused, MalformedOutput, Authentication, Cancelled }
public sealed class LlmException(LlmErrorKind kind, string message, TimeSpan? retryAfter = null) : Exception(message) {
    public LlmErrorKind Kind { get; } = kind;
    public TimeSpan? RetryAfter { get; } = retryAfter; }

public interface ILlmProvider {
    string ProviderId { get; }
    ModelInfo Resolve(ModelTier tier);
    Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken ct);   // honors cancellation
}
