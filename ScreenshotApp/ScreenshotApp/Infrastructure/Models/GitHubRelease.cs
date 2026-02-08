using System.Text.Json.Serialization;

namespace ScreenshotApp.Infrastructure.Models;

/// <summary>
/// Represents a GitHub release response from the GitHub API.
/// </summary>
public class GitHubRelease
{
    /// <summary>
    /// Gets or sets the tag name (version) of the release.
    /// </summary>
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the release.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTML URL to the release page.
    /// </summary>
    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the release notes/body.
    /// </summary>
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is a prerelease.
    /// </summary>
    [JsonPropertyName("prerelease")]
    public bool Prerelease { get; set; }

    /// <summary>
    /// Gets or sets the published date.
    /// </summary>
    [JsonPropertyName("published_at")]
    public DateTime PublishedAt { get; set; }
}
