namespace Jla.Tests.Architecture;

/// <summary>The dependency rules of design doc section 1. Pure functions over a graph, so the rules themselves are testable.</summary>
public static class DependencyRules
{
    public static readonly string[] SourceProjects =
        ["Jla.Contracts", "Jla.Core", "Jla.Persistence", "Jla.Llm.Claude", "Jla.Agents", "Jla.Simulation", "Jla.Host"];

    public static readonly string[] TestProjects =
        ["Jla.Tests.Unit", "Jla.Tests.Architecture", "Jla.Tests.Acceptance"];

    private static readonly string[] EfCorePrefixes = ["Microsoft.EntityFrameworkCore", "Npgsql"];
    private static readonly string[] HttpOrAnthropicPrefixes = ["Anthropic", "Microsoft.Extensions.Http", "System.Net.Http"];

    public static IReadOnlyList<string> Violations(ProjectGraph g)
    {
        var v = new List<string>();
        var all = g.ProjectRefs.Keys.ToHashSet();

        foreach (var expected in SourceProjects.Concat(TestProjects))
            if (!all.Contains(expected)) v.Add($"Missing project {expected}");
        foreach (var extra in all.Except(SourceProjects).Except(TestProjects))
            v.Add($"Unexpected project {extra}. The solution has exactly seven source and three test projects");

        void Only(string project, params string[] allowed)
        {
            if (!g.ProjectRefs.TryGetValue(project, out var refs)) return;
            foreach (var r in refs.Except(allowed))
                v.Add($"{project} must not reference {r}");
        }

        Only("Jla.Contracts");
        Only("Jla.Core", "Jla.Contracts");
        Only("Jla.Persistence", "Jla.Contracts", "Jla.Core");
        Only("Jla.Llm.Claude", "Jla.Contracts");
        Only("Jla.Agents", "Jla.Contracts");
        Only("Jla.Simulation", "Jla.Contracts");
        // Jla.Host is the composition root and may reference every source project.
        Only("Jla.Host", "Jla.Contracts", "Jla.Core", "Jla.Persistence", "Jla.Llm.Claude", "Jla.Agents", "Jla.Simulation");

        // No source project may reference a test project.
        foreach (var s in SourceProjects)
            if (g.ProjectRefs.TryGetValue(s, out var refs))
                foreach (var t in refs.Intersect(TestProjects)) v.Add($"{s} must not reference test project {t}");

        foreach (var (project, pkgs) in g.PackageRefs)
        {
            if (!SourceProjects.Contains(project)) continue;
            foreach (var p in pkgs.Where(p => EfCorePrefixes.Any(p.StartsWith)))
                if (project != "Jla.Persistence") v.Add($"Only Jla.Persistence may reference {p}");
            foreach (var p in pkgs.Where(p => HttpOrAnthropicPrefixes.Any(p.StartsWith)))
                if (project != "Jla.Llm.Claude") v.Add($"Only Jla.Llm.Claude may reference {p}");
        }
        return v;
    }
}
