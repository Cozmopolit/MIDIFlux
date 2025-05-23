using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace MIDIFlux.Core.GameController;

/// <summary>
/// Manager for game controller emulation
/// </summary>
public class GameControllerManager : IDisposable
{
    private static GameControllerManager? _instance;
    private static readonly object _lock = new();
    private readonly ILogger _logger;
    private ViGEmClient? _client;
    private readonly Dictionary<int, IXbox360Controller?> _controllers = new();
    private bool _isConnected;
    private bool _isDisposed;
    private const int MAX_CONTROLLERS = 4;

    /// <summary>
    /// Gets a value indicating whether the ViGEm client is available
    /// </summary>
    public bool IsViGEmAvailable => _client != null && _isConnected;

    /// <summary>
    /// Gets the Xbox 360 controller instance (for backward compatibility)
    /// </summary>
    public IXbox360Controller? Controller => GetController(0);

    /// <summary>
    /// Gets the Xbox 360 controller instance with the specified index
    /// </summary>
    /// <param name="index">The controller index (0-3)</param>
    /// <returns>The controller instance, or null if not available</returns>
    public IXbox360Controller? GetController(int index)
    {
        if (index < 0 || index >= MAX_CONTROLLERS)
        {
            _logger.LogWarning("Invalid controller index: {Index}. Must be between 0 and {MaxControllers}", index, MAX_CONTROLLERS - 1);
            return null;
        }

        if (!_controllers.TryGetValue(index, out var controller))
        {
            if (!IsViGEmAvailable)
            {
                _logger.LogWarning("ViGEm is not available. Cannot create controller {Index}", index);
                return null;
            }

            controller = CreateController(index);
            _controllers[index] = controller;
        }

        return controller;
    }

    /// <summary>
    /// Private constructor to prevent direct instantiation
    /// </summary>
    /// <param name="logger">The logger to use</param>
    private GameControllerManager(ILogger logger)
    {
        _logger = logger;
        InitializeViGEm();
    }

    /// <summary>
    /// Gets the singleton instance of the GameControllerManager
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <returns>The singleton instance</returns>
    public static GameControllerManager GetInstance(ILogger logger)
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                _instance ??= new GameControllerManager(logger);
            }
        }

        return _instance;
    }

    /// <summary>
    /// Initializes the ViGEm client
    /// </summary>
    private void InitializeViGEm()
    {
        try
        {
            _logger.LogInformation("Initializing ViGEm client");
            _client = new ViGEmClient();
            _isConnected = true;
            _logger.LogInformation("ViGEm client initialized successfully");
        }
        catch (DllNotFoundException ex)
        {
            _logger.LogWarning("ViGEm Bus Driver not found: {Message}. Game controller features will be disabled.", ex.Message);
            _client = null;
            _isConnected = false;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to initialize ViGEm: {Message}. Game controller features will be disabled.", ex.Message);
            _client = null;
            _isConnected = false;
        }
    }

    /// <summary>
    /// Creates a new Xbox 360 controller
    /// </summary>
    /// <param name="index">The controller index (0-3)</param>
    /// <returns>The created controller, or null if creation failed</returns>
    private IXbox360Controller? CreateController(int index)
    {
        if (!IsViGEmAvailable || _client == null)
        {
            _logger.LogWarning("ViGEm is not available. Cannot create controller {Index}", index);
            return null;
        }

        try
        {
            _logger.LogInformation("Creating Xbox 360 controller {Index}", index);
            var controller = _client.CreateXbox360Controller();

            _logger.LogInformation("Connecting controller {Index}", index);
            controller.Connect();

            _logger.LogInformation("Controller {Index} initialized successfully", index);
            return controller;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create controller {Index}: {Message}", index, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Disposes the game controller manager
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the game controller manager
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // Disconnect all controllers
                foreach (var kvp in _controllers)
                {
                    var controller = kvp.Value;
                    if (controller != null && _isConnected)
                    {
                        try
                        {
                            controller.Disconnect();
                            _logger.LogInformation("ViGEm controller {Index} disconnected", kvp.Key);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Error disconnecting ViGEm controller {Index}: {Message}", kvp.Key, ex.Message);
                        }
                    }
                }

                _controllers.Clear();
                _client?.Dispose();
                _client = null;
            }

            _isDisposed = true;
        }
    }
}
