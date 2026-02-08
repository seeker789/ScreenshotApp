using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ScreenshotApp.Core.Services.Interfaces;

namespace ScreenshotApp.Views;

/// <summary>
/// A floating toolbar for annotation tools with spring animation.
/// </summary>
public partial class FloatingToolbar : System.Windows.Controls.UserControl
{
    /// <summary>
    /// Actual width of the toolbar when rendered (including padding and content).
    /// </summary>
    public const double ActualToolbarWidth = 340;

    /// <summary>
    /// Actual height of the toolbar when rendered.
    /// </summary>
    public const double ActualToolbarHeight = 60;

    private bool _isAnimationEnabled;
    private System.Windows.Media.Color _selectedColor = System.Windows.Media.Colors.Black;
    private System.Windows.Controls.Button? _selectedColorButton;

    /// <summary>
    /// Gets the currently selected color.
    /// </summary>
    public System.Windows.Media.Color SelectedColor => _selectedColor;

    /// <summary>
    /// Raised when a color is selected from the palette.
    /// </summary>
    public event EventHandler<System.Windows.Media.Color>? ColorSelected;

    /// <summary>
    /// Raised when the pen tool is activated.
    /// </summary>
    public event EventHandler? PenToolActivated;

    /// <summary>
    /// Raised when the toolbar is closed.
    /// </summary>
    public event EventHandler? ToolbarClosed;

    public FloatingToolbar()
    {
        InitializeComponent();
    }

    private void OnFloatingToolbarLoaded(object sender, RoutedEventArgs e)
    {
        // Check if animations should be enabled (respect Windows reduced motion setting)
        // Use ClientAreaAnimation as a proxy for animation preferences
        _isAnimationEnabled = SystemParameters.ClientAreaAnimation && !SystemParameters.HighContrast;

        if (_isAnimationEnabled)
        {
            PlaySpringAnimation();
        }
        else
        {
            // Skip animation for accessibility
            ToolbarScale.ScaleX = 1;
            ToolbarScale.ScaleY = 1;
            ToolbarTranslate.Y = 0;
        }

        // Load saved color preference and apply it
        LoadSavedColorPreference();
    }

