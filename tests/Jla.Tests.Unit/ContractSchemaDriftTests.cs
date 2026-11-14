using Jla.Contracts;

namespace Jla.Tests.Unit;

public class ContractSchemaDriftTests
{
    public static IEnumerable<object[]> Roots() => ContractSchemas.Roots.Keys.Select(k => new object[] { k });

    [Theory]
    [MemberData(nameof(Roots))]
    public void Committed_schema_matches_the_contract_type(string name)
    {
        var path = Path.Combine(RepoPaths.Root, "schemas", name + ".schema.json");
        var generated = ContractSchemas.Export(ContractSchemas.Roots[name]);

        // Regenerate with: JLA_UPDATE_SCHEMAS=1 dotnet test --filter ContractSchemaDriftTests
        if (Environment.GetEnvironmentVariable("JLA_UPDATE_SCHEMAS") == "1")
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, generated);
            return;
        }

        Assert.True(File.Exists(path), $"Missing committed schema {path}. Regenerate with JLA_UPDATE_SCHEMAS=1.");
        Assert.Equal(File.ReadAllText(path).ReplaceLineEndings("\n"), generated);
    }

    [Fact]
    public void No_committed_schema_lacks_a_contract_root()
    {
        var committed = Directory.GetFiles(Path.Combine(RepoPaths.Root, "schemas"), "*.schema.json")
            .Select(f => Path.GetFileName(f)[..^".schema.json".Length])
            .Order(StringComparer.Ordinal);
        Assert.Equal(ContractSchemas.Roots.Keys.Order(StringComparer.Ordinal), committed);
    }
}
