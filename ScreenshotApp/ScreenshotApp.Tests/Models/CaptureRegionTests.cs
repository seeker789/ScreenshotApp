using FluentAssertions;
using ScreenshotApp.Core.Models;

namespace ScreenshotApp.Tests.Models;

public class CaptureRegionTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        // Act
        var region = new CaptureRegion(10, 20, 100, 200);

        // Assert
        region.X.Should().Be(10);
        region.Y.Should().Be(20);
        region.Width.Should().Be(100);
        region.Height.Should().Be(200);
    }

    [Theory]
    [InlineData(0, 0, 0, 0, true)]   // Empty dimensions
    [InlineData(0, 0, 100, 0, true)] // Zero height
    [InlineData(0, 0, 0, 100, true)] // Zero width
    [InlineData(0, 0, 100, 100, false)] // Valid region
    public void IsEmpty_ShouldReturnExpectedValue(int x, int y, int width, int height, bool expectedEmpty)
    {
        // Arrange
        var region = new CaptureRegion(x, y, width, height);

        // Act & Assert
        region.IsEmpty.Should().Be(expectedEmpty);
    }

    [Theory]
    [InlineData(50, 50, true)]   // Inside region
    [InlineData(10, 10, true)]   // On edge
    [InlineData(109, 109, true)] // Just inside
    [InlineData(110, 50, false)] // Outside (x too high)
    [InlineData(50, 110, false)] // Outside (y too high)
    [InlineData(9, 50, false)]   // Outside (x too low)
    [InlineData(50, 9, false)]   // Outside (y too low)
    public void Contains_ShouldReturnExpectedValue(int pointX, int pointY, bool expectedContains)
    {
        // Arrange
        var region = new CaptureRegion(10, 10, 100, 100);

        // Act
        bool contains = region.Contains(pointX, pointY);

        // Assert
        contains.Should().Be(expectedContains);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var region = new CaptureRegion(10, 20, 100, 200);

        // Act
        string result = region.ToString();

        // Assert
        result.Should().Be("CaptureRegion(10, 20, 100x200)");
    }

    [Fact]
    public void RecordEquality_SameValues_ShouldBeEqual()
    {
        // Arrange
        var region1 = new CaptureRegion(10, 20, 100, 200);
        var region2 = new CaptureRegion(10, 20, 100, 200);

        // Act & Assert
        region1.Should().Be(region2);
        (region1 == region2).Should().BeTrue();
    }

    [Fact]
    public void RecordEquality_DifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var region1 = new CaptureRegion(10, 20, 100, 200);
        var region2 = new CaptureRegion(15, 25, 100, 200);

        // Act & Assert
        region1.Should().NotBe(region2);
        (region1 != region2).Should().BeTrue();
    }
}
