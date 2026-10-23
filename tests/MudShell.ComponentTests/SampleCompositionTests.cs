using System.Text.RegularExpressions;
using Xunit;

namespace MudShell.ComponentTests;

public class SampleCompositionTests
{
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
}
