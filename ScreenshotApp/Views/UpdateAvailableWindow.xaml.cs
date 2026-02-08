using System.Diagnostics;
using System.Windows;
using ScreenshotApp.Core.Models;

namespace ScreenshotApp.Views;

/// <summary>
/// Window for displaying update check results (update available, up-to-date, or error).
/// </summary>
public partial class UpdateAvailableWindow : Window
{
    private readonly UpdateCheckResult _result;
    private readonly bool _isError;
    private readonly bool _isUpToDate;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAvailableWindow"/> class for update available.
    /// </summary>
    /// <param name="result">The update check result.</param>
    public UpdateAvailableWindow(UpdateCheckResult result)
    {
        InitializeComponent();
        _result = result;
        _isError = result.HasError;
        _isUpToDate = !result.HasError && !result.IsUpdateAvailable;

        ConfigureWindow();
    }

    private void ConfigureWindow()
    {
        if (_isError)
        {
            ConfigureErrorState();
        }
        else if (_isUpToDate)
        {
            ConfigureUpToDateState();
        }
        else
        {
            ConfigureUpdateAvailableState();
        }
    }

    private void ConfigureErrorState()
    {
        Title = "Update Check Failed";
        IconText.Text = "\xE783"; // Error icon
        IconText.Foreground = System.Windows.Media.Brushes.OrangeRed;
        TitleText.Text = "Unable to Check for Updates";
        VersionText.Text = "";
        MessageText.Text = _result.ErrorMessage ?? "An unexpected error occurred. Please try again later.";
        CurrentVersionText.Text = "";
        LatestVersionText.Text = "";
        PrimaryButton.Content = "Retry";
        PrimaryButton.Visibility = Visibility.Collapsed; // Hide retry for simplicity
    }

    private void ConfigureUpToDateState()
    {
        Title = "Up to Date";
        IconText.Text = "\xE930"; // Checkmark icon
        IconText.Foreground = System.Windows.Media.Brushes.LimeGreen;
        TitleText.Text = "You're Up to Date!";
        VersionText.Text = $"Version {_result.CurrentVersion}";
        MessageText.Text = "You are running the latest version of Screenshot Tool.";
        CurrentVersionText.Text = $"Current Version: {_result.CurrentVersion}";
        LatestVersionText.Text = "Status: Latest";
        PrimaryButton.Content = "OK";
    }

    private void ConfigureUpdateAvailableState()
    {
        Title = "Update Available";
        IconText.Text = "\xE896"; // Download icon
        IconText.Foreground = System.Windows.Media.Brushes.DodgerBlue;
        TitleText.Text = "Update Available";
        VersionText.Text = $"New Version: {_result.LatestVersion}";
        MessageText.Text = "A new version of Screenshot Tool is available. Download the latest version from GitHub.";
        CurrentVersionText.Text = $"Current Version: {_result.CurrentVersion}";
        LatestVersionText.Text = $"Latest Version: {_result.LatestVersion}";
        PrimaryButton.Content = "Go to GitHub";
    }

    private void OnPrimaryClick(object sender, RoutedEventArgs e)
    {
        if (_isUpToDate)
        {
            Close();
        }
        else if (!_isError && _result.IsUpdateAvailable && !string.IsNullOrEmpty(_result.ReleaseUrl))
        {
            // Open GitHub releases page
            try
            {
                Process.Start(new ProcessStartInfo(_result.ReleaseUrl)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening browser: {ex}");
                System.Windows.MessageBox.Show(
                    $"Unable to open browser. Please visit:\n{_result.ReleaseUrl}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            Close();
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
