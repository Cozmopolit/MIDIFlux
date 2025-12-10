using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Hardware;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.State;
using MIDIFlux.Core.Tests.Mocks;

namespace MIDIFlux.Core.Tests.Infrastructure;

/// <summary>
/// Base class for all MIDIFlux tests providing common setup and utilities
/// </summary>
public abstract class TestBase : IDisposable
{
    private bool _disposed = false;

    protected IServiceProvider ServiceProvider { get; private set; }
    protected ILogger Logger { get; private set; }
    protected MockMidiHardwareAdapter MockHardwareAdapter { get; private set; }
    protected ActionStateManager StateManager { get; private set; }

    protected TestBase()
    {
        ServiceProvider = CreateServiceProvider();
        Logger = ServiceProvider.GetRequiredService<ILogger<TestBase>>();
        MockHardwareAdapter = (MockMidiHardwareAdapter)ServiceProvider.GetRequiredService<IMidiHardwareAdapter>();
        StateManager = ServiceProvider.GetRequiredService<ActionStateManager>();

        // Set the global service provider for actions
        ActionBase.ServiceProvider = ServiceProvider;

        // Ensure clean state for each test
        ResetState();
    }

    /// <summary>
    /// Creates a service provider configured for testing
    /// </summary>
    protected virtual IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        // Logging - use a simple debug logger for tests to avoid console output conflicts
        // The Console logger can interfere with xUnit's output handling
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Warning);
            builder.AddDebug(); // Writes to Debug output (visible in debugger, not console)
        });

        // Mock hardware adapter
        services.AddSingleton<IMidiHardwareAdapter, MockMidiHardwareAdapter>();

        // Core services
        services.AddSingleton<ActionStateManager>();
        services.AddSingleton<KeyboardSimulator>();

        // Additional test-specific services can be added by derived classes
        ConfigureServices(services);

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Override this method to add additional services for specific test scenarios
    /// </summary>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Default implementation - no additional services
    }

    /// <summary>
    /// Creates a test logger for a specific type
    /// </summary>
    protected ILogger<T> CreateLogger<T>()
    {
        return ServiceProvider.GetRequiredService<ILogger<T>>();
    }

    /// <summary>
    /// Resets the state manager to a clean state
    /// </summary>
    protected void ResetState()
    {
        StateManager.ClearAllStates();
    }

    /// <summary>
    /// Initializes states for testing
    /// </summary>
    protected void InitializeTestStates(Dictionary<string, int> states)
    {
        StateManager.InitializeStates(states);
    }

    public virtual void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            // Clear state before disposing
            ResetState();
        }
        catch (Exception)
        {
            // Ignore errors during cleanup - service provider might already be disposed
        }

        // Clear the global service provider
        ActionBase.ServiceProvider = null;

        // Dispose the service provider
        if (ServiceProvider is IDisposable disposableProvider)
        {
            disposableProvider.Dispose();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
