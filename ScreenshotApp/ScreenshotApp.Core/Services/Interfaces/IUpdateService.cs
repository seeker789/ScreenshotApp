using ScreenshotApp.Core.Models;

namespace ScreenshotApp.Core.Services.Interfaces;

/// <summary>
/// Service for checking application updates from remote sources.
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// Checks for available updates asynchronously.
    /// </summary>
    /// <returns>A task representing the update check result.</returns>
    Task<UpdateCheckResult> CheckForUpdatesAsync();

    /// <summary>
    /// Gets the current application version.
    /// </summary>
    /// <returns>The current version.</returns>
    Version GetCurrentVersion();
}
