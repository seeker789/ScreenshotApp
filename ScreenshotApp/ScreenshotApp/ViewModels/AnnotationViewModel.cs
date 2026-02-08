using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScreenshotApp.Core.Models;
using ScreenshotApp.Views;

namespace ScreenshotApp.ViewModels;

/// <summary>
/// ViewModel for managing the annotation canvas and captured image display.
/// </summary>
public partial class AnnotationViewModel : ObservableObject, IDisposable
{
    private AnnotationWindow? _annotationWindow;
    private bool _disposed;

    /// <summary>
    /// Gets or sets the captured image displayed in the annotation canvas.
    /// </summary>
    [ObservableProperty]
    private BitmapImage? _capturedImage;

    /// <summary>
    /// Gets or sets the region that was captured.
    /// </summary>
    [ObservableProperty]
    private CaptureRegion _captureRegion;

    /// <summary>
    /// Gets or sets whether the annotation window is currently visible.
    /// </summary>
    [ObservableProperty]
    private bool _isAnnotationWindowVisible;

    /// <summary>
    /// Gets or sets the toolbar left position for positioning at selection corner.
    /// </summary>
    [ObservableProperty]
    private double _toolbarLeft;

    /// <summary>
    /// Gets or sets the toolbar top position for positioning at selection corner.
    /// </summary>
    [ObservableProperty]
    private double _toolbarTop;

    /// <summary>
    /// Gets or sets whether the toolbar should flip to left side (when near right edge).
    /// </summary>
    [ObservableProperty]
    private bool _toolbarFlipLeft;

    /// <summary>
    /// Gets or sets whether the toolbar should flip to top side (when near bottom edge).
    /// </summary>
    [ObservableProperty]
    private bool _toolbarFlipTop;

    /// <summary>
    /// Raised when the annotation window is closed.
    /// </summary>
    public event EventHandler? AnnotationWindowClosed;

    /// <summary>
    /// Shows the annotation canvas with the captured image.
    /// </summary>
    /// <param name="result">The capture result containing the image data and region info.</param>
    public void ShowAnnotationCanvas(CaptureResult result)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(AnnotationViewModel));

        ArgumentNullException.ThrowIfNull(result);

        try
        {
            // Convert byte array to BitmapImage for WPF display
            CapturedImage = ConvertToBitmapImage(result.ImageData);
            CaptureRegion = result.Region;

            // Calculate toolbar position at selection corner
            CalculateToolbarPosition(result.Region);

            // Create or reuse annotation window (single instance pattern)
            if (_annotationWindow == null)
            {
                _annotationWindow = new AnnotationWindow();
                _annotationWindow.DataContext = this;
                _annotationWindow.Closed += OnAnnotationWindowClosed;
                _annotationWindow.KeyDown += OnAnnotationWindowKeyDown;
            }

            // Show the annotation window
            _annotationWindow.Show();
            _annotationWindow.Activate();
            IsAnnotationWindowVisible = true;

            Debug.WriteLine($"AnnotationWindow shown for region: {result.Region}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error showing annotation canvas: {ex}");
            IsAnnotationWindowVisible = false;
            throw;
        }
    }

    /// <summary>
    /// Closes the annotation window if it's open.
    /// </summary>
    [RelayCommand]
    public void CloseAnnotationWindow()
    {
        if (_annotationWindow != null)
        {
            _annotationWindow.Close();
            // Window.Closed event handler will clean up the reference
        }
    }

    /// <summary>
    /// Calculates the toolbar position at the selection corner with edge-flip logic.
    /// Uses actual dimensions from FloatingToolbar for accurate positioning.
    /// </summary>
    private void CalculateToolbarPosition(CaptureRegion region)
    {
        const double MARGIN = 12;
        const double FLIP_THRESHOLD = 100;
        // Use actual dimensions from FloatingToolbar class to match XAML
        const double TOOLBAR_WIDTH = FloatingToolbar.ActualToolbarWidth;
        const double TOOLBAR_HEIGHT = FloatingToolbar.ActualToolbarHeight;

        // Default: bottom-right of selection
        double left = region.X + region.Width + MARGIN;
        double top = region.Y + region.Height + MARGIN;

        // Check if near right edge - flip to left
        if (left + TOOLBAR_WIDTH > SystemParameters.VirtualScreenWidth - FLIP_THRESHOLD)
        {
            left = region.X - TOOLBAR_WIDTH - MARGIN;
            ToolbarFlipLeft = true;
        }
        else
        {
            ToolbarFlipLeft = false;
        }

        // Check if near bottom edge - flip to top
        if (top + TOOLBAR_HEIGHT > SystemParameters.VirtualScreenHeight - FLIP_THRESHOLD)
        {
            top = region.Y - TOOLBAR_HEIGHT - MARGIN;
            ToolbarFlipTop = true;
        }
        else
        {
            ToolbarFlipTop = false;
        }

        // Ensure toolbar stays within virtual screen bounds
        left = Math.Max(SystemParameters.VirtualScreenLeft, Math.Min(left,
            SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - TOOLBAR_WIDTH));
        top = Math.Max(SystemParameters.VirtualScreenTop, Math.Min(top,
            SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - TOOLBAR_HEIGHT));

        ToolbarLeft = left;
        ToolbarTop = top;

        Debug.WriteLine($"Toolbar positioned at ({ToolbarLeft}, {ToolbarTop}), FlipLeft={ToolbarFlipLeft}, FlipTop={ToolbarFlipTop}");
    }

    /// <summary>
    /// Converts PNG byte array to BitmapImage for WPF display.
    /// </summary>
    private BitmapImage ConvertToBitmapImage(byte[] imageData)
    {
        using var ms = new MemoryStream(imageData);
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.StreamSource = ms;
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze(); // Important for cross-thread access and memory management
        return bitmap;
    }

    private void OnAnnotationWindowClosed(object? sender, EventArgs e)
    {
        IsAnnotationWindowVisible = false;

        // Clean up the window reference and unsubscribe all events
        if (_annotationWindow != null)
        {
            _annotationWindow.Closed -= OnAnnotationWindowClosed;
            _annotationWindow.KeyDown -= OnAnnotationWindowKeyDown;
            _annotationWindow = null;
        }

        // Clear the captured image to free memory
        CapturedImage = null;

        AnnotationWindowClosed?.Invoke(this, EventArgs.Empty);
    }

    private void OnAnnotationWindowKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Escape)
        {
            CloseAnnotationWindow();
            e.Handled = true;
        }
    }

    /// <summary>
    /// Disposes the ViewModel and closes any open annotation window.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            if (_annotationWindow != null)
            {
                _annotationWindow.Closed -= OnAnnotationWindowClosed;
                _annotationWindow.KeyDown -= OnAnnotationWindowKeyDown;
                _annotationWindow.Close();
                _annotationWindow = null;
            }

            CapturedImage = null;
            _disposed = true;
        }
    }
}
