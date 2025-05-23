using MIDIFlux.App.Services;
using MIDIFlux.Core;
using MIDIFlux.Core.Config;
using MIDIFlux.Core.Handlers.Factory;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Midi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.App.Extensions;

/// <summary>
/// Extension methods for IServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MIDIFlux services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMIDIFluxServices(this IServiceCollection services)
    {
        // Add core services
        services.AddSingleton<KeyboardSimulator>();
        services.AddSingleton<HandlerFactory>();
        services.AddSingleton<KeyStateManager>();
        services.AddSingleton<EventDispatcher>((provider) => {
            var keyboardSimulator = provider.GetRequiredService<KeyboardSimulator>();
            var logger = provider.GetRequiredService<ILogger<EventDispatcher>>();
            var handlerFactory = provider.GetRequiredService<HandlerFactory>();
            var keyStateManager = provider.GetRequiredService<KeyStateManager>();
            return new EventDispatcher(keyboardSimulator, logger, handlerFactory, keyStateManager, provider);
        });
        services.AddSingleton<ConfigLoader>();
        services.AddSingleton<MidiManager>();

        // Add the MIDI processing service
        services.AddHostedService<MidiProcessingService>();
        services.AddSingleton<MidiProcessingService>(provider =>
            provider.GetRequiredService<IHostedService>() as MidiProcessingService
            ?? throw new InvalidOperationException("Failed to resolve MidiProcessingService"));

        return services;
    }
}
