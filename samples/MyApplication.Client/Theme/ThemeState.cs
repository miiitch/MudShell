using MudBlazor;
using MudShell.Components.AppShell;
using MudShell.Theme;

namespace MyApplication.Client.Theme;

public sealed class ThemeState
{
    public event Action? Changed;

    public bool IsDarkMode { get; private set; } = true;
    public MbxTheme.MbxThemePreset SelectedPreset { get; private set; } = MbxTheme.MbxThemePreset.Cobalt;
    public MudTheme CurrentTheme { get; private set; } = MbxTheme.CreateTheme(MbxTheme.MbxThemePreset.Cobalt);
    public MdsAppShell.MbxBackgroundMode BackgroundMode { get; private set; } = MdsAppShell.MbxBackgroundMode.Palette;
    public string? BackgroundImageUrl { get; private set; }
    public MdsDensity Density { get; private set; } = MdsDensity.Comfortable;

    public IReadOnlyList<MbxTheme.PresetInfo> Presets => MbxTheme.Presets;

    public void SetDarkMode(bool isDarkMode)
    {
        if (IsDarkMode == isDarkMode)
            return;

        IsDarkMode = isDarkMode;
        Changed?.Invoke();
    }

    public void ToggleDarkMode() => SetDarkMode(!IsDarkMode);

    /// <summary>
    /// Applies <c>?preset=&lt;MbxThemePreset&gt;&amp;mode=light|dark</c> from a URI so screenshots
    /// of every preset/mode can be automated.
    /// </summary>
    public void ApplyFromUri(string uri)
    {
        var queryStart = uri.IndexOf('?');
        if (queryStart < 0)
            return;

        foreach (var part in uri[(queryStart + 1)..].Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            if (kv.Length != 2)
                continue;

            var value = Uri.UnescapeDataString(kv[1]);
            if (kv[0].Equals("preset", StringComparison.OrdinalIgnoreCase)
                && Enum.TryParse<MbxTheme.MbxThemePreset>(value, ignoreCase: true, out var preset))
            {
                ApplyPreset(preset);
            }
            else if (kv[0].Equals("mode", StringComparison.OrdinalIgnoreCase))
            {
                if (value.Equals("light", StringComparison.OrdinalIgnoreCase))
                    SetDarkMode(false);
                else if (value.Equals("dark", StringComparison.OrdinalIgnoreCase))
                    SetDarkMode(true);
            }
        }
    }

    public void SetDensity(MdsDensity density)
    {
        if (Density == density)
            return;

        Density = density;
        Changed?.Invoke();
    }

    public void ApplyPreset(MbxTheme.MbxThemePreset preset)
    {
        if (SelectedPreset == preset)
            return;

        SelectedPreset = preset;
        CurrentTheme = MbxTheme.CreateTheme(preset);
        Changed?.Invoke();
    }

    public void ApplyPrimary(string primaryHex)
    {
        CurrentTheme = MbxTheme.CreateThemeFromPrimary(primaryHex);
        Changed?.Invoke();
    }

    public void ApplyTriad(string primaryHex, string secondaryHex, string tertiaryHex)
    {
        CurrentTheme = MbxTheme.CreateThemeFromTriad(primaryHex, secondaryHex, tertiaryHex);
        Changed?.Invoke();
    }

    public void SetBackgroundMode(MdsAppShell.MbxBackgroundMode mode, string? imageUrl = null)
    {
        var normalizedImageUrl = mode == MdsAppShell.MbxBackgroundMode.Image
            ? string.IsNullOrWhiteSpace(imageUrl) ? "/imgs/bg1.jpg" : imageUrl
            : null;

        if (BackgroundMode == mode && string.Equals(BackgroundImageUrl, normalizedImageUrl, StringComparison.Ordinal))
            return;

        BackgroundMode = mode;
        BackgroundImageUrl = normalizedImageUrl;
        Changed?.Invoke();
    }
}
