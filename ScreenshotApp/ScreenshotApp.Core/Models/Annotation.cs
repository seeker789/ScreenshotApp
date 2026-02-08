namespace ScreenshotApp.Core.Models;

/// <summary>
/// Represents a drawing annotation (stroke) on a capture.
/// </summary>
public class Annotation
{
    /// <summary>
    /// Gets the unique identifier for this annotation.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the list of points that make up this stroke.
    /// </summary>
    public List<StrokePoint> Points { get; }

    /// <summary>
    /// Gets or sets the color of the stroke (ARGB format).
    /// </summary>
    public uint Color { get; set; }

    /// <summary>
    /// Gets or sets the thickness of the stroke in pixels.
    /// </summary>
    public double Thickness { get; set; }

    /// <summary>
    /// Gets the timestamp when the annotation was created.
    /// </summary>
    public DateTime CreatedAt { get; }

    public Annotation()
    {
        Id = Guid.NewGuid();
        Points = new List<StrokePoint>();
        Color = 0xFFFF0000; // Red by default
        Thickness = 2.0;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddPoint(double x, double y)
    {
        Points.Add(new StrokePoint(x, y));
    }
}

/// <summary>
/// Represents a single point in a stroke.
/// </summary>
public readonly record struct StrokePoint
{
    public double X { get; }
    public double Y { get; }

    public StrokePoint(double x, double y)
    {
        X = x;
        Y = y;
    }
}
