using System.Diagnostics;
using System.Reflection;
using System.Windows;
using ScreenshotApp.Services;

namespace ScreenshotApp.Views;

/// <summary>
/// About window showing application information and version.
/// </summary>
public partial class AboutWindow : Window
{
    // GitHub repository URL - matches GitHubUpdateService defaults
    // TODO: Make this configurable via settings or shared configuration
    private const string GitHubRepoUrl = $"https://github.com/{GitHubUpdateService.DefaultRepoOwner}/{GitHubUpdateService.DefaultRepoName}";

    public AboutWindow()
    {
        InitializeComponent();
        LoadVersionInfo();
        LoadGitHubLink();
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

    private void LoadGitHubLink()
    {
        try
        {
            GitHubLink.NavigateUri = new System.Uri(GitHubRepoUrl);
        }
        catch
        {
            // Fallback to a safe default if URI construction fails
            Debug.WriteLine($"Failed to set GitHub link URI: {GitHubRepoUrl}");
        }
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnHyperlinkNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });
            e.Handled = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening link: {ex}");
        }
    }
}
