namespace ScreenshotApp.Core.Services.Interfaces;

/// <summary>
/// Service for managing application theme and appearance.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets the current application theme.
    /// </summary>
    AppTheme CurrentTheme { get; }

    /// <summary>
    /// Gets whether the system is currently using dark mode.
    /// </summary>
    bool IsSystemDarkMode { get; }

    /// <summary>
    /// Sets the application theme.
    /// </summary>
    /// <param name="theme">The theme to apply.</param>
    void SetTheme(AppTheme theme);

    /// <summary>
    /// Refreshes the theme based on current system settings.
    /// </summary>
    void RefreshSystemTheme();

    /// <summary>
    /// Occurs when the theme changes.
    /// </summary>
    event EventHandler? ThemeChanged;
}
