namespace ScreenshotApp.Core.Services.Interfaces;

/// <summary>
/// Interface for types that provide access to an IServiceProvider.
/// Used to resolve services from the application's dependency injection container.
/// </summary>
public interface IServiceProviderProvider
{
    /// <summary>
    /// Gets a service of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of service to get.</typeparam>
    /// <returns>The service instance, or null if not found.</returns>
    T? GetService<T>() where T : class;
}
