using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScreenshotApp.Core.Infrastructure;
using ScreenshotApp.Core.Models;
using ScreenshotApp.Core.Services.Interfaces;

namespace ScreenshotApp.ViewModels;

/// <summary>
/// Main ViewModel for the application.
/// </summary>
public partial class MainViewModel : ObservableObject, IDisposable
{
    private bool _disposed;
    private readonly IHotkeyService _hotkeyService;
    private readonly ISettingsService _settingsService;
    private readonly IThemeService _themeService;
    private readonly OverlayViewModel _overlayViewModel;

    [ObservableProperty]
    private bool _isDarkMode;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isCapturing;

    [ObservableProperty]
    private string _hotkeyDisplay = "Print Screen";

    /// <summary>
    /// Raised when a capture region is selected and ready for annotation.
    /// </summary>
    public event EventHandler<CaptureRegion>? OnCaptureRegionSelected;

    public MainViewModel()
    {
        // Get services from ServiceLocator
        _hotkeyService = ServiceLocator.Get<IHotkeyService>();
        _settingsService = ServiceLocator.Get<ISettingsService>();
        _themeService = ServiceLocator.Get<IThemeService>();

        // Create overlay ViewModel and preload window for <50ms response
        _overlayViewModel = new OverlayViewModel();
        _overlayViewModel.OverlayCancelled += OnOverlayCancelled;
        _overlayViewModel.RegionSelected += OnRegionSelected;
        _overlayViewModel.PreloadOverlayWindow(); // Pre-create for instant show

        // Subscribe to events
        _hotkeyService.HotkeyPressed += OnHotkeyPressed;
        _hotkeyService.HotkeyConflictDetected += OnHotkeyConflictDetected;
        _themeService.ThemeChanged += OnThemeChanged;

        // Initialize theme
        UpdateTheme();

        // Initialize hotkey display
        UpdateHotkeyDisplay();
    }

    private void OnHotkeyPressed(object? sender, HotkeyPressedEventArgs e)
    {
        // Trigger capture workflow - must be on UI thread
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            StatusMessage = "Hotkey pressed - starting capture...";
            StartCapture();
        });
    }

    private void OnHotkeyConflictDetected(object? sender, HotkeyConflictEventArgs e)
    {
        // Save conflict status to settings
        _settingsService.HotkeyConflictDetected = true;
        _settingsService.HotkeyConflictFallback = e.FallbackHotkey;

        // Update display
        UpdateHotkeyDisplay();

        // Show toast notification
        ShowToastNotification($"{e.RequestedHotkey} in use. Using {e.FallbackHotkey} instead.");
    }

    private void UpdateHotkeyDisplay()
    {
        var currentHotkey = _hotkeyService.CurrentHotkey ?? KeyCombo.Default;
        var conflictDetected = _hotkeyService.ConflictDetected || _settingsService.HotkeyConflictDetected;
        HotkeyDisplay = currentHotkey.ToDisplayString(conflictDetected);
    }

    private void ShowToastNotification(string message)
    {
        // Use the tray service to show notification if available
        try
        {
            var trayService = ServiceLocator.Get<ITrayService>();
            trayService.ShowNotification("Screenshot Tool", message, TrayIconType.Warning);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to show toast notification: {ex}");
        }
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        UpdateTheme();
    }

    private void UpdateTheme()
    {
        IsDarkMode = _themeService.IsSystemDarkMode;
    }

    [RelayCommand]
    private void StartCapture()
    {
        if (IsCapturing) return;

        IsCapturing = true;
        StatusMessage = "Select region to capture...";

        // Show overlay window for region selection (Story 2.2)
        ShowOverlayWindow();
    }

    private void ShowOverlayWindow()
    {
        try
        {
            // Show overlay using the OverlayViewModel
            _overlayViewModel.ShowOverlayCommand.Execute(null);
            StatusMessage = "Select region to capture...";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error showing overlay window: {ex}");
            IsCapturing = false;
            StatusMessage = "Capture failed";
        }
    }

    private void OnOverlayCancelled(object? sender, EventArgs e)
    {
        // Overlay was cancelled (Escape pressed)
        IsCapturing = false;
        StatusMessage = "Capture cancelled";
    }

    private void OnRegionSelected(object? sender, CaptureRegion region)
    {
        // Region was selected on the overlay - transition to annotation canvas (Story 2.3)
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            IsCapturing = false;
            StatusMessage = $"Region selected: {region.Width}×{region.Height}";

            // Raise event for future annotation canvas integration (Story 3.1)
            OnCaptureRegionSelected?.Invoke(this, region);

            // For now, just show a notification with the selection details
            try
            {
                var trayService = ServiceLocator.Get<ITrayService>();
                trayService.ShowNotification("Screenshot Tool",
                    $"Selected region: {region.Width}×{region.Height} pixels",
                    TrayIconType.Info);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to show selection notification: {ex}");
            }
        });
    }

    [RelayCommand]
    private void OpenSettings()
    {
        StatusMessage = "Opening settings...";

        // Show the settings window
        var settingsWindow = new Views.SettingsWindow();
        settingsWindow.ShowDialog();

        StatusMessage = "Ready";
    }

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        try
        {
            StatusMessage = "Checking for updates...";

            var updateService = ServiceLocator.Get<IUpdateService>();
            var result = await updateService.CheckForUpdatesAsync();

            // Show result window
            var updateWindow = new Views.UpdateAvailableWindow(result);
            updateWindow.ShowDialog();

            StatusMessage = result.HasError
                ? "Update check failed"
                : result.IsUpdateAvailable
                    ? "Update available"
                    : "Up to date";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error checking for updates: {ex}");
            StatusMessage = "Update check failed";

            System.Windows.MessageBox.Show(
                "Unable to check for updates. Please try again later.",
                "Screenshot Tool",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _hotkeyService.HotkeyPressed -= OnHotkeyPressed;
            _hotkeyService.HotkeyConflictDetected -= OnHotkeyConflictDetected;
            _themeService.ThemeChanged -= OnThemeChanged;

            _overlayViewModel.OverlayCancelled -= OnOverlayCancelled;
            _overlayViewModel.RegionSelected -= OnRegionSelected;
            _overlayViewModel.Dispose();

            _disposed = true;
        }
    }
}
