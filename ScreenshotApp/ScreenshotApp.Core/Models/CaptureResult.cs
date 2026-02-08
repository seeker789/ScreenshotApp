namespace ScreenshotApp.Core.Models;

/// <summary>
/// Represents the result of a screen capture operation.
/// </summary>
public class CaptureResult
{
    /// <summary>
    /// Gets the captured image data as a byte array (PNG format).
    /// </summary>
    public byte[] ImageData { get; }

    /// <summary>
    /// Gets the region that was captured.
    /// </summary>
    public CaptureRegion Region { get; }

    /// <summary>
    /// Gets the timestamp when the capture occurred.
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Gets the width of the captured image.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height of the captured image.
    /// </summary>
    public int Height { get; }

    public CaptureResult(byte[] imageData, CaptureRegion region, int width, int height)
    {
        ImageData = imageData ?? throw new ArgumentNullException(nameof(imageData));
        Region = region;
        Width = width;
        Height = height;
        Timestamp = DateTime.UtcNow;
    }
}
