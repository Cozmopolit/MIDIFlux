using MIDIFlux.Core.Helpers;

namespace MIDIFlux.Core.Tests;

/// <summary>
/// Base class for all unit tests that ensures proper test environment setup.
/// Enables silent mode to prevent message boxes from hanging tests.
/// </summary>
public abstract class TestBase : IDisposable
{
    private readonly bool _originalSilentMode;

    /// <summary>
    /// Initializes the test base by enabling silent mode
    /// </summary>
    protected TestBase()
    {
        // Store the original silent mode state
        _originalSilentMode = ApplicationErrorHandler.SilentMode;
        
        // Enable silent mode to prevent message boxes during tests
        ApplicationErrorHandler.SilentMode = true;
    }

    /// <summary>
    /// Restores the original silent mode state
    /// </summary>
    public virtual void Dispose()
    {
        // Restore the original silent mode state
        ApplicationErrorHandler.SilentMode = _originalSilentMode;
        GC.SuppressFinalize(this);
    }
}
