using System.Diagnostics;
using Microsoft.Win32;
using ScreenshotApp.Core.Models;
using ScreenshotApp.Core.Services.Interfaces;

namespace ScreenshotApp.Services;

/// <summary>
/// Registry-based implementation of ISettingsService.
/// Settings stored at HKEY_CURRENT_USER\Software\ScreenshotApp
/// </summary>
public class RegistrySettingsService : ISettingsService
{
    private const string RegistryKeyPath = @"Software\ScreenshotApp";
    private readonly Settings _settings;
    private readonly IStartupRegistryService? _startupService;

    /// <summary>
    /// Gets the last error message from auto-start synchronization, if any.
    /// </summary>
    public string? LastAutoStartError { get; private set; }

    public HotkeyConfig CaptureHotkey
    {
        get => _settings.CaptureHotkey;
        set
        {
            if (!Equals(_settings.CaptureHotkey.Modifiers, value.Modifiers) ||
                _settings.CaptureHotkey.Key != value.Key)
            {
                _settings.CaptureHotkey = value;
                OnSettingsChanged();
            }
        }
    }

    public bool AutoStartWithWindows
    {
        get => _settings.AutoStartWithWindows;
        set
        {
            if (_settings.AutoStartWithWindows != value)
            {
                var syncResult = SyncWindowsStartup(value);
                if (syncResult.Success)
                {
                    _settings.AutoStartWithWindows = value;
                    LastAutoStartError = null;
                    OnSettingsChanged();
                }
                else
                {
                    // Sync failed - don't change the setting, preserve the error
                    LastAutoStartError = syncResult.ErrorMessage;
                }
            }
        }
    }

