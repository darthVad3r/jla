using System.Text.RegularExpressions;

namespace Jla.Tests.Architecture;

/// <summary>Source-level rules for the namespaces inside Jla.Core and for the write interfaces. Scans .cs files, so it works before the types exist.</summary>
public static partial class NamespaceRules
{
    public static IReadOnlyList<string> Violations(string repoRoot)
    {
        var v = new List<string>();
        var src = Path.Combine(repoRoot, "src");

        // Core.Governance does not reference Core.Orchestration.
        foreach (var f in Files(Path.Combine(src, "Jla.Core", "Governance")))
            if (OrchestrationRef().IsMatch(File.ReadAllText(f)))
                v.Add($"{Rel(repoRoot, f)} (Governance) references Jla.Core.Orchestration");

        // Only Core.Orchestration uses IPlanWriter and IAcceptanceWriter. The interfaces are declared under Core/Store.
        foreach (var f in Files(src))
        {
            var rel = Rel(repoRoot, f);
            if (Under(rel, "src/Jla.Core/Orchestration") || Under(rel, "src/Jla.Core/Store") || Under(rel, "src/Jla.Persistence")) continue;
            if (WriterRef().IsMatch(File.ReadAllText(f))) v.Add($"{rel} uses IPlanWriter or IAcceptanceWriter outside Core.Orchestration");
        }

        // Only Core.Execution uses IOperationHandler. Jla.Simulation implements it, and Contracts declares it.
        foreach (var f in Files(src))
        {
            var rel = Rel(repoRoot, f);
            if (Under(rel, "src/Jla.Core/Execution") || Under(rel, "src/Jla.Simulation") || Under(rel, "src/Jla.Contracts")
                || Under(rel, "src/Jla.Host")) continue;
            if (HandlerRef().IsMatch(File.ReadAllText(f))) v.Add($"{rel} uses IOperationHandler outside Core.Execution");
        }
        return v;
    }

    private static IEnumerable<string> Files(string dir) =>
        Directory.Exists(dir)
            ? Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                         && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            : [];

    private static string Rel(string root, string f) => Path.GetRelativePath(root, f).Replace('\\', '/');
    private static bool Under(string rel, string prefix) => rel.StartsWith(prefix + "/", StringComparison.Ordinal);

    [GeneratedRegex(@"\bJla\.Core\.Orchestration\b")] private static partial Regex OrchestrationRef();
    [GeneratedRegex(@"\b(IPlanWriter|IAcceptanceWriter)\b")] private static partial Regex WriterRef();
    [GeneratedRegex(@"\bIOperationHandler\b")] private static partial Regex HandlerRef();
}
