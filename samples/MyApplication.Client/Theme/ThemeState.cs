using MudBlazor;
using MudShell.Components.AppShell;
using MudShell.Theme;

namespace MyApplication.Client.Theme;

public sealed class ThemeState
{
    public event Action? Changed;

    public bool IsDarkMode { get; private set; } = true;
    public MbxTheme.MbxThemePreset SelectedPreset { get; private set; } = MbxTheme.MbxThemePreset.Indigo;
    public MudTheme CurrentTheme { get; private set; } = MbxTheme.CreateTheme(MbxTheme.MbxThemePreset.Indigo);
    public MdsAppShell.MbxBackgroundMode BackgroundMode { get; private set; } = MdsAppShell.MbxBackgroundMode.Palette;
    public string? BackgroundImageUrl { get; private set; }

    public IReadOnlyList<MbxTheme.PresetInfo> Presets => MbxTheme.Presets;

    public void SetDarkMode(bool isDarkMode)
    {
        if (IsDarkMode == isDarkMode)
            return;

        IsDarkMode = isDarkMode;
        Changed?.Invoke();
    }

    public void ToggleDarkMode() => SetDarkMode(!IsDarkMode);

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
