namespace ScreenshotApp.Core.Models;

/// <summary>
/// Represents a rectangular region on the screen for capture.
/// </summary>
public readonly record struct CaptureRegion
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }

    public CaptureRegion(int x, int y, int width, int height)
    {
        if (width < 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be non-negative.");
        if (height < 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be non-negative.");

        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public bool IsEmpty => Width == 0 || Height == 0;

    public bool Contains(int x, int y) =>
        x >= X && x < X + Width &&
        y >= Y && y < Y + Height;

    /// <summary>
    /// Converts this CaptureRegion to a WPF Rect.
    /// </summary>
    public System.Windows.Rect ToRect()
    {
        return new System.Windows.Rect(X, Y, Width, Height);
    }

    /// <summary>
    /// Converts this CaptureRegion to a System.Drawing.Rectangle for GDI+ operations.
    /// </summary>
    public System.Drawing.Rectangle ToDrawingRect()
    {
        return new System.Drawing.Rectangle(X, Y, Width, Height);
    }

    public override string ToString() => $"CaptureRegion({X}, {Y}, {Width}x{Height})";
}
