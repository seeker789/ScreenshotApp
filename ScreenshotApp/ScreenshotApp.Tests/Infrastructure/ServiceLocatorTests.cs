using FluentAssertions;
using ScreenshotApp.Core.Infrastructure;

namespace ScreenshotApp.Tests.Infrastructure;

public class ServiceLocatorTests : IDisposable
{
    public ServiceLocatorTests()
    {
        // Reset before each test
        ServiceLocator.Reset();
    }

    public void Dispose()
    {
        ServiceLocator.Reset();
    }

    [Fact]
    public void Initialize_ShouldSetIsInitializedToTrue()
    {
        // Act
        ServiceLocator.Initialize();

        // Assert
        ServiceLocator.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public void Initialize_WhenAlreadyInitialized_ShouldThrowInvalidOperationException()
    {
        // Arrange
        ServiceLocator.Initialize();

        // Act
        Action act = () => ServiceLocator.Initialize();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already been initialized*");
    }

    [Fact]
    public void Register_AndGet_ShouldReturnRegisteredService()
    {
        // Arrange
        ServiceLocator.Initialize();
        var service = new TestService();

        // Act
        ServiceLocator.Register<ITestService>(service);
        var result = ServiceLocator.Get<ITestService>();

        // Assert
        result.Should().BeSameAs(service);
    }

    [Fact]
    public void Get_WhenServiceNotRegistered_ShouldThrowInvalidOperationException()
    {
        // Arrange
        ServiceLocator.Initialize();

        // Act
        Action act = () => ServiceLocator.Get<ITestService>();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not registered*");
    }

    [Fact]
    public void TryGet_WhenServiceRegistered_ShouldReturnTrueAndService()
    {
        // Arrange
        ServiceLocator.Initialize();
        var service = new TestService();
        ServiceLocator.Register<ITestService>(service);

        // Act
        bool found = ServiceLocator.TryGet<ITestService>(out var result);

        // Assert
        found.Should().BeTrue();
        result.Should().BeSameAs(service);
    }

    [Fact]
    public void TryGet_WhenServiceNotRegistered_ShouldReturnFalseAndNull()
    {
        // Arrange
        ServiceLocator.Initialize();

        // Act
        bool found = ServiceLocator.TryGet<ITestService>(out var result);

        // Assert
        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void IsRegistered_WhenServiceRegistered_ShouldReturnTrue()
    {
        // Arrange
        ServiceLocator.Initialize();
        var service = new TestService();
        ServiceLocator.Register<ITestService>(service);

        // Act
        bool registered = ServiceLocator.IsRegistered<ITestService>();

        // Assert
        registered.Should().BeTrue();
    }

    [Fact]
    public void IsRegistered_WhenServiceNotRegistered_ShouldReturnFalse()
    {
        // Arrange
        ServiceLocator.Initialize();

        // Act
        bool registered = ServiceLocator.IsRegistered<ITestService>();

        // Assert
        registered.Should().BeFalse();
    }

    [Fact]
    public void Register_WithNullService_ShouldThrowArgumentNullException()
    {
        // Arrange
        ServiceLocator.Initialize();
        ITestService? nullService = null;

        // Act
        Action act = () => ServiceLocator.Register(nullService!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Reset_ShouldClearAllServicesAndResetInitializedFlag()
    {
        // Arrange
        ServiceLocator.Initialize();
        var service = new TestService();
        ServiceLocator.Register<ITestService>(service);

        // Act
        ServiceLocator.Reset();

        // Assert
        ServiceLocator.IsInitialized.Should().BeFalse();
        ServiceLocator.IsRegistered<ITestService>().Should().BeFalse();
    }

    // Test interfaces and implementations
    public interface ITestService { }
    public class TestService : ITestService { }
}
