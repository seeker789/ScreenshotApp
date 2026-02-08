using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScreenshotApp.Core.Models;
using ScreenshotApp.Core.Services.Interfaces;
using ScreenshotApp.Views;

namespace ScreenshotApp.ViewModels;

/// <summary>
/// ViewModel for managing the overlay window lifecycle.
/// </summary>
public partial class OverlayViewModel : ObservableObject, IDisposable
{
    private OverlayWindow? _overlayWindow;
    private OverlayWindow? _preCreatedOverlay;
    private bool _disposed;

    /// <summary>
    /// Gets whether the overlay is currently visible.
    /// </summary>
    [ObservableProperty]
    private bool _isOverlayVisible;

    /// <summary>
    /// Raised when the overlay is cancelled by the user.
    /// </summary>
    public event EventHandler? OverlayCancelled;

    /// <summary>
    /// Raised when a region is selected (Story 2.3).
    /// </summary>
    public event EventHandler<CaptureRegion>? RegionSelected;

    /// <summary>
    /// Pre-creates the overlay window hidden for instant show.
    /// Call this during app initialization to achieve <50ms response time.
    /// </summary>
    public void PreloadOverlayWindow()
    {
        if (_disposed) return;

        try
        {
            if (_preCreatedOverlay == null)
            {
                _preCreatedOverlay = new OverlayWindow();
                _preCreatedOverlay.Hide(); // Ensure it's hidden
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to preload overlay: {ex}");
        }
    }

    /// <summary>
    /// Shows the overlay window.
    /// </summary>
    [RelayCommand]
    public void ShowOverlay()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(OverlayViewModel));

        try
        {
            // Close existing overlay if any
            if (_overlayWindow != null)
            {
                _overlayWindow.OverlayCancelled -= OnOverlayCancelled;
                _overlayWindow.OnSelectionComplete -= OnOverlaySelectionComplete;
                _overlayWindow.Close();
                _overlayWindow = null;
            }

            // Use pre-created window for instant show, or create new if none available
            _overlayWindow = _preCreatedOverlay ?? new OverlayWindow();
            _preCreatedOverlay = null; // Will create new after show

            _overlayWindow.OverlayCancelled += OnOverlayCancelled;
            _overlayWindow.OnSelectionComplete += OnOverlaySelectionComplete;
            _overlayWindow.Closed += (s, e) =>
            {
                IsOverlayVisible = false;
                if (_overlayWindow != null)
                {
                    _overlayWindow.OverlayCancelled -= OnOverlayCancelled;
                    _overlayWindow.OnSelectionComplete -= OnOverlaySelectionComplete;
                    _overlayWindow = null;
                }
            };

            _overlayWindow.Show();
            IsOverlayVisible = true;

            // Pre-create next window in background for next time
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(100); // Small delay to not interfere with current show
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => PreloadOverlayWindow());
                }
                catch { /* Ignore background preload errors */ }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to show overlay: {ex}");
            IsOverlayVisible = false;

            // Show tray notification as fallback
            ShowErrorNotification("Capture failed. Please try again.");
        }
    }

    /// <summary>
    /// Cancels and closes the overlay window.
    /// </summary>
    [RelayCommand]
    public void CancelOverlay()
    {
        if (_disposed)
            return;

        _overlayWindow?.CancelAndClose();
    }

    private void OnOverlayCancelled(object? sender, EventArgs e)
    {
        // Handle cancellation - return to idle state
        IsOverlayVisible = false;
        OverlayCancelled?.Invoke(this, EventArgs.Empty);
    }

    private async void OnOverlaySelectionComplete(object? sender, CaptureRegion region)
    {
        // Handle selection completion - trigger transition to annotation canvas
        IsOverlayVisible = false;

        // Close the overlay window with a brief delay for visual feedback
        if (_overlayWindow != null)
        {
            _overlayWindow.OverlayCancelled -= OnOverlayCancelled;
            _overlayWindow.OnSelectionComplete -= OnOverlaySelectionComplete;

            // Store reference to prevent race condition
            var windowToClose = _overlayWindow;
            _overlayWindow = null;

            // Close after a short delay to allow visual feedback
            try
            {
                await Task.Delay(100);
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        windowToClose.Close();
                    }
                    catch (InvalidOperationException)
                    {
                        // Window may already be closing
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during overlay close delay: {ex}");
            }
        }

        // Raise RegionSelected event for MainViewModel to handle
        RegionSelected?.Invoke(this, region);
    }

    private void ShowErrorNotification(string message)
    {
        try
        {
            var trayService = Core.Infrastructure.ServiceLocator.Get<ITrayService>();
            trayService.ShowNotification("Screenshot Tool", message, TrayIconType.Error);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to show error notification: {ex}");
        }
    }

    /// <summary>
    /// Disposes the ViewModel and closes any open overlay window.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            if (_overlayWindow != null)
            {
                _overlayWindow.OverlayCancelled -= OnOverlayCancelled;
                _overlayWindow.OnSelectionComplete -= OnOverlaySelectionComplete;
                _overlayWindow.Close();
                _overlayWindow = null;
            }

            if (_preCreatedOverlay != null)
            {
                _preCreatedOverlay.Close();
                _preCreatedOverlay = null;
            }

            _disposed = true;
        }
    }
}
