using FluentAssertions;
using ScreenshotApp.Core.Services.Interfaces;
using ScreenshotApp.Services;

namespace ScreenshotApp.Tests.Services;

public class StartupRegistryServiceTests : IDisposable
{
    private readonly StartupRegistryService _service;

    public StartupRegistryServiceTests()
    {
        _service = new StartupRegistryService();

        // Clean up any existing registration before each test
        _service.TryUnregisterStartup(out _);
    }

    public void Dispose()
    {
        // Clean up after tests
        _service.TryUnregisterStartup(out _);
    }

    [Fact]
    public void IsRegistered_WhenNotRegistered_ShouldReturnFalse()
    {
        // Arrange - ensure not registered
        _service.TryUnregisterStartup(out _);

        // Act
        var result = _service.IsRegistered;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TryRegisterStartup_ShouldRegisterSuccessfully()
    {
        // Act
        var success = _service.TryRegisterStartup(out var error);

        // Assert
        success.Should().BeTrue($"because error should be null but was: {error}");
        error.Should().BeNull();
        _service.IsRegistered.Should().BeTrue();
    }

    [Fact]
    public void TryRegisterStartup_ShouldIncludeAutoStartedArgument()
    {
        // Act
        _service.TryRegisterStartup(out _);

        // Assert - Verify registry value contains --auto-started argument
        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
        var value = key?.GetValue("ScreenshotApp") as string;

        value.Should().NotBeNullOrEmpty();
        value.Should().Contain("--auto-started", "because the registry value must include the silent startup detection argument");
        value.Should().StartWith("\"", "because the executable path must be quoted for paths with spaces");
    }

    [Fact]
    public void TryUnregisterStartup_WhenRegistered_ShouldUnregisterSuccessfully()
    {
        // Arrange
        _service.TryRegisterStartup(out _);
        _service.IsRegistered.Should().BeTrue();

        // Act
        var success = _service.TryUnregisterStartup(out var error);

        // Assert
        success.Should().BeTrue($"because error should be null but was: {error}");
        error.Should().BeNull();
        _service.IsRegistered.Should().BeFalse();
    }

    [Fact]
    public void TryUnregisterStartup_WhenNotRegistered_ShouldReturnSuccess()
    {
        // Arrange - ensure not registered
        _service.TryUnregisterStartup(out _);

        // Act
        var success = _service.TryUnregisterStartup(out var error);

        // Assert
        success.Should().BeTrue();
        // Error may be null (success) or indicate already not registered
    }

    [Fact]
    public void IsRegistered_AfterRegisterThenUnregister_ShouldReturnFalse()
    {
        // Arrange
        _service.TryRegisterStartup(out _);
        _service.IsRegistered.Should().BeTrue();

        // Act
        _service.TryUnregisterStartup(out _);
        var result = _service.IsRegistered;

        // Assert
        result.Should().BeFalse();
    }
}
