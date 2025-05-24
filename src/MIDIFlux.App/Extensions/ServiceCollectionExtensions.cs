using MIDIFlux.App.Services;
using MIDIFlux.Core;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Config;
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
    /// Adds MIDIFlux services to the service collection with unified action system
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMIDIFluxServices(this IServiceCollection services)
    {
        // Add core services
        services.AddSingleton<KeyboardSimulator>();
        services.AddSingleton<KeyStateManager>();

        // Add unified action system services
        services.AddSingleton<IUnifiedActionFactory, UnifiedActionFactory>();

        // Add EventDispatcher with unified action system
        services.AddSingleton<EventDispatcher>((provider) => {
            var logger = provider.GetRequiredService<ILogger<EventDispatcher>>();
            var keyStateManager = provider.GetRequiredService<KeyStateManager>();
            var actionFactory = provider.GetRequiredService<IUnifiedActionFactory>();
            return new EventDispatcher(logger, keyStateManager, actionFactory, provider);
        });

        services.AddSingleton<MidiManager>();

        // Add the MIDI processing service with unified action system
        services.AddHostedService<MidiProcessingService>();
        services.AddSingleton<MidiProcessingService>(provider =>
            provider.GetRequiredService<IHostedService>() as MidiProcessingService
            ?? throw new InvalidOperationException("Failed to resolve MidiProcessingService"));

        return services;
    }
}
