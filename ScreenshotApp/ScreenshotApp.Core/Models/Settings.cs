using ScreenshotApp.Core.Services.Interfaces;

namespace ScreenshotApp.Core.Models;

/// <summary>
/// Application settings data model.
/// </summary>
public class Settings
{
    /// <summary>
    /// Gets or sets the capture hotkey configuration.
    /// </summary>
    public HotkeyConfig CaptureHotkey { get; set; }

    /// <summary>
    /// Gets or sets whether the application starts with Windows.
    /// </summary>
    public bool AutoStartWithWindows { get; set; }

    /// <summary>
    /// Gets or sets the application theme.
    /// </summary>
    public AppTheme Theme { get; set; }

    /// <summary>
    /// Gets or sets the default save location for screenshots.
    /// </summary>
    public string DefaultSaveLocation { get; set; }

    /// <summary>
    /// Gets or sets whether to copy captures to clipboard automatically.
    /// </summary>
    public bool AutoCopyToClipboard { get; set; }

    /// <summary>
    /// Gets or sets whether to show notifications after capture.
    /// </summary>
    public bool ShowCaptureNotifications { get; set; }

    /// <summary>
    /// Gets or sets whether a hotkey conflict was detected.
    /// </summary>
    public bool HotkeyConflictDetected { get; set; }

    /// <summary>
    /// Gets or sets the fallback hotkey used when conflict was detected.
    /// </summary>
    public HotkeyConfig? HotkeyConflictFallback { get; set; }

    public Settings()
    {
        CaptureHotkey = new HotkeyConfig();
        AutoStartWithWindows = false;
        Theme = AppTheme.System;
        DefaultSaveLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Screenshots");
        AutoCopyToClipboard = true;
        ShowCaptureNotifications = true;
    }

    /// <summary>
    /// Creates a copy of these settings.
    /// </summary>
    public Settings Clone()
    {
        return new Settings
        {
            CaptureHotkey = new HotkeyConfig(CaptureHotkey.Modifiers, CaptureHotkey.Key),
            AutoStartWithWindows = AutoStartWithWindows,
            Theme = Theme,
            DefaultSaveLocation = DefaultSaveLocation,
            AutoCopyToClipboard = AutoCopyToClipboard,
            ShowCaptureNotifications = ShowCaptureNotifications,
            HotkeyConflictDetected = HotkeyConflictDetected,
            HotkeyConflictFallback = HotkeyConflictFallback != null
                ? new HotkeyConfig(HotkeyConflictFallback.Modifiers, HotkeyConflictFallback.Key)
                : null
        };
    }
}
