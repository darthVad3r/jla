namespace Jla.Tests.Architecture;

public class ArchitectureTests
{
    private static readonly string Root = FindRoot();

    [Fact]
    public void Solution_has_seven_source_and_three_test_projects()
    {
        var g = ProjectGraph.Load(Root);
        Assert.Equal(DependencyRules.SourceProjects.Concat(DependencyRules.TestProjects).Order(), g.ProjectRefs.Keys.Order());
        Assert.Equal(10, g.ProjectRefs.Count);
    }

    [Fact]
    public void Projects_follow_the_dependency_rules()
    {
        Assert.Empty(DependencyRules.Violations(ProjectGraph.Load(Root)));
    }

    [Fact]
    public void Namespaces_follow_the_rules()
    {
        Assert.Empty(NamespaceRules.Violations(Root));
    }

    // The rules must be able to fail. Each case adds one violating reference to a copy of the real graph.
    [Theory]
    [InlineData("Jla.Agents", "Jla.Simulation", "project")]
    [InlineData("Jla.Simulation", "Jla.Agents", "project")]
    [InlineData("Jla.Agents", "Jla.Core", "project")]
    [InlineData("Jla.Llm.Claude", "Jla.Core", "project")]
    [InlineData("Jla.Core", "Jla.Persistence", "project")]
    [InlineData("Jla.Contracts", "Jla.Core", "project")]
    [InlineData("Jla.Core", "Microsoft.EntityFrameworkCore", "package")]
    [InlineData("Jla.Agents", "Npgsql", "package")]
    [InlineData("Jla.Core", "Anthropic.SDK", "package")]
    [InlineData("Jla.Persistence", "Microsoft.Extensions.Http", "package")]
    public void A_deliberately_violating_reference_is_caught(string from, string to, string kind)
    {
        var real = ProjectGraph.Load(Root);
        var projects = real.ProjectRefs.ToDictionary(kv => kv.Key, kv => (IReadOnlySet<string>)kv.Value.ToHashSet());
        var packages = real.PackageRefs.ToDictionary(kv => kv.Key, kv => (IReadOnlySet<string>)kv.Value.ToHashSet());
        if (kind == "project") projects[from] = projects[from].Append(to).ToHashSet();
        else packages[from] = packages[from].Append(to).ToHashSet();

        Assert.NotEmpty(DependencyRules.Violations(new ProjectGraph(projects, packages)));
    }

    [Fact]
    public void A_fourth_test_project_or_an_extra_source_project_is_caught()
    {
        var real = ProjectGraph.Load(Root);
        var projects = real.ProjectRefs.ToDictionary(kv => kv.Key, kv => kv.Value);
        projects["Jla.Cli"] = new HashSet<string>();
        Assert.NotEmpty(DependencyRules.Violations(new ProjectGraph(projects, real.PackageRefs)));
    }

    [Fact]
    public void Namespace_rules_catch_deliberate_violations()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "jla-arch-" + Guid.NewGuid().ToString("N"));
        try
        {
            void Write(string rel, string text)
            {
                var path = Path.Combine(tmp, rel.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, text);
            }
            Write("src/Jla.Core/Governance/Gate.cs", "using Jla.Core.Orchestration;");
            Write("src/Jla.Core/Execution/Loop.cs", "class L { IPlanWriter w; }");
            Write("src/Jla.Agents/A.cs", "class A { IOperationHandler h; }");
            Write("src/Jla.Core/Orchestration/Ok.cs", "class Ok { IPlanWriter w; IAcceptanceWriter a; }");
            Write("src/Jla.Core/Execution/Ok2.cs", "class Ok2 { IOperationHandler h; }");

            var found = NamespaceRules.Violations(tmp);
            Assert.Equal(3, found.Count);
        }
        finally { if (Directory.Exists(tmp)) Directory.Delete(tmp, true); }
    }

    private static string FindRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Jla.sln"))) dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Jla.sln not found");
    }
}
