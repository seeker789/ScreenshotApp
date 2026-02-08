using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using ScreenshotApp.Core.Services.Interfaces;

namespace ScreenshotApp.Services;

/// <summary>
/// Manages Windows startup registry entries for the application.
/// Uses HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
/// </summary>
public class StartupRegistryService : IStartupRegistryService
{
    private const string StartupKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "ScreenshotApp";
    private const string AutoStartArgument = "--auto-started";

    /// <inheritdoc/>
    public bool IsRegistered
    {
        get
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(StartupKeyPath);
                var value = key?.GetValue(AppName);
                return value != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to check startup registration: {ex}");
                return false;
            }
        }
    }

    /// <inheritdoc/>
    public bool TryRegisterStartup(out string? error)
    {
        try
        {
            var exePath = GetExecutablePath();
            if (string.IsNullOrEmpty(exePath))
            {
                error = "Could not determine application executable path.";
                return false;
            }

            // Validate executable path exists
            if (!File.Exists(exePath))
            {
                error = $"Executable path does not exist: {exePath}";
                return false;
            }

            // Add --auto-started argument to detect silent startup
            var startupValue = $"\"{exePath}\" {AutoStartArgument}";

            using var key = Registry.CurrentUser.OpenSubKey(StartupKeyPath, writable: true);
            if (key == null)
            {
                error = "Could not open Windows startup registry key.";
                return false;
            }

            key.SetValue(AppName, startupValue, RegistryValueKind.String);
            error = null;
            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            error = "Permission denied. Unable to write to registry.";
            Debug.WriteLine($"Startup registration failed (permission): {ex}");
            return false;
        }
        catch (Exception ex)
        {
            error = $"Failed to register startup: {ex.Message}";
            Debug.WriteLine($"Startup registration failed: {ex}");
            return false;
        }
    }

    /// <inheritdoc/>
    public bool TryUnregisterStartup(out string? error)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(StartupKeyPath, writable: true);
            if (key == null)
            {
                error = "Could not open Windows startup registry key.";
                return false;
            }

            // Check if value exists before trying to delete
            if (key.GetValue(AppName) == null)
            {
                error = null; // Already not registered, which is success
                return true;
            }

            key.DeleteValue(AppName, throwOnMissingValue: false);
            error = null;
            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            error = "Permission denied. Unable to modify registry.";
            Debug.WriteLine($"Startup unregistration failed (permission): {ex}");
            return false;
        }
        catch (Exception ex)
        {
            error = $"Failed to unregister startup: {ex.Message}";
            Debug.WriteLine($"Startup unregistration failed: {ex}");
            return false;
        }
    }

    /// <summary>
    /// Gets the path to the application executable.
    /// </summary>
    /// <returns>The full path to the executable, or null if it cannot be determined.</returns>
    private static string? GetExecutablePath()
    {
        try
        {
            // Try to get the main module file name
            var mainModule = Process.GetCurrentProcess().MainModule;
            if (!string.IsNullOrEmpty(mainModule?.FileName))
            {
                return mainModule.FileName;
            }
        }
        catch
        {
            // MainModule can throw on some platforms, fall through
        }

        try
        {
            // Fallback to assembly location
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.Location;
        }
        catch
        {
            return null;
        }
    }
}
