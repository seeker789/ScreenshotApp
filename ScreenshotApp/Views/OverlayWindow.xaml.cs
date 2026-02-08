using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ScreenshotApp.Core.Models;

namespace ScreenshotApp.Views;

/// <summary>
/// A borderless, full-screen overlay window for screenshot capture.
/// Displays a dimmed backdrop with a crosshair cursor and region selection.
/// </summary>
public partial class OverlayWindow : Window
{
    private readonly bool _animationsEnabled;
    private bool _isClosing;

    // Selection state
    private System.Windows.Point _selectionStart;
    private bool _isSelecting;
    private bool _isSelectionComplete;
    private const int MIN_SELECTION_SIZE = 10;

    /// <summary>
    /// Raised when the overlay is cancelled (Escape pressed).
    /// </summary>
    public event EventHandler? OverlayCancelled;

    /// <summary>
    /// Raised when a region is selected (mouse released with valid selection).
    /// </summary>
    public event EventHandler<CaptureRegion>? OnSelectionComplete;

    /// <summary>
    /// Initializes a new instance of the OverlayWindow.
    /// </summary>
    public OverlayWindow()
    {
        InitializeComponent();

        // Check Windows reduced motion setting
        _animationsEnabled = SystemParameters.ClientAreaAnimation;

        // Position to cover all screens
        PositionToVirtualScreen();

        // Set up event handlers
        Loaded += OnLoaded;
        MouseDown += OnMouseDown;
        MouseMove += OnMouseMove;
        MouseUp += OnMouseUp;
        PreviewKeyDown += OnPreviewKeyDown;
        Closed += OnClosed;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Reset closing flag for window reuse
        _isClosing = false;
        _isSelectionComplete = false;

        // Activate and focus to capture keyboard input
        Activate();
        Focus();

        // Start fade-in animation
        if (_animationsEnabled)
        {
            var fadeIn = new DoubleAnimation(0, 0.85, TimeSpan.FromMilliseconds(150))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            fadeIn.Completed += (s, ev) =>
            {
                // After fade-in, ensure backdrop is at full opacity
                Backdrop.Opacity = 0.85;
            };
            Backdrop.BeginAnimation(OpacityProperty, fadeIn);
            AnimateDimmedStrips(0, 0.85);
        }
        else
        {
            // Instant show for reduced motion
            Backdrop.Opacity = 0.85;
            SetDimmedStripsOpacity(0.85);
        }
    }

