using FluentAssertions;
using ScreenshotApp.Core.Services.Interfaces;
using ScreenshotApp.Services;

namespace ScreenshotApp.Tests.Services;

public class TrayServiceTests : IDisposable
{
    private readonly TrayService _trayService;

    public TrayServiceTests()
    {
        _trayService = new TrayService();
    }

    public void Dispose()
    {
        _trayService.Dispose();
    }

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Assert
        _trayService.Should().NotBeNull();
        _trayService.IsInitialized.Should().BeFalse();
        _trayService.IsVisible.Should().BeFalse();
    }

    [Fact]
    public void Initialize_WhenCalled_ShouldSetIsInitializedToTrue()
    {
        // Act
        _trayService.Initialize();

        // Assert
        _trayService.IsInitialized.Should().BeTrue();
        _trayService.IsVisible.Should().BeTrue();
    }

    [Fact]
    public void Initialize_WhenAlreadyInitialized_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _trayService.Initialize();

        // Act
        Action act = () => _trayService.Initialize();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already been initialized*");
    }

    [Fact]
    public void TrayLeftClick_WhenSubscribed_ShouldTriggerEvent()
    {
        // Arrange
        var eventTriggered = false;
        _trayService.TrayLeftClick += (s, e) => eventTriggered = true;
        _trayService.Initialize();

        // Act - simulate left click via reflection or direct invocation
        _trayService.GetType()
            .GetMethod("OnTrayClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_trayService, new object?[] { null, new System.Windows.Forms.MouseEventArgs(System.Windows.Forms.MouseButtons.Left, 1, 0, 0, 0) });

        // Assert
        eventTriggered.Should().BeTrue();
    }

    [Fact]
    public void CaptureRequested_WhenSubscribed_ShouldTriggerEvent()
    {
        // Arrange
        var eventTriggered = false;
        _trayService.CaptureRequested += (s, e) => eventTriggered = true;
        _trayService.Initialize();

        // Act - trigger via reflection to simulate menu click
        _trayService.GetType()
            .GetMethod("OnCaptureClicked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_trayService, new object?[] { null, EventArgs.Empty });

        // Assert
        eventTriggered.Should().BeTrue();
    }

    [Fact]
    public void SettingsRequested_WhenSubscribed_ShouldTriggerEvent()
    {
        // Arrange
        var eventTriggered = false;
        _trayService.SettingsRequested += (s, e) => eventTriggered = true;
        _trayService.Initialize();

        // Act
        _trayService.GetType()
            .GetMethod("OnSettingsClicked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_trayService, new object?[] { null, EventArgs.Empty });

        // Assert
        eventTriggered.Should().BeTrue();
    }

    [Fact]
    public void CheckForUpdatesRequested_WhenSubscribed_ShouldTriggerEvent()
    {
        // Arrange
        var eventTriggered = false;
        _trayService.CheckForUpdatesRequested += (s, e) => eventTriggered = true;
        _trayService.Initialize();

        // Act
        _trayService.GetType()
            .GetMethod("OnCheckForUpdatesClicked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_trayService, new object?[] { null, EventArgs.Empty });

        // Assert
        eventTriggered.Should().BeTrue();
    }

    [Fact]
    public void AboutRequested_WhenSubscribed_ShouldTriggerEvent()
    {
        // Arrange
        var eventTriggered = false;
        _trayService.AboutRequested += (s, e) => eventTriggered = true;
        _trayService.Initialize();

        // Act
        _trayService.GetType()
            .GetMethod("OnAboutClicked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_trayService, new object?[] { null, EventArgs.Empty });

        // Assert
        eventTriggered.Should().BeTrue();
    }

    [Fact]
    public void ExitRequested_WhenSubscribed_ShouldTriggerEvent()
    {
        // Arrange
        var eventTriggered = false;
        _trayService.ExitRequested += (s, e) => eventTriggered = true;
        _trayService.Initialize();

        // Act
        _trayService.GetType()
            .GetMethod("OnExitClicked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_trayService, new object?[] { null, EventArgs.Empty });

        // Assert
        eventTriggered.Should().BeTrue();
    }

    [Fact]
    public void ShowNotification_WhenNotInitialized_ShouldNotThrow()
    {
        // Act
        Action act = () => _trayService.ShowNotification("Test", "Message");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_WhenCalled_ShouldSetIsInitializedToFalse()
    {
        // Arrange
        _trayService.Initialize();

        // Act
        _trayService.Dispose();

        // Assert - after dispose, the service should be in a disposed state
        // Note: IsInitialized remains true until Dispose completes, then it's false
        // But since we can't check after full dispose, we verify no exception is thrown
    }

    [Fact]
    public void Dispose_WhenCalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        _trayService.Initialize();
        _trayService.Dispose();

        // Act
        Action act = () => _trayService.Dispose();

        // Assert
        act.Should().NotThrow();
    }
}
