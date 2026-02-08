using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ScreenshotApp.ViewModels;

namespace ScreenshotApp.Views;

/// <summary>
/// Full-screen annotation window for displaying and editing captured screenshots.
/// </summary>
public partial class AnnotationWindow : Window
{
    private bool _isClosing;

    public AnnotationWindow()
    {
        InitializeComponent();

        // Ensure window is positioned on the virtual screen (all monitors)
        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Toolbar positioning is now handled via Canvas.Left/Top bindings in XAML
            // based on ViewModel.ToolbarLeft and ToolbarTop values

            // Fade out instructions after 2 seconds
            FadeOutInstructions();

            // Subscribe to toolbar events
            if (Toolbar != null)
            {
                Toolbar.ToolbarClosed += OnToolbarClosed;
                Toolbar.ColorSelected += OnColorSelected;
                Toolbar.PenToolActivated += OnPenToolActivated;
            }

            // Ensure window has focus for keyboard input
            Activate();
            Focus();

            Debug.WriteLine("AnnotationWindow loaded and focused");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in AnnotationWindow.OnWindowLoaded: {ex}");
        }
    }

    private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_isClosing) return;
        _isClosing = true;

        try
        {
            // Unsubscribe from toolbar events
            if (Toolbar != null)
            {
                Toolbar.ToolbarClosed -= OnToolbarClosed;
                Toolbar.ColorSelected -= OnColorSelected;
                Toolbar.PenToolActivated -= OnPenToolActivated;
            }

            // Apply fade-out animation before closing (150ms as per AC)
            e.Cancel = true; // Cancel immediate close
            PlayFadeOutAndClose();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in AnnotationWindow.OnWindowClosing: {ex}");
        }
    }

    /// <summary>
    /// Plays the 150ms fade-out animation and then closes the window.
    /// Respects Windows reduced motion settings.
    /// </summary>
    private void PlayFadeOutAndClose()
    {
        // Check if animations are enabled (respect Windows accessibility settings)
        // Use ClientAreaAnimation as proxy for animation preferences
        bool animationsEnabled = SystemParameters.ClientAreaAnimation && !SystemParameters.HighContrast;

        if (!animationsEnabled)
        {
            // Skip animation for accessibility
            CompleteClose();
            return;
        }

        try
        {
            var fadeOutAnimation = new DoubleAnimation
            {
                From = Opacity,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            fadeOutAnimation.Completed += (s, e) => CompleteClose();
            BeginAnimation(OpacityProperty, fadeOutAnimation);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error during fade-out animation: {ex}");
            CompleteClose();
        }
    }

    /// <summary>
    /// Completes the window close after animation.
    /// </summary>
    private void CompleteClose()
    {
        // Remove the closing handler to prevent recursion
        Closing -= OnWindowClosing;
        Close();
    }

    /// <summary>
    /// Fades out the instructions text after a delay.
    /// </summary>
    private void FadeOutInstructions()
    {
        try
        {
            var fadeOutAnimation = new DoubleAnimation
            {
                From = 0.8,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(500),
                BeginTime = TimeSpan.FromSeconds(2),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            InstructionsText?.BeginAnimation(OpacityProperty, fadeOutAnimation);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error fading out instructions: {ex}");
            // Hide instructions immediately on error
            if (InstructionsText != null)
                InstructionsText.Visibility = Visibility.Collapsed;
        }
    }

    private void OnToolbarClosed(object? sender, EventArgs e)
    {
        Close();
    }

    private void OnColorSelected(object? sender, System.Windows.Media.Color color)
    {
        Debug.WriteLine($"Color selected in annotation window: {color}");
        // Future: Pass color to annotation canvas for drawing
    }

    private void OnPenToolActivated(object? sender, EventArgs e)
    {
        Debug.WriteLine("Pen tool activated in annotation window");
        // Future: Activate pen drawing mode on canvas
    }

    protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);

        // Re-activate to maintain topmost status
        // This helps ensure the window stays on top of other applications
        if (IsVisible && !_isClosing)
        {
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    Activate();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error re-activating window: {ex}");
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}
