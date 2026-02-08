using ScreenshotApp.Core.Infrastructure;
using ScreenshotApp.ViewModels;

namespace ScreenshotApp.Infrastructure;

/// <summary>
/// Provides ViewModel instances as XAML-accessible resources.
/// Used for DataContext binding in XAML files.
/// </summary>
public class ViewModelLocator
{
    private MainViewModel? _mainViewModel;

    /// <summary>
    /// Gets the MainViewModel instance.
    /// Creates on first access after ServiceLocator is initialized.
    /// </summary>
    public MainViewModel MainViewModel
    {
        get
        {
            if (_mainViewModel == null)
            {
                if (!ServiceLocator.IsInitialized)
                {
                    throw new InvalidOperationException(
                        "ServiceLocator must be initialized before accessing ViewModels. " +
                        "Ensure App.xaml.cs calls ServiceLocator.Initialize() before creating UI.");
                }
                _mainViewModel = new MainViewModel();
            }
            return _mainViewModel;
        }
    }

    /// <summary>
    /// Resets the ViewModel instances.
    /// Used primarily for testing scenarios.
    /// </summary>
    public void Reset()
    {
        _mainViewModel = null;
    }
}
