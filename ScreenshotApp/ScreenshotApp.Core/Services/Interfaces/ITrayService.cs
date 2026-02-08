namespace ScreenshotApp.Core.Services.Interfaces;

/// <summary>
/// Service for managing the system tray icon and interactions.
/// </summary>
public interface ITrayService : IDisposable
{
    /// <summary>
    /// Gets whether the tray icon is currently visible.
    /// </summary>
    bool IsVisible { get; }

    /// <summary>
    /// Gets whether the tray service has been initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Initializes and shows the tray icon.
    /// Must be called on the UI thread.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Shows a balloon tip notification.
    /// </summary>
    /// <param name="title">The notification title.</param>
    /// <param name="message">The notification message.</param>
    /// <param name="icon">The icon type.</param>
    void ShowNotification(string title, string message, TrayIconType icon = TrayIconType.Info);

    /// <summary>
    /// Occurs when the user clicks the tray icon with the left mouse button.
    /// </summary>
    event EventHandler? TrayLeftClick;

    /// <summary>
    /// Occurs when the user requests to capture a region from the tray menu.
    /// </summary>
    event EventHandler? CaptureRequested;

    /// <summary>
    /// Occurs when the user requests to open settings from the tray menu.
    /// </summary>
    event EventHandler? SettingsRequested;

    /// <summary>
    /// Occurs when the user requests to check for updates from the tray menu.
    /// </summary>
    event EventHandler? CheckForUpdatesRequested;

    /// <summary>
    /// Occurs when the user requests to show the about dialog from the tray menu.
    /// </summary>
    event EventHandler? AboutRequested;

    /// <summary>
    /// Occurs when the user requests to exit the application from the tray menu.
    /// </summary>
    event EventHandler? ExitRequested;
}

/// <summary>
/// Icon types for tray notifications.
/// </summary>
public enum TrayIconType
{
    None,
    Info,
    Warning,
    Error
}
