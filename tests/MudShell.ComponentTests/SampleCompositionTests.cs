using System.Text.RegularExpressions;
using Xunit;

namespace MudShell.ComponentTests;

public class SampleCompositionTests
{
    private static readonly Regex MainContentWrapperTag = new(@"</?(MdsMainPart|MdsMainSection)\b[^>]*>", RegexOptions.Compiled);

    [Fact]
    public void Samples_UseComponentStylesWithoutCustomCss()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !Directory.Exists(Path.Combine(root.FullName, "samples")))
            root = root.Parent;
        Assert.NotNull(root);

        var files = Directory.EnumerateFiles(Path.Combine(root.FullName, "samples"), "*", SearchOption.AllDirectories)
            .Where(path => !path.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj"))
            .ToArray();
        Assert.DoesNotContain(files, path => path.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".css.map", StringComparison.OrdinalIgnoreCase));
        var components = files.Where(path => path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase)).ToArray();
        Assert.NotEmpty(components);
        foreach (var path in components)
        {
            Assert.False(Regex.IsMatch(File.ReadAllText(path), @"(?i)<style\b|\bstyle\s*="),
                $"Sample components must use component parameters, not custom CSS: {path}");
        }
    }

    [Fact]
    public void Samples_DoNotNestMainContentWrappers()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !Directory.Exists(Path.Combine(root.FullName, "samples")))
            root = root.Parent;
        Assert.NotNull(root);

        var components = Directory.EnumerateFiles(Path.Combine(root.FullName, "samples"), "*.razor", SearchOption.AllDirectories)
            .Where(path => !path.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj"));
        foreach (var path in components)
        {
            var wrappers = new Stack<string>();
            foreach (Match match in MainContentWrapperTag.Matches(File.ReadAllText(path)))
            {
                var tagName = match.Groups[1].Value;
                if (match.Value.StartsWith("</", StringComparison.Ordinal))
                {
                    Assert.True(wrappers.Count > 0, $"Unexpected closing {tagName} in {path}");
                    Assert.Equal(tagName, wrappers.Pop());
                    continue;
                }

                if (wrappers.Count > 0)
                    Assert.Fail($"Main content wrappers must be siblings, not nested; {tagName} is nested in {wrappers.Peek()} in {path}");
                if (!match.Value.EndsWith("/>", StringComparison.Ordinal))
                    wrappers.Push(tagName);
            }

            Assert.Empty(wrappers);
        }
    }
}
