using FluentAssertions;
using ScreenshotApp.Views;
using Xunit;

namespace ScreenshotApp.Tests.Views;

/// <summary>
/// Tests for OverlayWindow.
/// Note: These tests verify basic construction. Full UI tests require STA thread.
/// </summary>
public class OverlayWindowTests
{
    [Fact]
    public void OverlayWindow_TypeExists()
    {
        // Verify the type exists and can be referenced
        var type = typeof(OverlayWindow);
        type.Should().NotBeNull();
        type.Name.Should().Be("OverlayWindow");
    }

    [Fact]
    public void OverlayWindow_InheritsFromWindow()
    {
        // Verify inheritance
        var type = typeof(OverlayWindow);
        type.BaseType.Should().Be(typeof(System.Windows.Window));
    }

    [Fact]
    public void OverlayWindow_HasOverlayCancelledEvent()
    {
        // Verify the OverlayCancelled event exists
        var eventInfo = typeof(OverlayWindow).GetEvent("OverlayCancelled");
        eventInfo.Should().NotBeNull();
        eventInfo?.EventHandlerType.Should().Be(typeof(EventHandler));
    }

    [Fact]
    public void OverlayWindow_HasCancelAndCloseMethod()
    {
        // Verify the CancelAndClose method exists
        var method = typeof(OverlayWindow).GetMethod("CancelAndClose");
        method.Should().NotBeNull();
        method?.ReturnType.Should().Be(typeof(void));
    }

    [Fact]
    public void OverlayWindow_HasAnimationsEnabledProperty()
    {
        // Verify the AnimationsEnabled property exists
        var property = typeof(OverlayWindow).GetProperty("AnimationsEnabled");
        property.Should().NotBeNull();
        property?.PropertyType.Should().Be(typeof(bool));
    }
}
