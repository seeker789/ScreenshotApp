namespace ScreenshotApp.Core.Services.Interfaces;

/// <summary>
/// Service for managing application settings.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets or sets the capture hotkey configuration.
    /// </summary>
    HotkeyConfig CaptureHotkey { get; set; }

    /// <summary>
    /// Gets or sets whether the application starts with Windows.
    /// </summary>
    bool AutoStartWithWindows { get; set; }

    /// <summary>
    /// Gets or sets the application theme.
    /// </summary>
    AppTheme Theme { get; set; }

    /// <summary>
    /// Gets or sets the default save location for screenshots.
    /// </summary>
    string DefaultSaveLocation { get; set; }

    /// <summary>
    /// Loads settings from persistent storage.
    /// </summary>
    void Load();

    /// <summary>
    /// Saves settings to persistent storage.
    /// </summary>
    void Save();

    /// <summary>
    /// Resets all settings to default values.
    /// </summary>
    void ResetToDefaults();

    /// <summary>
    /// Occurs when settings are changed.
    /// </summary>
    event EventHandler? SettingsChanged;

    /// <summary>
    /// Gets or sets whether a hotkey conflict was detected.
    /// </summary>
    bool HotkeyConflictDetected { get; set; }

    /// <summary>
    /// Gets or sets the fallback hotkey used when conflict was detected.
    /// </summary>
    KeyCombo? HotkeyConflictFallback { get; set; }
}

/// <summary>
/// Hotkey configuration.
/// </summary>
public class HotkeyConfig
{
    public KeyModifiers Modifiers { get; set; }
    public uint Key { get; set; }

    public HotkeyConfig()
    {
        Modifiers = KeyModifiers.Control | KeyModifiers.Shift;
        Key = 0x53; // 'S' key
    }

    public HotkeyConfig(KeyModifiers modifiers, uint key)
    {
        Modifiers = modifiers;
        Key = key;
    }
}

/// <summary>
/// Application theme options.
/// </summary>
public enum AppTheme
{
    System,
    Light,
    Dark
}
