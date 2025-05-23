using System;

namespace MIDIFlux.Core.Interfaces
{
    /// <summary>
    /// Interface for the MIDI processing service
    /// </summary>
    public interface IMidiProcessingService
    {
        /// <summary>
        /// Gets the path to the active configuration
        /// </summary>
        string? ActiveConfigurationPath { get; }

        /// <summary>
        /// Event that is raised when the status of the service changes
        /// </summary>
        event EventHandler<bool>? StatusChanged;

        /// <summary>
        /// Loads a configuration from the specified path
        /// </summary>
        /// <param name="configPath">The path to the configuration file</param>
        /// <returns>True if successful, false otherwise</returns>
        bool LoadConfiguration(string configPath);

        /// <summary>
        /// Starts MIDI processing
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        bool Start();

        /// <summary>
        /// Stops MIDI processing
        /// </summary>
        void Stop();
    }
}
