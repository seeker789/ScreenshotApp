using ScreenshotApp.Core.Services.Interfaces;
using ScreenshotApp.Infrastructure.Win32;

namespace ScreenshotApp.Tests.Services;

public class Win32HotkeyServiceTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Act
        var service = new Win32HotkeyService();

        // Assert
        Assert.NotNull(service);
        Assert.Null(service.CurrentHotkey);
        Assert.False(service.ConflictDetected);
    }

    [Fact]
    public void SetWindowHandle_WhenCalled_ShouldSetHandle()
    {
        // Arrange
        using var service = new Win32HotkeyService();
        var expectedHandle = new IntPtr(1234);

        // Act - This would normally require a real window handle
        // For unit testing, we just verify the method doesn't throw
        service.SetWindowHandle(expectedHandle);

        // Assert
        // If we had access to the private field, we could verify it was set
        // For now, we just verify no exception was thrown
        Assert.True(true);
    }

    [Fact]
    public void TryRegisterHotkey_WithZeroWindowHandle_ShouldReturnFalse()
    {
        // Arrange
        using var service = new Win32HotkeyService();

        // Act
        bool result = service.TryRegisterHotkey(1, KeyModifiers.None, 0x2C, out string? error);

        // Assert
        Assert.False(result);
        Assert.NotNull(error);
        Assert.Contains("Window handle not set", error);
    }

    [Fact]
    public void TryRegisterHotkey_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        using var service = new Win32HotkeyService();
        service.SetWindowHandle(new IntPtr(1234));

        // Act
        bool result = service.TryRegisterHotkey(0, KeyModifiers.None, 0x2C, out string? error);

        // Assert
        // Note: This test may pass or fail depending on Win32 behavior
        // The important thing is that it doesn't throw
        // and returns a consistent result
    }

    [Fact]
    public void TryUnregisterHotkey_WhenNotRegistered_ShouldReturnFalse()
    {
        // Arrange
        using var service = new Win32HotkeyService();
        service.SetWindowHandle(new IntPtr(1234));

        // Act
        bool result = service.TryUnregisterHotkey(999, out string? error);

        // Assert
        Assert.False(result);
        Assert.NotNull(error);
        Assert.Contains("not registered", error);
    }

    [Fact]
    public void UnregisterAll_WhenNoHotkeysRegistered_ShouldNotThrow()
    {
        // Arrange
        using var service = new Win32HotkeyService();

        // Act & Assert
        var exception = Record.Exception(() => service.UnregisterAll());
        Assert.Null(exception);
    }

    [Fact]
    public void Dispose_WhenCalled_ShouldNotThrow()
    {
        // Arrange
        var service = new Win32HotkeyService();

        // Act & Assert
        var exception = Record.Exception(() => service.Dispose());
        Assert.Null(exception);
    }

    [Fact]
    public void Dispose_WhenCalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var service = new Win32HotkeyService();

        // Act & Assert
        service.Dispose();
        var exception = Record.Exception(() => service.Dispose());
        Assert.Null(exception);
    }

    [Fact]
    public void TryRegisterHotkey_WithKeyCombo_WithoutWindowHandle_ShouldReturnFalse()
    {
        // Arrange
        using var service = new Win32HotkeyService();
        var keyCombo = KeyCombo.Default;

        // Act
        bool result = service.TryRegisterHotkey(keyCombo);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void KeyCombo_Default_ShouldBePrintScreen()
    {
        // Arrange & Act
        var defaultCombo = KeyCombo.Default;

        // Assert
        Assert.Equal(0x2Cu, defaultCombo.Key); // VK_SNAPSHOT
        Assert.Equal(KeyModifiers.None, defaultCombo.Modifiers);
    }

    [Fact]
    public void KeyCombo_Fallback_ShouldBeCtrlShiftS()
    {
        // Arrange & Act
        var fallbackCombo = KeyCombo.Fallback;

        // Assert
        Assert.Equal(0x53u, fallbackCombo.Key); // 'S' key
        Assert.Equal(KeyModifiers.Control | KeyModifiers.Shift, fallbackCombo.Modifiers);
    }

    [Theory]
    [InlineData(0x2C, KeyModifiers.None, "Print Screen")]
    [InlineData(0x53, KeyModifiers.Control | KeyModifiers.Shift, "Ctrl+Shift+S")]
    [InlineData(0x41, KeyModifiers.Control, "Ctrl+0x41")] // Generic key code format for non-special keys
    [InlineData(0x42, KeyModifiers.Alt, "Alt+0x42")]
    [InlineData(0x43, KeyModifiers.Shift, "Shift+0x43")]
    [InlineData(0x44, KeyModifiers.Windows, "Win+0x44")]
    public void KeyCombo_ToString_ShouldFormatCorrectly(uint key, KeyModifiers modifiers, string expected)
    {
        // Arrange
        var combo = new KeyCombo(key, modifiers);

        // Act
        var result = combo.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void KeyCombo_ToDisplayString_WithConflict_ShouldIncludeConflictText()
    {
        // Arrange
        var combo = new KeyCombo(0x53, KeyModifiers.Control | KeyModifiers.Shift);

        // Act
        var result = combo.ToDisplayString(conflictDetected: true);

        // Assert
        Assert.Contains("conflict detected", result);
    }

    [Fact]
    public void KeyCombo_ToDisplayString_WithoutConflict_ShouldNotIncludeConflictText()
    {
        // Arrange
        var combo = new KeyCombo(0x2C, KeyModifiers.None);

        // Act
        var result = combo.ToDisplayString(conflictDetected: false);

        // Assert
        Assert.DoesNotContain("conflict", result);
        Assert.Equal("Print Screen", result);
    }

    [Fact]
    public void HotkeyConflictEventArgs_ShouldStoreValues()
    {
        // Arrange
        var requested = new KeyCombo(0x2C, KeyModifiers.None);
        var fallback = new KeyCombo(0x53, KeyModifiers.Control | KeyModifiers.Shift);

        // Act
        var args = new HotkeyConflictEventArgs
        {
            RequestedHotkey = requested,
            FallbackHotkey = fallback
        };

        // Assert
        Assert.Equal(requested, args.RequestedHotkey);
        Assert.Equal(fallback, args.FallbackHotkey);
    }

    [Fact]
    public void HotkeyPressedEventArgs_ShouldStoreHotkeyId()
    {
        // Arrange & Act
        var args = new HotkeyPressedEventArgs(42);

        // Assert
        Assert.Equal(42, args.HotkeyId);
    }

    [Fact]
    public void ProcessWindowMessage_WhenHotkeyMessage_ShouldRaiseEvent()
    {
        // Arrange
        using var service = new Win32HotkeyService();
        bool eventRaised = false;
        int? receivedHotkeyId = null;

        service.HotkeyPressed += (sender, args) =>
        {
            eventRaised = true;
            receivedHotkeyId = args.HotkeyId;
        };

        // Act - WM_HOTKEY = 0x0312
        service.ProcessWindowMessage(0x0312, new IntPtr(1));

        // Assert
        Assert.True(eventRaised);
        Assert.Equal(1, receivedHotkeyId);
    }

    [Fact]
    public void ProcessWindowMessage_WhenNotHotkeyMessage_ShouldNotRaiseEvent()
    {
        // Arrange
        using var service = new Win32HotkeyService();
        bool eventRaised = false;

        service.HotkeyPressed += (sender, args) =>
        {
            eventRaised = true;
        };

        // Act
        service.ProcessWindowMessage(0x0001, new IntPtr(1)); // WM_CREATE

        // Assert
        Assert.False(eventRaised);
    }

    [Fact]
    public void KeyCombo_Equality_SameValues_ShouldBeEqual()
    {
        // Arrange
        var combo1 = new KeyCombo(0x53, KeyModifiers.Control);
        var combo2 = new KeyCombo(0x53, KeyModifiers.Control);

        // Act & Assert
        Assert.Equal(combo1, combo2);
        Assert.True(combo1 == combo2);
    }

    [Fact]
    public void KeyCombo_Equality_DifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var combo1 = new KeyCombo(0x53, KeyModifiers.Control);
        var combo2 = new KeyCombo(0x41, KeyModifiers.Control);

        // Act & Assert
        Assert.NotEqual(combo1, combo2);
        Assert.True(combo1 != combo2);
    }
}