    /// <summary>
    /// Loads the saved color preference from settings and applies it.
    /// </summary>
    private void LoadSavedColorPreference()
    {
        try
        {
            // Get settings service from application resources or service locator
            var settingsService = App.Current?.Resources["SettingsService"] as ISettingsService
                ?? GetSettingsServiceFromServiceProvider();

            if (settingsService != null)
            {
                var savedColorHex = settingsService.LastAnnotationColor;
                if (!string.IsNullOrEmpty(savedColorHex))
                {
                    // Find the button with matching tag
                    foreach (var child in ColorPalettePanel.Children)
                    {
                        if (child is System.Windows.Controls.Button btn && btn.Tag is string btnColor
                            && btnColor.Equals(savedColorHex, StringComparison.OrdinalIgnoreCase))
                        {
                            // Parse and set the color
                            var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(savedColorHex);
                            _selectedColor = color;
                            UpdateSelectedColorButton(btn);
                            ColorSelected?.Invoke(this, color);
                            Debug.WriteLine($"Loaded saved color preference: {savedColorHex}");
                            return;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading saved color preference: {ex}");
        }

        // Default to black if no saved preference or error occurred
        _selectedColorButton = BlackColorButton;
    }

    /// <summary>
    /// Attempts to get the settings service from the service provider.
    /// </summary>
    private static ISettingsService? GetSettingsServiceFromServiceProvider()
    {
        // Try to resolve from the application's service provider if available
        if (App.Current is IServiceProviderProvider provider)
        {
            return provider.GetService<ISettingsService>();
        }
        return null;
    }

    /// <summary>
    /// Saves the selected color preference to settings.
    /// </summary>
    private static void SaveColorPreference(string colorHex)
    {
        try
        {
            var settingsService = App.Current?.Resources["SettingsService"] as ISettingsService
                ?? GetSettingsServiceFromServiceProvider();

            if (settingsService != null)
            {
                settingsService.LastAnnotationColor = colorHex;
                Debug.WriteLine($"Saved color preference: {colorHex}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving color preference: {ex}");
        }
    }

    private void OnFloatingToolbarUnloaded(object sender, RoutedEventArgs e)
    {
        // Clean up if needed
    }

    /// <summary>
    /// Plays the spring animation for toolbar slide-in.
    /// Target: 200ms with spring physics.
    /// </summary>
    private void PlaySpringAnimation()
    {
        try
        {
            // Create scale animation (spring effect)
            var scaleXAnimation = new SpringDoubleAnimation
            {
                From = 0,
                To = 1,
                SpringStiffness = 300,
                SpringDamping = 20,
                Duration = TimeSpan.FromMilliseconds(200)
            };

            var scaleYAnimation = new SpringDoubleAnimation
            {
                From = 0,
                To = 1,
                SpringStiffness = 300,
                SpringDamping = 20,
                Duration = TimeSpan.FromMilliseconds(200)
            };

            // Create translate animation (slide up)
            var translateAnimation = new DoubleAnimation
            {
                From = 20,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // Apply animations
            ToolbarScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
            ToolbarScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);
            ToolbarTranslate.BeginAnimation(TranslateTransform.YProperty, translateAnimation);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error playing spring animation: {ex}");
            // Fallback: show toolbar without animation
            ToolbarScale.ScaleX = 1;
            ToolbarScale.ScaleY = 1;
            ToolbarTranslate.Y = 0;
        }
    }

    private void OnPenToolClick(object sender, RoutedEventArgs e)
    {
        PenToolActivated?.Invoke(this, EventArgs.Empty);
        Debug.WriteLine("Pen tool activated");
    }

    private void OnColorClick(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is string colorHex)
        {
            try
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorHex);
                _selectedColor = color;

                // Update visual selection state
                UpdateSelectedColorButton(button);

                // Persist color preference
                SaveColorPreference(colorHex);

                ColorSelected?.Invoke(this, color);
                Debug.WriteLine($"Color selected: {colorHex}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing color: {ex}");
            }
        }
    }

    /// <summary>
    /// Updates the visual selection state of color buttons.
    /// Selected button shows white border (IsDefault=true).
    /// </summary>
    private void UpdateSelectedColorButton(System.Windows.Controls.Button newlySelected)
    {
        // Clear previous selection
        if (_selectedColorButton != null)
        {
            _selectedColorButton.IsDefault = false;
        }

        // Set new selection
        _selectedColorButton = newlySelected;
        _selectedColorButton.IsDefault = true;
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        ToolbarClosed?.Invoke(this, EventArgs.Empty);
        Debug.WriteLine("Toolbar close requested");
    }

    /// <summary>
    /// Checks if reduced motion is enabled in Windows settings.
    /// Uses SystemParameters.ClientAreaAnimation which reflects the user's
    /// accessibility preference for animations.
    /// </summary>
    private static bool IsReducedMotionEnabled()
    {
        try
        {
            // SystemParameters.ClientAreaAnimation reflects the user's preference
            // for showing animations in Windows (Accessibility > Visual effects)
            return !SystemParameters.ClientAreaAnimation;
        }
        catch
        {
            // Default to allowing animations if we can't determine the setting
            return false;
        }
    }
}

/// <summary>
/// Spring animation using spring physics for natural motion.
/// Provides a more natural, physics-based animation compared to standard easing functions.
/// </summary>
public class SpringDoubleAnimation : DoubleAnimationBase
{
    /// <summary>
    /// Identifies the SpringStiffness dependency property.
    /// Controls how stiff the spring is (higher values = faster oscillation).
    /// </summary>
    public static readonly DependencyProperty SpringStiffnessProperty =
        DependencyProperty.Register(nameof(SpringStiffness), typeof(double), typeof(SpringDoubleAnimation),
            new PropertyMetadata(100.0));

    /// <summary>
    /// Identifies the SpringDamping dependency property.
    /// Controls how much the spring oscillation is damped (higher values = less oscillation).
    /// </summary>
    public static readonly DependencyProperty SpringDampingProperty =
        DependencyProperty.Register(nameof(SpringDamping), typeof(double), typeof(SpringDoubleAnimation),
            new PropertyMetadata(10.0));

    /// <summary>
    /// Identifies the From dependency property.
    /// The starting value of the animation.
    /// </summary>
    public static readonly DependencyProperty FromProperty =
        DependencyProperty.Register(nameof(From), typeof(double), typeof(SpringDoubleAnimation),
            new PropertyMetadata(0.0));

    /// <summary>
    /// Identifies the To dependency property.
    /// The ending value of the animation.
    /// </summary>
    public static readonly DependencyProperty ToProperty =
        DependencyProperty.Register(nameof(To), typeof(double), typeof(SpringDoubleAnimation),
            new PropertyMetadata(1.0));

    /// <summary>
    /// Gets or sets the spring stiffness. Higher values create faster oscillation.
    /// Default value is 100.
    /// </summary>
    public double SpringStiffness
    {
        get => (double)GetValue(SpringStiffnessProperty);
        set => SetValue(SpringStiffnessProperty, value);
    }

    /// <summary>
    /// Gets or sets the spring damping. Higher values reduce oscillation.
    /// Default value is 10.
    /// </summary>
    public double SpringDamping
    {
        get => (double)GetValue(SpringDampingProperty);
        set => SetValue(SpringDampingProperty, value);
    }

    /// <summary>
    /// Gets or sets the starting value of the animation.
    /// </summary>
    public double From
    {
        get => (double)GetValue(FromProperty);
        set => SetValue(FromProperty, value);
    }

    /// <summary>
    /// Gets or sets the ending value of the animation.
    /// </summary>
    public double To
    {
        get => (double)GetValue(ToProperty);
        set => SetValue(ToProperty, value);
    }

    protected override double GetCurrentValueCore(double defaultOriginValue, double defaultDestinationValue, AnimationClock clock)
    {
        if (!clock.CurrentProgress.HasValue)
            return From;

        double t = clock.CurrentProgress.Value;
        double delta = To - From;

        // Spring physics: damped harmonic oscillator
        // x(t) = A * e^(-bt) * cos(ωt) + (target - A)
        // Simplified for UI animation
        double stiffness = SpringStiffness;
        double damping = SpringDamping;

        // Normalized spring equation
        double omega = Math.Sqrt(stiffness);
        double zeta = damping / (2 * Math.Sqrt(stiffness));

        double value;
        if (zeta < 1) // Underdamped - oscillates
        {
            double omegaD = omega * Math.Sqrt(1 - zeta * zeta);
            value = 1 - Math.Exp(-zeta * omega * t) * (Math.Cos(omegaD * t) + (zeta * omega / omegaD) * Math.Sin(omegaD * t));
        }
        else // Critically damped or overdamped
        {
            value = 1 - Math.Exp(-omega * t) * (1 + omega * t);
        }

        // Clamp to [0, 1] range and scale to target
        value = Math.Max(0, Math.Min(1, value));
        return From + delta * value;
    }

    protected override Freezable CreateInstanceCore()
    {
        return new SpringDoubleAnimation();
    }
}
