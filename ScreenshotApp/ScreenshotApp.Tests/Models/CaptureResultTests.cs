using FluentAssertions;
using ScreenshotApp.Core.Models;

namespace ScreenshotApp.Tests.Models;

public class CaptureResultTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        // Arrange
        var imageData = new byte[] { 1, 2, 3, 4, 5 };
        var region = new CaptureRegion(10, 20, 100, 200);

        // Act
        var result = new CaptureResult(imageData, region, 100, 200);

        // Assert
        result.ImageData.Should().BeEquivalentTo(imageData);
        result.Region.Should().Be(region);
        result.Width.Should().Be(100);
        result.Height.Should().Be(200);
    }

    [Fact]
    public void Constructor_ShouldSetTimestamp()
    {
        // Arrange
        var before = DateTime.UtcNow.AddSeconds(-1);
        var imageData = new byte[] { 1, 2, 3 };
        var region = new CaptureRegion(0, 0, 100, 100);

        // Act
        var result = new CaptureResult(imageData, region, 100, 100);

        // Assert
        var after = DateTime.UtcNow.AddSeconds(1);
        result.Timestamp.Should().BeAfter(before);
        result.Timestamp.Should().BeBefore(after);
    }

    [Fact]
    public void Constructor_WithNullImageData_ShouldThrowArgumentNullException()
    {
        // Arrange
        byte[]? imageData = null;
        var region = new CaptureRegion(0, 0, 100, 100);

        // Act
        Action act = () => new CaptureResult(imageData!, region, 100, 100);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("imageData");
    }
}
