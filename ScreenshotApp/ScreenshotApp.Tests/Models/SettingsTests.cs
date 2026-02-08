using FluentAssertions;
using ScreenshotApp.Core.Models;
using ScreenshotApp.Core.Services.Interfaces;

namespace ScreenshotApp.Tests.Models;

public class SettingsTests
{
    [Fact]
    public void Constructor_ShouldInitializeDefaults()
    {
        // Act
        var settings = new Settings();

        // Assert
        settings.CaptureHotkey.Modifiers.Should().Be(KeyModifiers.Control | KeyModifiers.Shift);
        settings.CaptureHotkey.Key.Should().Be(0x53); // 'S'
        settings.AutoStartWithWindows.Should().BeFalse();
        settings.Theme.Should().Be(AppTheme.System);
        settings.AutoCopyToClipboard.Should().BeTrue();
        settings.ShowCaptureNotifications.Should().BeTrue();
        settings.DefaultSaveLocation.Should().Contain("Screenshots");
    }

    [Fact]
    public void Clone_ShouldCreateIndependentCopy()
    {
        // Arrange
        var original = new Settings
        {
            CaptureHotkey = new HotkeyConfig(KeyModifiers.Alt, 0x41),
            AutoStartWithWindows = true,
            Theme = AppTheme.Dark,
            DefaultSaveLocation = @"C:\Custom\Path"
        };

        // Act
        var clone = original.Clone();

        // Modify original
        original.CaptureHotkey.Modifiers = KeyModifiers.Control;
        original.Theme = AppTheme.Light;

        // Assert - clone should not be affected
        clone.CaptureHotkey.Modifiers.Should().Be(KeyModifiers.Alt);
        clone.Theme.Should().Be(AppTheme.Dark);
        clone.AutoStartWithWindows.Should().BeTrue();
        clone.DefaultSaveLocation.Should().Be(@"C:\Custom\Path");
    }

    [Fact]
    public void HotkeyConfig_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var config = new HotkeyConfig();

        // Assert
        config.Modifiers.Should().Be(KeyModifiers.Control | KeyModifiers.Shift);
        config.Key.Should().Be(0x53); // 'S'
    }

    [Fact]
    public void HotkeyConfig_ParameterizedConstructor_ShouldSetValues()
    {
        // Act
        var config = new HotkeyConfig(KeyModifiers.Alt | KeyModifiers.Shift, 0x50); // 'P'

        // Assert
        config.Modifiers.Should().Be(KeyModifiers.Alt | KeyModifiers.Shift);
        config.Key.Should().Be(0x50);
    }
}
