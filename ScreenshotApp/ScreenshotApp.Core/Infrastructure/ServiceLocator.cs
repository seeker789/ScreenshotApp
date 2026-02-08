using System.Collections.Concurrent;

namespace ScreenshotApp.Core.Infrastructure;

/// <summary>
/// Provides static access to registered services throughout the application.
/// Implements the Service Locator pattern for dependency resolution.
/// </summary>
public static class ServiceLocator
{
    private static readonly ConcurrentDictionary<Type, object> _services = new();
    private static bool _isInitialized = false;

    /// <summary>
    /// Gets whether the ServiceLocator has been initialized.
    /// </summary>
    public static bool IsInitialized => _isInitialized;

    /// <summary>
    /// Initializes the ServiceLocator with all application services.
    /// Must be called once at application startup.
    /// </summary>
    public static void Initialize()
    {
        if (_isInitialized)
        {
            throw new InvalidOperationException("ServiceLocator has already been initialized.");
        }

        // Services will be registered here by App.xaml.cs
        _isInitialized = true;
    }

    /// <summary>
    /// Registers a service instance.
    /// </summary>
    /// <typeparam name="T">The service interface type.</typeparam>
    /// <param name="service">The service implementation instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when service is null.</exception>
    public static void Register<T>(T service) where T : class
    {
        ArgumentNullException.ThrowIfNull(service);
        _services[typeof(T)] = service;
    }

    /// <summary>
    /// Gets a registered service by type.
    /// </summary>
    /// <typeparam name="T">The service interface type.</typeparam>
    /// <returns>The registered service instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when service is not registered.</exception>
    public static T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var service))
        {
            return (T)service;
        }

        throw new InvalidOperationException($"Service {typeof(T).FullName} is not registered.");
    }

    /// <summary>
    /// Attempts to get a registered service by type.
    /// </summary>
    /// <typeparam name="T">The service interface type.</typeparam>
    /// <param name="service">The service instance if found; otherwise, null.</param>
    /// <returns>True if the service is registered; otherwise, false.</returns>
    public static bool TryGet<T>(out T? service) where T : class
    {
        if (_services.TryGetValue(typeof(T), out var obj))
        {
            service = (T)obj;
            return true;
        }

        service = null;
        return false;
    }

    /// <summary>
    /// Checks if a service is registered.
    /// </summary>
    /// <typeparam name="T">The service interface type.</typeparam>
    /// <returns>True if the service is registered; otherwise, false.</returns>
    public static bool IsRegistered<T>() where T : class
    {
        return _services.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Clears all registered services. Primarily used for testing.
    /// </summary>
    public static void Reset()
    {
        _services.Clear();
        _isInitialized = false;
    }
}
