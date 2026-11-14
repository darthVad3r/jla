using Jla.Contracts;

namespace Jla.Core;

/// <summary>
/// Trusted tenant context. Every tenant-owned store operation takes one and applies an explicit tenant predicate.
/// It can be built only from the authenticated <see cref="RequesterContext"/> (public) or from a run that was
/// itself loaded under a scope (internal). Nothing an LLM, a plan, or an agent supplies can become a TenantScope.
/// </summary>
public sealed record TenantScope
{
    public TenantId TenantId { get; }

    private TenantScope(TenantId tenantId) => TenantId = tenantId;

    public static TenantScope For(RequesterContext requester) => new(requester.TenantId);

    internal static TenantScope For(Run run) => new(run.TenantId);
}
