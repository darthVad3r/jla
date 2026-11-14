namespace Jla.Tests.Unit;

internal static class RepoPaths
{
    public static string Root { get; } = Find();

    private static string Find()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Jla.sln"))) dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Jla.sln not found above " + AppContext.BaseDirectory);
    }
}
