using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ScreenshotApp.Core.Services.Interfaces;

namespace ScreenshotApp.Services;

/// <summary>
/// Windows Forms NotifyIcon-based implementation of ITrayService.
/// Provides system tray integration without external dependencies.
/// Handles Explorer restart to recreate tray icon.
/// </summary>
public class TrayService : ITrayService, IDisposable
{
    private NotifyIcon? _notifyIcon;
    private TrayMessageWindow? _messageWindow;
    private bool _isInitialized;
    private bool _isDisposed;

    // Window message sent when taskbar is created (Explorer restart)
    private static readonly uint WM_TASKBARCREATED = RegisterWindowMessage("TaskbarCreated");

    /// <inheritdoc />
    public bool IsVisible => _notifyIcon?.Visible ?? false;

    /// <inheritdoc />
    public bool IsInitialized => _isInitialized;

    /// <inheritdoc />
    public event EventHandler? TrayLeftClick;

    /// <inheritdoc />
    public event EventHandler? CaptureRequested;

    /// <inheritdoc />
    public event EventHandler? SettingsRequested;

    /// <inheritdoc />
    public event EventHandler? CheckForUpdatesRequested;

    /// <inheritdoc />
    public event EventHandler? AboutRequested;

    /// <inheritdoc />
    public event EventHandler? ExitRequested;

    /// <inheritdoc />
    public void Initialize()
    {
        if (_isInitialized)
        {
            throw new InvalidOperationException("TrayService has already been initialized.");
        }

        try
        {
            _notifyIcon = CreateNotifyIcon();
            _messageWindow = new TrayMessageWindow(this);
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to initialize tray icon: {ex}");
            throw;
        }
    }

    /// <inheritdoc />
    public void ShowNotification(string title, string message, TrayIconType icon = TrayIconType.Info)
    {
        if (_notifyIcon == null || !_isInitialized)
        {
            Debug.WriteLine("Cannot show notification: tray icon not initialized");
            return;
        }

        try
        {
            var toolTipIcon = icon switch
            {
                TrayIconType.Warning => ToolTipIcon.Warning,
                TrayIconType.Error => ToolTipIcon.Error,
                TrayIconType.Info => ToolTipIcon.Info,
                _ => ToolTipIcon.None
            };

            _notifyIcon.ShowBalloonTip(3000, title, message, toolTipIcon);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error showing notification: {ex}");
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        try
        {
            _messageWindow?.DestroyHandle();
            _messageWindow = null;

            _notifyIcon?.Dispose();
            _notifyIcon = null;
            _isInitialized = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error disposing tray icon: {ex}");
        }
        finally
        {
            _isDisposed = true;
        }

        GC.SuppressFinalize(this);
    }

    private NotifyIcon CreateNotifyIcon()
    {
        var icon = new NotifyIcon
        {
            Text = "Screenshot Tool",
            Visible = true,
            Icon = LoadTrayIcon()
        };

        // Handle left-click for capture
        icon.Click += OnTrayClick;

        // Create context menu for right-click
        icon.ContextMenuStrip = CreateContextMenu();

        return icon;
    }

    private void OnTrayClick(object? sender, EventArgs e)
    {
        // Only handle left-click (MouseButtons.Left is not available in EventArgs directly,
        // so we check if it's NOT a right-click via MouseEventArgs)
        if (e is MouseEventArgs mouseArgs)
        {
            if (mouseArgs.Button == MouseButtons.Left)
            {
                try
                {
                    TrayLeftClick?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Tray left-click error: {ex}");
                }
            }
        }
    }

    private ContextMenuStrip CreateContextMenu()
    {
        var menu = new ContextMenuStrip();

        // Capture Region
        menu.Items.Add(CreateMenuItem("Capture Region", OnCaptureClicked));
        menu.Items.Add(new ToolStripSeparator());

        // Settings
        menu.Items.Add(CreateMenuItem("Settings", OnSettingsClicked));

        // Check for Updates
        menu.Items.Add(CreateMenuItem("Check for Updates", OnCheckForUpdatesClicked));

        // About
        menu.Items.Add(CreateMenuItem("About", OnAboutClicked));
        menu.Items.Add(new ToolStripSeparator());

        // Exit
        menu.Items.Add(CreateMenuItem("Exit", OnExitClicked));

        return menu;
    }

    private static ToolStripMenuItem CreateMenuItem(string text, EventHandler onClick)
    {
        var item = new ToolStripMenuItem(text);
        item.Click += onClick;
        return item;
    }

    private void OnCaptureClicked(object? sender, EventArgs e)
    {
        try
        {
            CaptureRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Capture menu error: {ex}");
        }
    }

    private void OnSettingsClicked(object? sender, EventArgs e)
    {
        try
        {
            SettingsRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Settings menu error: {ex}");
        }
    }

    private void OnCheckForUpdatesClicked(object? sender, EventArgs e)
    {
        try
        {
            CheckForUpdatesRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Check for updates menu error: {ex}");
        }
    }

    private void OnAboutClicked(object? sender, EventArgs e)
    {
        try
        {
            AboutRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"About menu error: {ex}");
        }
    }

