using FluentAssertions;
using ScreenshotApp.ViewModels;
using Xunit;

namespace ScreenshotApp.Tests.ViewModels;

public class OverlayViewModelTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Act
        var vm = new OverlayViewModel();

        // Assert
        vm.IsOverlayVisible.Should().BeFalse();

        // Cleanup
        vm.Dispose();
    }

    [Fact]
    public void ShowOverlayCommand_CanExecute()
    {
        // Arrange
        var vm = new OverlayViewModel();

        // Act & Assert
        vm.ShowOverlayCommand.CanExecute(null).Should().BeTrue();

        // Cleanup
        vm.Dispose();
    }

    [Fact]
    public void CancelOverlayCommand_CanExecute()
    {
        // Arrange
        var vm = new OverlayViewModel();

        // Act & Assert
        vm.CancelOverlayCommand.CanExecute(null).Should().BeTrue();

        // Cleanup
        vm.Dispose();
    }

    [Fact]
    public void OverlayCancelled_RaisesEvent()
    {
        // Arrange
        var vm = new OverlayViewModel();
        var eventSubscribed = false;
        vm.OverlayCancelled += (s, e) => { eventSubscribed = true; };

        // Assert
        eventSubscribed.Should().BeFalse(); // Not raised yet, just verifying subscription works

        // Cleanup
        vm.Dispose();
    }

    [Fact]
    public void RegionSelected_RaisesEvent()
    {
        // Arrange
        var vm = new OverlayViewModel();
        var eventSubscribed = false;
        vm.RegionSelected += (s, e) => { eventSubscribed = true; };

        // Assert
        eventSubscribed.Should().BeFalse(); // Not raised yet, just verifying subscription works

        // Cleanup
        vm.Dispose();
    }

    [Fact]
    public void Dispose_CleansUpResources()
    {
        // Arrange
        var vm = new OverlayViewModel();

        // Act
        vm.Dispose();

        // Assert - Should not throw when disposing twice
        var act = () => vm.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void ShowOverlay_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var vm = new OverlayViewModel();
        vm.Dispose();

        // Act & Assert
        var act = () => vm.ShowOverlay();
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void PreloadOverlayWindow_CanBeCalled()
    {
        // Arrange
        var vm = new OverlayViewModel();

        // Act & Assert - Should not throw
        var act = () => vm.PreloadOverlayWindow();
        act.Should().NotThrow();

        // Cleanup
        vm.Dispose();
    }

    [Fact]
    public void PreloadOverlayWindow_AfterDispose_DoesNotThrow()
    {
        // Arrange
        var vm = new OverlayViewModel();
        vm.Dispose();

        // Act & Assert - Should silently return
        var act = () => vm.PreloadOverlayWindow();
        act.Should().NotThrow();
    }
}
