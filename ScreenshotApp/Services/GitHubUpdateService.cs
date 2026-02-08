using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using ScreenshotApp.Core.Models;
using ScreenshotApp.Core.Services.Interfaces;
using ScreenshotApp.Infrastructure.Models;

namespace ScreenshotApp.Services;

/// <summary>
/// GitHub API implementation of IUpdateService for checking application updates.
/// </summary>
public class GitHubUpdateService : IUpdateService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _repoOwner;
    private readonly string _repoName;
    private bool _disposed;

    // Default repository for the ScreenshotApp project
    public const string DefaultRepoOwner = "seeker789";
    public const string DefaultRepoName = "ScreenshotApp";

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubUpdateService"/> class.
    /// </summary>
    /// <param name="repoOwner">The GitHub repository owner. Defaults to "seeker".</param>
    /// <param name="repoName">The GitHub repository name. Defaults to "ScreenshotApp".</param>
    public GitHubUpdateService(string? repoOwner = null, string? repoName = null)
    {
        _repoOwner = repoOwner ?? DefaultRepoOwner;
        _repoName = repoName ?? DefaultRepoName;

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "ScreenshotApp-UpdateChecker");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
    }

    /// <inheritdoc />
    public async Task<UpdateCheckResult> CheckForUpdatesAsync()
    {
        try
        {
            var currentVersion = GetCurrentVersion();
            var apiUrl = $"https://api.github.com/repos/{_repoOwner}/{_repoName}/releases/latest";

            Debug.WriteLine($"Checking for updates at: {apiUrl}");

            var response = await _httpClient.GetStringAsync(apiUrl);

            GitHubRelease? release;
            try
            {
                release = JsonSerializer.Deserialize<GitHubRelease>(response);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Failed to parse GitHub response: {ex}");
                return UpdateCheckResult.Failed("Invalid response from update server.");
            }

            if (release == null || string.IsNullOrEmpty(release.TagName))
            {
                return UpdateCheckResult.Failed("Invalid release information received.");
            }

            // Skip prerelease versions to avoid prompting users for unstable updates
            if (release.Prerelease)
            {
                Debug.WriteLine("Latest release is a prerelease, checking for stable version...");
                // Note: In a future enhancement, we could query all releases and find the latest stable one
                // For now, we treat prerelease as "no update available" to prevent unstable updates
                return UpdateCheckResult.UpToDate(currentVersion);
            }

            var latestVersion = ParseVersion(release.TagName);

            // Compare versions
            if (latestVersion > currentVersion)
            {
                return UpdateCheckResult.UpdateAvailable(
                    currentVersion,
                    latestVersion,
                    release.HtmlUrl);
            }

            return UpdateCheckResult.UpToDate(currentVersion);
        }
        catch (TaskCanceledException)
        {
            Debug.WriteLine("Update check timed out");
            return UpdateCheckResult.Failed("Request timed out. Please check your connection.");
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"Network error checking for updates: {ex}");
            return UpdateCheckResult.Failed("Unable to connect to update server.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unexpected error checking for updates: {ex}");
            return UpdateCheckResult.Failed("An unexpected error occurred. Please try again later.");
        }
    }

    /// <inheritdoc />
    public Version GetCurrentVersion()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version ?? new Version(1, 0, 0);
        }
        catch
        {
            return new Version(1, 0, 0);
        }
    }

    /// <summary>
    /// Parses a version string from a GitHub tag name.
    /// Handles 'v' prefix (e.g., "v1.0.0" -> "1.0.0").
    /// </summary>
    /// <param name="tagName">The tag name to parse.</param>
    /// <returns>The parsed version.</returns>
    private static Version ParseVersion(string tagName)
    {
        // Remove 'v' or 'V' prefix if present
        var versionString = tagName.TrimStart('v', 'V');

        // Try to parse the version
        if (Version.TryParse(versionString, out var version))
        {
            return version;
        }

        // Fallback: try to extract version-like pattern
        var match = System.Text.RegularExpressions.Regex.Match(
            versionString,
            @"(\d+)\.(\d+)(?:\.(\d+))?");

        if (match.Success)
        {
            var major = int.Parse(match.Groups[1].Value);
            var minor = int.Parse(match.Groups[2].Value);
            var build = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;
            return new Version(major, minor, build);
        }

        Debug.WriteLine($"Could not parse version from: {tagName}");
        return new Version(0, 0);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}
