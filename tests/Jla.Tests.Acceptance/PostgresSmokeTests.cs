using Testcontainers.PostgreSql;

namespace Jla.Tests.Acceptance;

/// <summary>Proves the Testcontainers PostgreSQL harness works on this machine. Needs Docker. Run with: dotnet test --filter Category=Integration</summary>
[Trait("Category", "Integration")]
public class PostgresSmokeTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder().WithImage("postgres:16").Build();

    public Task InitializeAsync() => _pg.StartAsync();
    public Task DisposeAsync() => _pg.DisposeAsync().AsTask();

    [Fact]
    public async Task Container_starts_and_answers_a_query()
    {
        var result = await _pg.ExecScriptAsync("select 1;");
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("1", result.Stdout);
    }
}
