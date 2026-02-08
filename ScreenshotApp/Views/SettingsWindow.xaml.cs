using System.Diagnostics;
using System.Reflection;
using System.Windows;
using ScreenshotApp.Core.Infrastructure;
using ScreenshotApp.Core.Services.Interfaces;

namespace ScreenshotApp.Views;

/// <summary>
/// Settings window for configuring application preferences.
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly ISettingsService _settingsService;
    private readonly IThemeService _themeService;

    public SettingsWindow()
    {
        InitializeComponent();

        // Get services from ServiceLocator
        _settingsService = ServiceLocator.Get<ISettingsService>();
        _themeService = ServiceLocator.Get<IThemeService>();

        LoadSettings();
        LoadVersionInfo();
    }

    private void LoadSettings()
    {
        try
        {
            // Load auto-start setting
            AutoStartCheckBox.IsChecked = _settingsService.AutoStartWithWindows;

            // Load theme setting
            ThemeComboBox.SelectedIndex = _settingsService.Theme switch
            {
                AppTheme.Dark => 0,
                AppTheme.Light => 1,
                _ => 2  // System default
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading settings: {ex}");
        }
    }

    private void LoadVersionInfo()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            VersionText.Text = $"v{version?.ToString(3) ?? "1.0.0"}";
        }
        catch
        {
            VersionText.Text = "v1.0.0";
        }
    }

    private void OnChangeHotkeyClick(object sender, RoutedEventArgs e)
    {
        // TODO: Implement hotkey recording dialog in Story 5.2
        System.Windows.MessageBox.Show(
            "Hotkey configuration will be implemented in a future update.",
            "Screenshot Tool",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void OnAutoStartClick(object sender, RoutedEventArgs e)
    {
        try
        {
            _settingsService.AutoStartWithWindows = AutoStartCheckBox.IsChecked ?? false;
            _settingsService.Save();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error setting auto-start: {ex}");
            System.Windows.MessageBox.Show(
                "Failed to update auto-start setting. Please try again.",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OnThemeChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        try
        {
            _settingsService.Theme = ThemeComboBox.SelectedIndex switch
            {
                0 => AppTheme.Dark,
                1 => AppTheme.Light,
                _ => AppTheme.System
            };
            _settingsService.Save();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error setting theme: {ex}");
        }
    }

    private async void OnCheckForUpdatesClick(object sender, RoutedEventArgs e)
    {
        try
        {
            // Get the update service
            if (!ServiceLocator.TryGet<IUpdateService>(out var updateService))
            {
                System.Windows.MessageBox.Show(
                    "Update service is not available.",
                    "Screenshot Tool",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            // Show progress indicator
            var checkButton = (System.Windows.Controls.Button)sender;
            var originalContent = checkButton.Content;
            checkButton.Content = "Checking...";
            checkButton.IsEnabled = false;

            // Check for updates
            var result = await updateService!.CheckForUpdatesAsync();

            // Restore button
            checkButton.Content = originalContent;
            checkButton.IsEnabled = true;

            // Show result window
            var updateWindow = new UpdateAvailableWindow(result);
            updateWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error checking for updates: {ex}");
            System.Windows.MessageBox.Show(
                "Unable to check for updates. Please try again later.",
                "Screenshot Tool",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