    private void PositionToVirtualScreen()
    {
        // Multi-monitor support: cover virtual screen bounds
        // Primary monitor might not be at (0,0), handle negative coordinates
        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && !_isSelectionComplete)
        {
            try
            {
                _isSelecting = true;
                _selectionStart = e.GetPosition(this);

                // Initialize selection rectangle at start point
                Canvas.SetLeft(SelectionRectangle, _selectionStart.X);
                Canvas.SetTop(SelectionRectangle, _selectionStart.Y);
                SelectionRectangle.Width = 0;
                SelectionRectangle.Height = 0;
                SelectionRectangle.Visibility = Visibility.Visible;

                // Show dimensions tooltip with "0 × 0"
                UpdateDimensionsTooltip(0, 0);
                PositionDimensionsTooltip(_selectionStart);
                DimensionsTooltip.Visibility = Visibility.Visible;

                // Hide full backdrop, show dimmed strips for inverted dimming
                Backdrop.Visibility = Visibility.Collapsed;
                UpdateInvertedDimming(_selectionStart.X, _selectionStart.Y, 0, 0);
                ShowDimmedStrips();

                CaptureMouse();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MouseDown error: {ex}");
                ResetSelectionState();
            }
        }
    }

    private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        try
        {
            var pos = e.GetPosition(this);

            // Always update crosshair position
            Canvas.SetLeft(CrosshairCanvas, pos.X - 10);
            Canvas.SetTop(CrosshairCanvas, pos.Y - 10);

            if (!_isSelecting) return;

            // Calculate selection rectangle
            double x = Math.Min(_selectionStart.X, pos.X);
            double y = Math.Min(_selectionStart.Y, pos.Y);
            double width = Math.Abs(pos.X - _selectionStart.X);
            double height = Math.Abs(pos.Y - _selectionStart.Y);

            // Check for Shift key constraint (square)
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                double size = Math.Min(width, height);
                width = size;
                height = size;

                // Adjust x/y based on drag direction to maintain anchor
                if (pos.X < _selectionStart.X) x = _selectionStart.X - size;
                if (pos.Y < _selectionStart.Y) y = _selectionStart.Y - size;
            }

            // Clamp to virtual screen bounds
            double virtualScreenRight = SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth;
            double virtualScreenBottom = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight;

            x = Math.Max(SystemParameters.VirtualScreenLeft, Math.Min(x, virtualScreenRight));
            y = Math.Max(SystemParameters.VirtualScreenTop, Math.Min(y, virtualScreenBottom));
            width = Math.Min(width, virtualScreenRight - x);
            height = Math.Min(height, virtualScreenBottom - y);

            // Update selection rectangle
            Canvas.SetLeft(SelectionRectangle, x);
            Canvas.SetTop(SelectionRectangle, y);
            SelectionRectangle.Width = width;
            SelectionRectangle.Height = height;

            // Update inverted dimming (hole in backdrop)
            UpdateInvertedDimming(x, y, width, height);

            // Update dimensions tooltip
            UpdateDimensionsTooltip((int)width, (int)height);
            PositionDimensionsTooltip(new System.Windows.Point(x, y));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MouseMove error: {ex}");
            // Don't reset - try to continue
        }
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isSelecting) return;

        try
        {
            _isSelecting = false;
            ReleaseMouseCapture();

            var width = SelectionRectangle.Width;
            var height = SelectionRectangle.Height;

            // Validate minimum size
            if (width < MIN_SELECTION_SIZE || height < MIN_SELECTION_SIZE)
            {
                ShowMinimumSizeWarning();
                // Reset selection but keep overlay active
                ResetSelectionState();
                return;
            }

            // Valid selection - capture coordinates and complete
            var x = Canvas.GetLeft(SelectionRectangle);
            var y = Canvas.GetTop(SelectionRectangle);

            // Ensure positive coordinates relative to virtual screen
            int captureX = (int)(x - SystemParameters.VirtualScreenLeft);
            int captureY = (int)(y - SystemParameters.VirtualScreenTop);

            var region = new CaptureRegion
            {
                X = captureX,
                Y = captureY,
                Width = (int)width,
                Height = (int)height
            };

            _isSelectionComplete = true;

            // Hide crosshair during transition
            CrosshairCanvas.Visibility = Visibility.Collapsed;

            // Raise selection complete event
            OnSelectionComplete?.Invoke(this, region);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MouseUp error: {ex}");
            ResetSelectionState();
        }
    }

    private void UpdateInvertedDimming(double x, double y, double width, double height)
    {
        // Update the 4 dimmed strips to create a "hole" around the selection
        // Top strip: from top of screen to selection top
        DimmedTop.Height = Math.Max(0, y - SystemParameters.VirtualScreenTop);

        // Bottom strip: from selection bottom to bottom of screen
        double screenBottom = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight;
        double selectionBottom = y + height;
        DimmedBottom.Height = Math.Max(0, screenBottom - selectionBottom);

        // Left strip: from left of screen to selection left, full height
        DimmedLeft.Width = Math.Max(0, x - SystemParameters.VirtualScreenLeft);
        // Left strip needs to fit between top and bottom strips
        Canvas.SetTop(DimmedLeft, y);
        DimmedLeft.Height = height;

        // Right strip: from selection right to right of screen
        double screenRight = SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth;
        double selectionRight = x + width;
        DimmedRight.Width = Math.Max(0, screenRight - selectionRight);
        // Right strip needs to fit between top and bottom strips
        Canvas.SetTop(DimmedRight, y);
        DimmedRight.Height = height;
    }

    private void ShowDimmedStrips()
    {
        DimmedTop.Visibility = Visibility.Visible;
        DimmedBottom.Visibility = Visibility.Visible;
        DimmedLeft.Visibility = Visibility.Visible;
        DimmedRight.Visibility = Visibility.Visible;
    }

    private void HideDimmedStrips()
    {
        DimmedTop.Visibility = Visibility.Collapsed;
        DimmedBottom.Visibility = Visibility.Collapsed;
        DimmedLeft.Visibility = Visibility.Collapsed;
        DimmedRight.Visibility = Visibility.Collapsed;
    }

    private void SetDimmedStripsOpacity(double opacity)
    {
        DimmedTop.Opacity = opacity;
        DimmedBottom.Opacity = opacity;
        DimmedLeft.Opacity = opacity;
        DimmedRight.Opacity = opacity;
    }

    private void AnimateDimmedStrips(double fromOpacity, double toOpacity)
    {
        var animation = new DoubleAnimation(fromOpacity, toOpacity, TimeSpan.FromMilliseconds(150))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        DimmedTop.BeginAnimation(OpacityProperty, animation);
        DimmedBottom.BeginAnimation(OpacityProperty, animation);
        DimmedLeft.BeginAnimation(OpacityProperty, animation);
        DimmedRight.BeginAnimation(OpacityProperty, animation);
    }

    private void UpdateDimensionsTooltip(int width, int height)
    {
        DimensionsText.Text = $"{width} × {height}";
    }

    private void PositionDimensionsTooltip(System.Windows.Point selectionTopLeft)
    {
        // Position 8px above selection
        var tooltipX = selectionTopLeft.X;
        var tooltipY = selectionTopLeft.Y - DimensionsTooltip.ActualHeight - 8;

        // Ensure tooltip stays on screen
        if (tooltipY < SystemParameters.VirtualScreenTop)
            tooltipY = selectionTopLeft.Y + 8;

        // Ensure tooltip doesn't go off right edge
        if (tooltipX + DimensionsTooltip.ActualWidth > SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth)
            tooltipX = SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - DimensionsTooltip.ActualWidth - 8;

        Canvas.SetLeft(DimensionsTooltip, tooltipX);
        Canvas.SetTop(DimensionsTooltip, tooltipY);
    }

    private void ShowMinimumSizeWarning()
    {
        // Brief tooltip showing "Selection too small"
        DimensionsText.Text = "Selection too small";

        // Reset after 1 second
        Task.Delay(1000).ContinueWith(_ =>
        {
            Dispatcher.Invoke(() =>
            {
                if (!_isSelecting)
                {
                    DimensionsText.Text = "0 × 0";
                }
            });
        });
    }

    private void ResetSelectionState()
    {
        _isSelecting = false;
        SelectionRectangle.Visibility = Visibility.Collapsed;
        DimensionsTooltip.Visibility = Visibility.Collapsed;
        HideDimmedStrips();
        Backdrop.Visibility = Visibility.Visible;
        DimensionsText.Text = "0 × 0";
    }

    private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;

            if (_isSelecting)
            {
                // Cancel current selection but keep overlay open
                ReleaseMouseCapture();
                ResetSelectionState();
            }
            else
            {
                // Close overlay
                CancelAndClose();
            }
        }
    }

    /// <summary>
    /// Cancels the overlay and closes it with a fade-out animation.
    /// </summary>
    public void CancelAndClose()
    {
        if (_isClosing)
            return;

        _isClosing = true;

        // Notify cancellation before closing
        OverlayCancelled?.Invoke(this, EventArgs.Empty);

        if (_animationsEnabled)
        {
            // Fade out animation
            var fadeOut = new DoubleAnimation(0.85, 0, TimeSpan.FromMilliseconds(150))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            fadeOut.Completed += (s, e) =>
            {
                try
                {
                    Close();
                }
                catch (InvalidOperationException)
                {
                    // Window may already be closing
                }
            };
            Backdrop.BeginAnimation(OpacityProperty, fadeOut);
            AnimateDimmedStrips(0.85, 0);
        }
        else
        {
            // Instant close for reduced motion
            Close();
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        // Clean up event handlers
        Loaded -= OnLoaded;
        MouseDown -= OnMouseDown;
        MouseMove -= OnMouseMove;
        MouseUp -= OnMouseUp;
        PreviewKeyDown -= OnPreviewKeyDown;
        Closed -= OnClosed;
    }

    /// <summary>
    /// Gets whether animations are enabled based on system settings.
    /// </summary>
    public bool AnimationsEnabled => _animationsEnabled;
}
