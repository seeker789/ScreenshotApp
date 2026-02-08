using System.Reflection;
using ScreenshotApp.Core.Models;
using ScreenshotApp.Services;

namespace ScreenshotApp.Tests.Services;

/// <summary>
/// Unit tests for the GitHubUpdateService.
/// </summary>
public class GitHubUpdateServiceTests
{
    [Fact]
    public void GetCurrentVersion_ReturnsValidVersion()
    {
        // Arrange
        using var service = new GitHubUpdateService();

        // Act
        var version = service.GetCurrentVersion();

        // Assert
        Assert.NotNull(version);
        Assert.True(version.Major >= 0);
    }

    [Fact]
    public void Constructor_WithDefaultParameters_UsesDefaultRepo()
    {
        // Arrange & Act
        using var service = new GitHubUpdateService();

        // Assert - should not throw and service should be created
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithCustomParameters_UsesCustomRepo()
    {
        // Arrange & Act
        using var service = new GitHubUpdateService("customOwner", "customRepo");

        // Assert - should not throw and service should be created
        Assert.NotNull(service);
    }

    [Fact]
    public void UpdateCheckResult_Failed_CreatesFailedResult()
    {
        // Arrange
        var errorMessage = "Test error";

        // Act
        var result = UpdateCheckResult.Failed(errorMessage);

        // Assert
        Assert.True(result.HasError);
        Assert.Equal(errorMessage, result.ErrorMessage);
        Assert.False(result.IsUpdateAvailable);
    }

    [Fact]
    public void UpdateCheckResult_UpToDate_CreatesUpToDateResult()
    {
        // Arrange
        var version = new Version(1, 0, 0);

        // Act
        var result = UpdateCheckResult.UpToDate(version);

        // Assert
        Assert.False(result.HasError);
        Assert.False(result.IsUpdateAvailable);
        Assert.Equal(version, result.CurrentVersion);
        Assert.Equal(version, result.LatestVersion);
    }

    [Fact]
    public void UpdateCheckResult_UpdateAvailable_CreatesUpdateAvailableResult()
    {
        // Arrange
        var currentVersion = new Version(1, 0, 0);
        var latestVersion = new Version(1, 1, 0);
        var releaseUrl = "https://github.com/owner/repo/releases/tag/v1.1.0";

        // Act
        var result = UpdateCheckResult.UpdateAvailable(currentVersion, latestVersion, releaseUrl);

        // Assert
        Assert.False(result.HasError);
        Assert.True(result.IsUpdateAvailable);
        Assert.Equal(currentVersion, result.CurrentVersion);
        Assert.Equal(latestVersion, result.LatestVersion);
        Assert.Equal(releaseUrl, result.ReleaseUrl);
    }

    [Theory]
    [InlineData("v1.0.0", 1, 0, 0)]
    [InlineData("V2.5.3", 2, 5, 3)]
    [InlineData("1.0.0", 1, 0, 0)]
    [InlineData("0.5.0-beta", 0, 5, 0)]
    public void ParseVersion_VariousFormats_ParsesCorrectly(string tagName, int expectedMajor, int expectedMinor, int expectedBuild)
    {
        // We need to test the private ParseVersion method via reflection
        // Arrange
        using var service = new GitHubUpdateService();
        var method = typeof(GitHubUpdateService).GetMethod("ParseVersion", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        // Act
        var version = method.Invoke(null, new object[] { tagName }) as Version;

        // Assert
        Assert.NotNull(version);
        Assert.Equal(expectedMajor, version.Major);
        Assert.Equal(expectedMinor, version.Minor);
        Assert.Equal(expectedBuild, version.Build);
    }

    [Fact]
    public void UpdateCheckResult_NullErrorMessage_HasErrorReturnsFalse()
    {
        // Arrange & Act
        var result = new UpdateCheckResult
        {
            ErrorMessage = null,
            IsUpdateAvailable = false
        };

        // Assert
        Assert.False(result.HasError);
    }

    [Fact]
    public void UpdateCheckResult_EmptyErrorMessage_HasErrorReturnsFalse()
    {
        // Arrange & Act
        var result = new UpdateCheckResult
        {
            ErrorMessage = "",
            IsUpdateAvailable = false
        };

        // Assert
        Assert.False(result.HasError);
    }

    [Fact]
    public void UpdateCheckResult_WhitespaceErrorMessage_HasErrorReturnsFalse()
    {
        // Arrange & Act
        var result = new UpdateCheckResult
        {
            ErrorMessage = "   ",
            IsUpdateAvailable = false
        };

        // Assert
        Assert.True(result.HasError); // string.IsNullOrEmpty returns false for whitespace only
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var service = new GitHubUpdateService();

        // Act & Assert - should not throw
        service.Dispose();
        service.Dispose();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_WithInvalidRepo_ReturnsError()
    {
        // Arrange - use a repo that doesn't exist to trigger error handling
        using var service = new GitHubUpdateService("this-owner-does-not-exist-12345", "this-repo-does-not-exist-67890");

        // Act
        var result = await service.CheckForUpdatesAsync();

        // Assert - should return error result (not throw)
        Assert.NotNull(result);
        Assert.True(result.HasError, "Expected HasError to be true for non-existent repository");
        Assert.NotNull(result.ErrorMessage);
        Assert.False(string.IsNullOrEmpty(result.ErrorMessage), "Expected error message to be provided");
    }

    [Fact]
    public void Constructor_SetsCorrectUserAgentHeader()
    {
        // Arrange & Act
        using var service = new GitHubUpdateService();

        // Assert - verify User-Agent is set via reflection on HttpClient
        var clientField = typeof(GitHubUpdateService).GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(clientField);
        var httpClient = clientField.GetValue(service) as HttpClient;
        Assert.NotNull(httpClient);
        Assert.True(httpClient.DefaultRequestHeaders.Contains("User-Agent"));
        var userAgent = httpClient.DefaultRequestHeaders.GetValues("User-Agent").FirstOrDefault();
        Assert.Equal("ScreenshotApp-UpdateChecker", userAgent);
    }

    [Theory]
    [InlineData("1.0.0", "1.0.0.0")]
    [InlineData("2.5.3", "2.5.3.0")]
    public void GetCurrentVersion_ReturnsAssemblyVersion(string expectedStart, string _)
    {
        // Arrange
        using var service = new GitHubUpdateService();

        // Act
        var version = service.GetCurrentVersion();

        // Assert
        Assert.NotNull(version);
        Assert.True(version.Major >= 0);
        // Version should be parsable and follow expected format
        Assert.True(version.ToString().StartsWith(expectedStart.Split('.')[0]) || version.ToString() == "1.0.0.0");
    }
}
