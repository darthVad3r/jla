using System.Reflection;
using Jla.Contracts;
using Jla.Core;

namespace Jla.Tests.Unit;

public class TenantScopeTests
{
    [Fact]
    public void Has_no_public_constructor()
    {
        Assert.Empty(typeof(TenantScope).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
    }

    [Fact]
    public void Only_the_requester_overload_is_public()
    {
        var factories = typeof(TenantScope).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(m => m.ReturnType == typeof(TenantScope)).ToList();
        var pub = Assert.Single(factories, m => m.IsPublic);
        Assert.Equal(typeof(RequesterContext), Assert.Single(pub.GetParameters()).ParameterType);
        Assert.Equal(typeof(Run), Assert.Single(factories, m => !m.IsPublic).GetParameters().Single().ParameterType);
    }

    [Fact]
    public void Scope_carries_the_requesters_tenant()
    {
        var tenant = new TenantId(Guid.NewGuid());
        var scope = TenantScope.For(new RequesterContext(tenant, "p1", new HashSet<string> { "approver" }));
        Assert.Equal(tenant, scope.TenantId);
    }
}
