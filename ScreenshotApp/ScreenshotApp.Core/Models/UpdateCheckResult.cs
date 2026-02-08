namespace ScreenshotApp.Core.Models;

/// <summary>
/// Represents the result of an update check operation.
/// </summary>
public class UpdateCheckResult
{
    /// <summary>
    /// Gets or sets a value indicating whether an update is available.
    /// </summary>
    public bool IsUpdateAvailable { get; set; }

    /// <summary>
    /// Gets or sets the current application version.
    /// </summary>
    public Version? CurrentVersion { get; set; }

    /// <summary>
    /// Gets or sets the latest version available from the remote source.
    /// </summary>
    public Version? LatestVersion { get; set; }

    /// <summary>
    /// Gets or sets the URL to the release page for the latest version.
    /// </summary>
    public string? ReleaseUrl { get; set; }

    /// <summary>
    /// Gets or sets an error message if the update check failed.
    /// Null if the check was successful.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets a value indicating whether the update check resulted in an error.
    /// </summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Creates a failed update check result with the specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>A failed update check result.</returns>
    public static UpdateCheckResult Failed(string errorMessage)
    {
        return new UpdateCheckResult
        {
            IsUpdateAvailable = false,
            ErrorMessage = errorMessage
        };
    }

    /// <summary>
    /// Creates a successful update check result indicating no update is available.
    /// </summary>
    /// <param name="currentVersion">The current version.</param>
    /// <returns>A successful update check result.</returns>
    public static UpdateCheckResult UpToDate(Version currentVersion)
    {
        return new UpdateCheckResult
        {
            IsUpdateAvailable = false,
            CurrentVersion = currentVersion,
            LatestVersion = currentVersion
        };
    }

    /// <summary>
    /// Creates a successful update check result indicating an update is available.
    /// </summary>
    /// <param name="currentVersion">The current version.</param>
    /// <param name="latestVersion">The latest available version.</param>
    /// <param name="releaseUrl">The URL to the release page.</param>
    /// <returns>A successful update check result with update available.</returns>
    public static UpdateCheckResult UpdateAvailable(Version currentVersion, Version latestVersion, string releaseUrl)
    {
        return new UpdateCheckResult
        {
            IsUpdateAvailable = true,
            CurrentVersion = currentVersion,
            LatestVersion = latestVersion,
            ReleaseUrl = releaseUrl
        };
    }
}