    private void OnExitClicked(object? sender, EventArgs e)
    {
        try
        {
            ExitRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exit menu error: {ex}");
        }
    }

    private static Icon LoadTrayIcon()
    {
        try
        {
            // Try to load from file path first
            var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Icons", "TrayIcon.ico");
            if (File.Exists(iconPath))
            {
                using var stream = File.OpenRead(iconPath);
                return new Icon(stream);
            }

            // Create a dynamic icon using a bitmap
            return CreateDynamicTrayIcon();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading tray icon: {ex}");
            return SystemIcons.Application;
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern uint RegisterWindowMessage(string lpString);

    /// <summary>
    /// Hidden window to receive Windows messages (for Explorer restart detection).
    /// </summary>
    private class TrayMessageWindow : NativeWindow
    {
        private readonly TrayService _service;
        private readonly int _taskbarCreatedMsg;

        public TrayMessageWindow(TrayService service)
        {
            _service = service;
            _taskbarCreatedMsg = (int)RegisterWindowMessage("TaskbarCreated");

            CreateHandle(new CreateParams
            {
                ExStyle = 0x80, // WS_EX_TOOLWINDOW (invisible)
                Style = unchecked((int)0x80000000), // WS_POPUP
            });
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == _taskbarCreatedMsg)
            {
                // Explorer restarted - recreate tray icon
                Debug.WriteLine("Explorer restarted - recreating tray icon");
                _service.RecreateTrayIcon();
            }

            base.WndProc(ref m);
        }
    }

    private void RecreateTrayIcon()
    {
        if (_isDisposed || !_isInitialized) return;

        try
        {
            // Dispose existing icon
            _notifyIcon?.Dispose();

            // Create new icon
            _notifyIcon = CreateNotifyIcon();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error recreating tray icon: {ex}");
        }
    }

    private static Icon CreateDynamicTrayIcon()
    {
        try
        {
            // Create a 32x32 bitmap for the tray icon
            using var bitmap = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);

                // Draw a simple camera shape using theme-appropriate colors
                var primaryColor = Color.FromArgb(0, 120, 212); // Windows blue

                // Camera body (rounded rectangle simulation)
                using var bodyBrush = new SolidBrush(primaryColor);
                g.FillRectangle(bodyBrush, 4, 10, 24, 14);

                // Camera lens (white circle)
                using var lensBrush = new SolidBrush(Color.White);
                g.FillEllipse(lensBrush, 12, 12, 8, 8);

                // Inner lens (dark)
                using var innerLensBrush = new SolidBrush(primaryColor);
                g.FillEllipse(innerLensBrush, 14, 14, 4, 4);

                // Flash (small white rectangle)
                g.FillRectangle(Brushes.White, 20, 6, 6, 3);
            }

            // Convert bitmap to icon
            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            ms.Position = 0;

            // Create icon from bitmap
            return Icon.FromHandle(bitmap.GetHicon());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating dynamic tray icon: {ex}");
            return SystemIcons.Application;
        }
    }
}
