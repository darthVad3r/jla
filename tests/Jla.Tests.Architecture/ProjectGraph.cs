using System.Xml.Linq;

namespace Jla.Tests.Architecture;

/// <summary>Project and package references read straight from the .csproj files, so the rules hold without loading assemblies.</summary>
public sealed record ProjectGraph(IReadOnlyDictionary<string, IReadOnlySet<string>> ProjectRefs,
                                  IReadOnlyDictionary<string, IReadOnlySet<string>> PackageRefs)
{
    public static ProjectGraph Load(string repoRoot)
    {
        var projects = new Dictionary<string, IReadOnlySet<string>>();
        var packages = new Dictionary<string, IReadOnlySet<string>>();
        foreach (var file in Directory.GetFiles(repoRoot, "*.csproj", SearchOption.AllDirectories)
                     .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")))
        {
            var doc = XDocument.Load(file);
            var name = Path.GetFileNameWithoutExtension(file);
            projects[name] = doc.Descendants("ProjectReference")
                .Select(e => Path.GetFileNameWithoutExtension(((string)e.Attribute("Include")!).Replace('\\', '/')))
                .ToHashSet();
            packages[name] = doc.Descendants("PackageReference")
                .Select(e => (string)e.Attribute("Include")!).ToHashSet();
        }
        return new ProjectGraph(projects, packages);
    }
}
