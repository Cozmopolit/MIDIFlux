using MIDIFlux.App.Services;
using MIDIFlux.Core;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Config;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.Hardware;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.State;
using MIDIFlux.Core.Services;
using MIDIFlux.Core.Helpers;
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
    /// Adds MIDIFlux services to the service collection with action system
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMIDIFluxServices(this IServiceCollection services)
    {
        // Add core services
        services.AddSingleton<KeyboardSimulator>();
        services.AddSingleton<ActionStateManager>();

        // Add ProfileManager with action system
        services.AddSingleton<ProfileManager>((provider) => {
            var logger = provider.GetRequiredService<ILogger<ProfileManager>>();
            var actionStateManager = provider.GetRequiredService<ActionStateManager>();
            return new ProfileManager(logger, actionStateManager, provider);
        });

        // Register MIDI hardware abstraction layer
        services.AddSingleton<IMidiHardwareAdapter, NAudioMidiAdapter>();
        services.AddSingleton<MidiDeviceManager>();

        // Add configuration services
        services.AddSingleton<ConfigurationService>();

        // Add audio services for PlaySoundAction
        services.AddSingleton<AudioFormatConverter>();
        services.AddSingleton<IAudioPlaybackService, AudioPlaybackService>();

        // Add the MIDI processing service with action system
        services.AddHostedService<MidiProcessingService>();
        services.AddSingleton<MidiProcessingService>(provider =>
            provider.GetRequiredService<IHostedService>() as MidiProcessingService
            ?? throw new InvalidOperationException("Failed to resolve MidiProcessingService"));

        return services;
    }



    /// <summary>
    /// Sets the static service provider for the unified action system
    /// </summary>
    /// <param name="serviceProvider">The service provider to set</param>
    public static void SetActionServiceProvider(IServiceProvider serviceProvider)
    {
        ActionBase.ServiceProvider = serviceProvider;
    }
}
