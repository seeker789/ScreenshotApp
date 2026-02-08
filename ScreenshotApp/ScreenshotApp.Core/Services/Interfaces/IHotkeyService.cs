namespace ScreenshotApp.Core.Services.Interfaces;

/// <summary>
/// Service for registering and managing global hotkeys.
/// </summary>
public interface IHotkeyService
{
    /// <summary>
    /// Occurs when a registered hotkey is pressed.
    /// </summary>
    event EventHandler<HotkeyPressedEventArgs>? HotkeyPressed;

    /// <summary>
    /// Occurs when a hotkey conflict is detected and fallback is used.
    /// </summary>
    event EventHandler<HotkeyConflictEventArgs>? HotkeyConflictDetected;

    /// <summary>
    /// Gets the currently registered hotkey configuration.
    /// </summary>
    KeyCombo? CurrentHotkey { get; }

    /// <summary>
    /// Gets whether a hotkey conflict was detected during registration.
    /// </summary>
    bool ConflictDetected { get; }

    /// <summary>
    /// Attempts to register a global hotkey.
    /// </summary>
    /// <param name="id">Unique identifier for the hotkey.</param>
    /// <param name="modifiers">Key modifiers (Ctrl, Alt, Shift, Win).</param>
    /// <param name="key">The virtual key code.</param>
    /// <param name="error">Error message if registration fails.</param>
    /// <returns>True if registration succeeded; otherwise, false.</returns>
    bool TryRegisterHotkey(int id, KeyModifiers modifiers, uint key, out string? error);

    /// <summary>
    /// Attempts to register a global hotkey using a KeyCombo.
    /// Automatically falls back to Ctrl+Shift+S on conflict.
    /// </summary>
    /// <param name="keyCombo">The key combination to register.</param>
    /// <returns>True if registration succeeded (either primary or fallback); otherwise, false.</returns>
    bool TryRegisterHotkey(KeyCombo keyCombo);

    /// <summary>
    /// Attempts to unregister a global hotkey.
    /// </summary>
    /// <param name="id">The hotkey identifier to unregister.</param>
    /// <param name="error">Error message if unregistration fails.</param>
    /// <returns>True if unregistration succeeded; otherwise, false.</returns>
    bool TryUnregisterHotkey(int id, out string? error);

    /// <summary>
    /// Unregisters all hotkeys.
    /// </summary>
    void UnregisterAll();
}

/// <summary>
/// Event arguments for hotkey pressed events.
/// </summary>
public class HotkeyPressedEventArgs : EventArgs
{
    public int HotkeyId { get; }

    public HotkeyPressedEventArgs(int hotkeyId)
    {
        HotkeyId = hotkeyId;
    }
}

/// <summary>
/// Event arguments for hotkey conflict detection.
/// </summary>
public class HotkeyConflictEventArgs : EventArgs
{
    /// <summary>
    /// The hotkey that was requested but conflicted.
    /// </summary>
    public KeyCombo RequestedHotkey { get; set; } = new();

    /// <summary>
    /// The fallback hotkey that was registered instead.
    /// </summary>
    public KeyCombo FallbackHotkey { get; set; } = new();
}

/// <summary>
/// Key modifier flags for hotkey registration.
/// </summary>
[Flags]
public enum KeyModifiers
{
    None = 0,
    Alt = 1,
    Control = 2,
    Shift = 4,
    Windows = 8
}

/// <summary>
/// Represents a key combination (key + modifiers) for hotkey registration.
/// </summary>
public readonly record struct KeyCombo
{
    /// <summary>
    /// The virtual key code.
    /// </summary>
    public uint Key { get; init; }

    /// <summary>
    /// The modifier keys.
    /// </summary>
    public KeyModifiers Modifiers { get; init; }

    public KeyCombo()
    {
        Key = 0;
        Modifiers = KeyModifiers.None;
    }

    public KeyCombo(uint key, KeyModifiers modifiers)
    {
        Key = key;
        Modifiers = modifiers;
    }

    /// <summary>
    /// Default hotkey: Print Screen (no modifiers).
    /// </summary>
    public static KeyCombo Default => new(0x2C, KeyModifiers.None); // VK_SNAPSHOT

    /// <summary>
    /// Fallback hotkey: Ctrl+Shift+S.
    /// </summary>
    public static KeyCombo Fallback => new(0x53, KeyModifiers.Control | KeyModifiers.Shift); // 'S' key

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        if (Modifiers.HasFlag(KeyModifiers.Control)) sb.Append("Ctrl+");
        if (Modifiers.HasFlag(KeyModifiers.Shift)) sb.Append("Shift+");
        if (Modifiers.HasFlag(KeyModifiers.Alt)) sb.Append("Alt+");
        if (Modifiers.HasFlag(KeyModifiers.Windows)) sb.Append("Win+");

        // Convert virtual key code to readable name
        var keyName = Key switch
        {
            0x2C => "Print Screen",
            0x53 => "S",
            _ => $"0x{Key:X}"
        };
        sb.Append(keyName);
        return sb.ToString();
    }

    /// <summary>
    /// Returns a string suitable for display in settings UI, optionally showing conflict status.
    /// </summary>
    public string ToDisplayString(bool conflictDetected = false)
    {
        return conflictDetected ? $"{this} (conflict detected)" : ToString();
    }
}
