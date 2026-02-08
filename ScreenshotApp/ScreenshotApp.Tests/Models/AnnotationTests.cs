using FluentAssertions;
using ScreenshotApp.Core.Models;

namespace ScreenshotApp.Tests.Models;

public class AnnotationTests
{
    [Fact]
    public void Constructor_ShouldInitializeDefaults()
    {
        // Act
        var annotation = new Annotation();

        // Assert
        annotation.Id.Should().NotBe(Guid.Empty);
        annotation.Points.Should().BeEmpty();
        annotation.Color.Should().Be(0xFFFF0000); // Red
        annotation.Thickness.Should().Be(2.0);
        annotation.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AddPoint_ShouldAddPointToList()
    {
        // Arrange
        var annotation = new Annotation();

        // Act
        annotation.AddPoint(10.5, 20.5);

        // Assert
        annotation.Points.Should().HaveCount(1);
        annotation.Points[0].X.Should().Be(10.5);
        annotation.Points[0].Y.Should().Be(20.5);
    }

    [Fact]
    public void AddPoint_MultiplePoints_ShouldMaintainOrder()
    {
        // Arrange
        var annotation = new Annotation();

        // Act
        annotation.AddPoint(1, 1);
        annotation.AddPoint(2, 2);
        annotation.AddPoint(3, 3);

        // Assert
        annotation.Points.Should().HaveCount(3);
        annotation.Points[0].Should().Be(new StrokePoint(1, 1));
        annotation.Points[1].Should().Be(new StrokePoint(2, 2));
        annotation.Points[2].Should().Be(new StrokePoint(3, 3));
    }

    [Fact]
    public void Color_SetCustomValue_ShouldPersist()
    {
        // Arrange
        var annotation = new Annotation();

        // Act
        annotation.Color = 0xFF00FF00; // Green

        // Assert
        annotation.Color.Should().Be(0xFF00FF00);
    }

    [Fact]
    public void Thickness_SetCustomValue_ShouldPersist()
    {
        // Arrange
        var annotation = new Annotation();

        // Act
        annotation.Thickness = 5.0;

        // Assert
        annotation.Thickness.Should().Be(5.0);
    }

    [Fact]
    public void MultipleAnnotations_ShouldHaveUniqueIds()
    {
        // Act
        var annotation1 = new Annotation();
        var annotation2 = new Annotation();

        // Assert
        annotation1.Id.Should().NotBe(annotation2.Id);
    }
}

public class StrokePointTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        // Act
        var point = new StrokePoint(10.5, 20.5);

        // Assert
        point.X.Should().Be(10.5);
        point.Y.Should().Be(20.5);
    }

    [Fact]
    public void RecordEquality_SameValues_ShouldBeEqual()
    {
        // Arrange
        var point1 = new StrokePoint(10.5, 20.5);
        var point2 = new StrokePoint(10.5, 20.5);

        // Act & Assert
        point1.Should().Be(point2);
        (point1 == point2).Should().BeTrue();
    }
}
