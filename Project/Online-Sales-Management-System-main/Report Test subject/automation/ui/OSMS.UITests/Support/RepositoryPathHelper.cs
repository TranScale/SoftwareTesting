namespace OSMS.UITests.Support;

public static class RepositoryPathHelper
{
    private static readonly Lazy<string> RepoRoot = new(FindRepositoryRoot);

    public static string GetRepositoryRoot() => RepoRoot.Value;

    public static string ResolveFromRepository(string relativePath)
    {
        var normalized = relativePath
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        return Path.GetFullPath(Path.Combine(GetRepositoryRoot(), normalized));
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current != null)
        {
            var gitPath = Path.Combine(current.FullName, ".git");
            var reportPath = Path.Combine(current.FullName, "Report Test subject");

            if (Directory.Exists(gitPath) && Directory.Exists(reportPath))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root from the test project output directory.");
    }
}
