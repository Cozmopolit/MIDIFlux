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

        // Register MIDI hardware abstraction layer via factory
        // Uses configuration-based adapter selection with automatic fallback
        services.AddSingleton<IMidiHardwareAdapter>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("MidiAdapterFactory");
            var configService = provider.GetRequiredService<ConfigurationService>();
            var adapterString = configService.GetSetting<string>("MIDI.Adapter", "Auto");
            var preferredType = MidiAdapterFactory.ParseAdapterType(adapterString);
            return MidiAdapterFactory.Create(preferredType, logger);
        });
        services.AddSingleton<MidiDeviceManager>();

        // Add MIDI input detection service
        services.AddSingleton<MidiInputDetector>();

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

        // Add API services
        services.AddSingleton<MIDIFlux.App.Api.ProfileManagementApi>();
        services.AddSingleton<MIDIFlux.App.Api.RuntimeConfigurationApi>();
        services.AddSingleton<MIDIFlux.App.Api.ProfileSwitchingApi>();

        return services;
    }

    /// <summary>
    /// Add MCP server services to the service collection.
    /// This is a separate method to avoid adding MCP services in normal GUI mode.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddMcpServerServices(this IServiceCollection services)
    {
        // Add MCP-specific documentation service
        services.AddSingleton<MIDIFlux.App.Services.DocumentationApi>();

        // Add MCP server
        services.AddSingleton<MIDIFlux.App.Services.MidiFluxMcpServer>();

        // Add MCP server hosted service
        services.AddHostedService<MIDIFlux.App.Services.McpServerHostedService>();

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
