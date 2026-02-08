using ScreenshotApp.Core.Models;

namespace ScreenshotApp.Core.Services.Interfaces;

/// <summary>
/// Service for capturing screen regions and windows.
/// </summary>
public interface ICaptureService
{
    /// <summary>
    /// Attempts to capture a specific screen region.
    /// </summary>
    /// <param name="region">The region to capture.</param>
    /// <param name="result">The capture result if successful.</param>
    /// <returns>True if capture succeeded; otherwise, false.</returns>
    bool TryCaptureRegion(CaptureRegion region, out CaptureResult? result);

    /// <summary>
    /// Attempts to capture the entire screen.
    /// </summary>
    /// <param name="result">The capture result if successful.</param>
    /// <returns>True if capture succeeded; otherwise, false.</returns>
    bool TryCaptureScreen(out CaptureResult? result);

    /// <summary>
    /// Attempts to capture a specific window by handle.
    /// </summary>
    /// <param name="windowHandle">The window handle to capture.</param>
    /// <param name="result">The capture result if successful.</param>
    /// <returns>True if capture succeeded; otherwise, false.</returns>
    bool TryCaptureWindow(IntPtr windowHandle, out CaptureResult? result);
}