    /// <summary>
    /// Result of a Windows startup synchronization operation.
    /// </summary>
    private readonly struct SyncResult
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }

        public static SyncResult Ok() => new() { Success = true };
        public static SyncResult Fail(string? message) => new() { Success = false, ErrorMessage = message };
    }

    /// <summary>
    /// Synchronizes the Windows startup registry with the application setting.
    /// </summary>
    /// <param name="enable">True to register in startup; false to unregister.</param>
    /// <returns>SyncResult indicating success or failure with error message.</returns>
    private SyncResult SyncWindowsStartup(bool enable)
    {
        if (_startupService == null)
        {
            var msg = "StartupRegistryService not available, skipping Windows startup sync";
            Debug.WriteLine(msg);
            return SyncResult.Fail(msg);
        }

        try
        {
            if (enable)
            {
                var success = _startupService.TryRegisterStartup(out var error);
                if (!success)
                {
                    Debug.WriteLine($"Auto-start registration warning: {error}");
                    return SyncResult.Fail(error);
                }
            }
            else
            {
                var success = _startupService.TryUnregisterStartup(out var error);
                if (!success)
                {
                    Debug.WriteLine($"Auto-start unregistration warning: {error}");
                    return SyncResult.Fail(error);
                }
            }
            return SyncResult.Ok();
        }
        catch (Exception ex)
        {
            // Defensive error handling - never crash on registry operations
            Debug.WriteLine($"Auto-start sync error: {ex}");
            return SyncResult.Fail(ex.Message);
        }
    }

    public AppTheme Theme
    {
        get => _settings.Theme;
        set
        {
            if (_settings.Theme != value)
            {
                _settings.Theme = value;
                OnSettingsChanged();
            }
        }
    }

    public string DefaultSaveLocation
    {
        get => _settings.DefaultSaveLocation;
        set
        {
            if (_settings.DefaultSaveLocation != value)
            {
                _settings.DefaultSaveLocation = value;
                OnSettingsChanged();
            }
        }
    }

    public bool HotkeyConflictDetected
    {
        get => _settings.HotkeyConflictDetected;
        set
        {
            if (_settings.HotkeyConflictDetected != value)
            {
                _settings.HotkeyConflictDetected = value;
                OnSettingsChanged();
            }
        }
    }

    public KeyCombo? HotkeyConflictFallback
    {
        get
        {
            if (_settings.HotkeyConflictFallback == null) return null;
            return new KeyCombo(_settings.HotkeyConflictFallback.Key, _settings.HotkeyConflictFallback.Modifiers);
        }
        set
        {
            if (value.HasValue)
            {
                _settings.HotkeyConflictFallback = new HotkeyConfig(value.Value.Modifiers, value.Value.Key);
            }
            else
            {
                _settings.HotkeyConflictFallback = null;
            }
            OnSettingsChanged();
        }
    }

    public event EventHandler? SettingsChanged;

    public RegistrySettingsService(IStartupRegistryService startupService)
    {
        _settings = new Settings();
        _startupService = startupService;
        Load();
        SyncSettingsWithWindowsStartup();
    }

    /// <summary>
    /// Synchronizes the application setting with the actual Windows startup registry state.
    /// Ensures consistency if registry was modified outside the application.
    /// </summary>
    private void SyncSettingsWithWindowsStartup()
    {
        if (_startupService == null) return;

        try
        {
            var isRegisteredInWindows = _startupService.IsRegistered;
            var settingValue = _settings.AutoStartWithWindows;

            // If setting says enabled but Windows registry says no, update the setting
            if (settingValue && !isRegisteredInWindows)
            {
                _settings.AutoStartWithWindows = false;
                OnSettingsChanged();
            }
            // If setting says disabled but Windows registry says yes, update the setting
            else if (!settingValue && isRegisteredInWindows)
            {
                _settings.AutoStartWithWindows = true;
                OnSettingsChanged();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to sync startup settings: {ex}");
        }
    }

    public void Load()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            if (key != null)
            {
                // Load capture hotkey
                var modifiersValue = key.GetValue("CaptureHotkeyModifiers");
                var keyValue = key.GetValue("CaptureHotkeyKey");
                if (modifiersValue is int mods && keyValue is int k)
                {
                    _settings.CaptureHotkey.Modifiers = (KeyModifiers)mods;
                    _settings.CaptureHotkey.Key = (uint)k;
                }

                // Load other settings
                if (key.GetValue("AutoStartWithWindows") is int autoStart)
                {
                    _settings.AutoStartWithWindows = autoStart != 0;
                }

                if (key.GetValue("Theme") is int theme)
                {
                    _settings.Theme = (AppTheme)theme;
                }

                if (key.GetValue("DefaultSaveLocation") is string saveLocation)
                {
                    _settings.DefaultSaveLocation = saveLocation;
                }

                if (key.GetValue("AutoCopyToClipboard") is int autoCopy)
                {
                    _settings.AutoCopyToClipboard = autoCopy != 0;
                }

                if (key.GetValue("ShowCaptureNotifications") is int showNotifications)
                {
                    _settings.ShowCaptureNotifications = showNotifications != 0;
                }

                if (key.GetValue("HotkeyConflictDetected") is int conflictDetected)
                {
                    _settings.HotkeyConflictDetected = conflictDetected != 0;
                }

                var conflictFallbackModifiers = key.GetValue("HotkeyConflictFallbackModifiers");
                var conflictFallbackKey = key.GetValue("HotkeyConflictFallbackKey");
                if (conflictFallbackModifiers is int cfMods && conflictFallbackKey is int cfKey)
                {
                    _settings.HotkeyConflictFallback = new HotkeyConfig((KeyModifiers)cfMods, (uint)cfKey);
                }
            }
        }
        catch (Exception ex)
        {
            // If loading fails, use defaults
            Debug.WriteLine($"RegistrySettingsService.Load failed: {ex}");
            ResetToDefaults();
        }
    }

    public void Save()
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
            if (key != null)
            {
                key.SetValue("CaptureHotkeyModifiers", (int)_settings.CaptureHotkey.Modifiers, RegistryValueKind.DWord);
                key.SetValue("CaptureHotkeyKey", (int)_settings.CaptureHotkey.Key, RegistryValueKind.DWord);
                key.SetValue("AutoStartWithWindows", _settings.AutoStartWithWindows ? 1 : 0, RegistryValueKind.DWord);
                key.SetValue("Theme", (int)_settings.Theme, RegistryValueKind.DWord);
                key.SetValue("DefaultSaveLocation", _settings.DefaultSaveLocation, RegistryValueKind.String);
                key.SetValue("AutoCopyToClipboard", _settings.AutoCopyToClipboard ? 1 : 0, RegistryValueKind.DWord);
                key.SetValue("ShowCaptureNotifications", _settings.ShowCaptureNotifications ? 1 : 0, RegistryValueKind.DWord);
                key.SetValue("HotkeyConflictDetected", _settings.HotkeyConflictDetected ? 1 : 0, RegistryValueKind.DWord);

                if (_settings.HotkeyConflictFallback != null)
                {
                    key.SetValue("HotkeyConflictFallbackModifiers", (int)_settings.HotkeyConflictFallback.Modifiers, RegistryValueKind.DWord);
                    key.SetValue("HotkeyConflictFallbackKey", (int)_settings.HotkeyConflictFallback.Key, RegistryValueKind.DWord);
                }
            }
        }
        catch (Exception ex)
        {
            // Log if registry is not accessible
            Debug.WriteLine($"RegistrySettingsService.Save failed: {ex}");
        }
    }

    public void ResetToDefaults()
    {
        var defaults = new Settings();
        _settings.CaptureHotkey = defaults.CaptureHotkey;
        _settings.AutoStartWithWindows = defaults.AutoStartWithWindows;
        _settings.Theme = defaults.Theme;
        _settings.DefaultSaveLocation = defaults.DefaultSaveLocation;
        _settings.AutoCopyToClipboard = defaults.AutoCopyToClipboard;
        _settings.ShowCaptureNotifications = defaults.ShowCaptureNotifications;
        OnSettingsChanged();
    }

    private void OnSettingsChanged()
    {
        SettingsChanged?.Invoke(this, EventArgs.Empty);
        Save();
    }
}
