using System.Diagnostics;
using System.Windows;
using ScreenshotApp.Core.Infrastructure;
using ScreenshotApp.Core.Services.Interfaces;
using ScreenshotApp.Infrastructure.Win32;
using ScreenshotApp.Services;

namespace ScreenshotApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private ITrayService? _trayService;
    private bool _isSilentStartup;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Detect if app was auto-started by Windows
        _isSilentStartup = DetectSilentStartup(e.Args);

        // Initialize ServiceLocator before any UI is created
        InitializeServices();

        // Initialize tray icon early (must be on UI thread)
        InitializeTrayIcon();

        // Only show MainWindow if not in silent startup mode
        if (!_isSilentStartup)
        {
            // Normal startup - show the main window
            if (MainWindow != null)
            {
                MainWindow.Show();
            }
        }
        else
        {
            // Silent startup - window is created but not shown
            // MainWindow is already initialized by XAML, we just don't show it
            System.Diagnostics.Debug.WriteLine("Silent startup: MainWindow will not be shown");
        }

        // Initialize hotkey service after MainWindow is created (needed for window handle)
        InitializeHotkeyService();

        base.OnStartup(e);
    }

    /// <summary>
    /// Detects if the application was started automatically by Windows.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>True if auto-started; otherwise, false.</returns>
    private static bool DetectSilentStartup(string[] args)
    {
        // Check for --auto-started argument passed via registry
        // Also check Environment.GetCommandLineArgs() as fallback for WPF startup scenarios
        return args.Contains("--auto-started") ||
               Environment.GetCommandLineArgs().Contains("--auto-started");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Clean up tray icon
        _trayService?.Dispose();

        base.OnExit(e);
    }

    private static void InitializeServices()
    {
        // Initialize the ServiceLocator
        ServiceLocator.Initialize();

        // Register all services as singletons
        ServiceLocator.Register<IHotkeyService>(new Win32HotkeyService());

        // Startup registry service must be registered before settings service
        var startupService = new StartupRegistryService();
        ServiceLocator.Register<IStartupRegistryService>(startupService);

        // Settings service depends on startup service for auto-start sync
        ServiceLocator.Register<ISettingsService>(new RegistrySettingsService(startupService));
        ServiceLocator.Register<IThemeService>(new Win32ThemeService());
        ServiceLocator.Register<ICaptureService>(new Win32CaptureService());
        ServiceLocator.Register<IUpdateService>(new GitHubUpdateService());
    }

    private void InitializeTrayIcon()
    {
        try
        {
            _trayService = new TrayService();
            _trayService.Initialize();

            // Register in ServiceLocator FIRST before wiring events
            // This ensures any service resolving ITrayService during event handlers can find it
            ServiceLocator.Register<ITrayService>(_trayService);

            // Wire up tray events
            _trayService.TrayLeftClick += OnTrayLeftClick;
            _trayService.CaptureRequested += OnTrayCaptureRequested;
            _trayService.SettingsRequested += OnTraySettingsRequested;
            _trayService.CheckForUpdatesRequested += OnTrayCheckForUpdatesRequested;
            _trayService.AboutRequested += OnTrayAboutRequested;
            _trayService.ExitRequested += OnTrayExitRequested;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize tray icon: {ex}");
            // Continue without tray - application can still function
        }
    }

    private void OnTrayLeftClick(object? sender, EventArgs e)
    {
        // Left-click initiates capture
        OnTrayCaptureRequested(sender, e);
    }

    private void OnTrayCaptureRequested(object? sender, EventArgs e)
    {
        // Trigger capture via MainWindow's ViewModel
        if (MainWindow?.DataContext is ViewModels.MainViewModel vm)
        {
            vm.StartCaptureCommand.Execute(null);
        }
    }

    private void OnTraySettingsRequested(object? sender, EventArgs e)
    {
        if (MainWindow?.DataContext is ViewModels.MainViewModel vm)
        {
            vm.OpenSettingsCommand.Execute(null);
        }
    }

    private async void OnTrayCheckForUpdatesRequested(object? sender, EventArgs e)
    {
        try
        {
            var updateService = ServiceLocator.Get<IUpdateService>();
            var result = await updateService.CheckForUpdatesAsync();

            // Show result window on UI thread
            Dispatcher.Invoke(() =>
            {
                var updateWindow = new Views.UpdateAvailableWindow(result);
                updateWindow.ShowDialog();
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error checking for updates: {ex}");
            _trayService?.ShowNotification("Screenshot Tool", "Unable to check for updates. Please try again later.", TrayIconType.Error);
        }
    }

    private void OnTrayAboutRequested(object? sender, EventArgs e)
    {
        // Show about dialog
        var aboutWindow = new Views.AboutWindow();
        aboutWindow.ShowDialog();
    }

    private void OnTrayExitRequested(object? sender, EventArgs e)
    {
        Shutdown();
    }

    private void InitializeHotkeyService()
    {
        try
        {
            var hotkeyService = ServiceLocator.Get<IHotkeyService>();
            var settingsService = ServiceLocator.Get<ISettingsService>();

            // Validate MainWindow is available
            if (MainWindow == null)
            {
                Debug.WriteLine("Hotkey initialization skipped: MainWindow is null");
                return;
            }

            // Ensure MainWindow handle is created
            var helper = new System.Windows.Interop.WindowInteropHelper(MainWindow);
            helper.EnsureHandle();

            if (helper.Handle == IntPtr.Zero)
            {
                Debug.WriteLine("Hotkey initialization failed: Unable to create window handle");
                return;
            }

            // Set window handle for hotkey message processing
            if (hotkeyService is Win32HotkeyService win32HotkeyService)
            {
                win32HotkeyService.SetWindowHandle(helper.Handle);

                // Add hook for WM_HOTKEY messages
                var source = System.Windows.Interop.HwndSource.FromHwnd(helper.Handle);
                source?.AddHook(HwndHook);
            }

            // Register the hotkey (default or saved)
            var hotkeyToRegister = settingsService.CaptureHotkey.Key != 0
                ? new KeyCombo(settingsService.CaptureHotkey.Key, settingsService.CaptureHotkey.Modifiers)
                : KeyCombo.Default;

            bool registered = hotkeyService.TryRegisterHotkey(hotkeyToRegister);

            if (registered)
            {
                Debug.WriteLine($"Hotkey registered: {hotkeyService.CurrentHotkey}");

                // Save the registered hotkey to settings
                if (hotkeyService.CurrentHotkey.HasValue)
                {
                    settingsService.CaptureHotkey = new HotkeyConfig(
                        hotkeyService.CurrentHotkey.Value.Modifiers,
                        hotkeyService.CurrentHotkey.Value.Key);
                }
            }
            else
            {
                Debug.WriteLine("Failed to register any hotkey. Tray capture still available.");
            }
        }
        catch (Exception ex)
        {
            // Defensive error handling - never crash on hotkey initialization
            Debug.WriteLine($"Error initializing hotkey service: {ex}");
        }
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY)
        {
            var hotkeyService = ServiceLocator.Get<IHotkeyService>();
            if (hotkeyService is Win32HotkeyService win32Service)
            {
                win32Service.ProcessWindowMessage(msg, wParam);
            }
            handled = true;
        }
        return IntPtr.Zero;
    }
}
