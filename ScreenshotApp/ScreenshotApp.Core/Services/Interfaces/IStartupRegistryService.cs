namespace ScreenshotApp.Core.Services.Interfaces;

/// <summary>
/// Service for managing Windows startup registry entries.
/// Handles registration and unregistration of the application in Windows startup.
/// </summary>
public interface IStartupRegistryService
{
    /// <summary>
    /// Gets a value indicating whether the application is registered to start with Windows.
    /// </summary>
    bool IsRegistered { get; }

    /// <summary>
    /// Registers the application to start with Windows.
    /// </summary>
    /// <param name="error">Error message if registration fails; otherwise, null.</param>
    /// <returns>True if registration succeeds; otherwise, false.</returns>
    bool TryRegisterStartup(out string? error);

    /// <summary>
    /// Unregisters the application from Windows startup.
    /// </summary>
    /// <param name="error">Error message if unregistration fails; otherwise, null.</param>
    /// <returns>True if unregistration succeeds; otherwise, false.</returns>
    bool TryUnregisterStartup(out string? error);
}
