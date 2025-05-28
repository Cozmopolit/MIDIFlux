using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Midi;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Thread-safe registry for action mappings with optimized lookup performance.
/// Uses immutable registry pattern with lock-free reads and atomic updates.
/// Implements 4-step lookup strategy for optimal performance.
/// </summary>
public class ActionMappingRegistry
{
    private readonly ILogger _logger;
    private readonly SysExPatternMatcher _sysExMatcher;

    // Immutable registry - replaced atomically on updates
    private volatile IReadOnlyDictionary<string, List<ActionMapping>> _mappings =
        new Dictionary<string, List<ActionMapping>>();

    // Separate storage for SysEx mappings that require pattern matching
    private volatile List<ActionMapping> _sysExMappings = new();

    /// <summary>
    /// Gets the total number of mappings in the registry
    /// </summary>
    public int Count => _mappings.Values.Sum(list => list.Count);

    /// <summary>
    /// Initializes a new instance of the ActionMappingRegistry
    /// </summary>
    /// <param name="logger">The logger to use for debug and trace output</param>
    public ActionMappingRegistry(ILogger<ActionMappingRegistry> logger)
    {
        _logger = logger;
        _sysExMatcher = new SysExPatternMatcher();
        _logger.LogDebug("ActionMappingRegistry initialized");
    }

    /// <summary>
    /// Loads mappings into the registry with atomic update.
    /// Builds a new registry and swaps it atomically for thread safety.
    /// </summary>
    /// <param name="mappings">The mappings to load</param>
    public void LoadMappings(IEnumerable<ActionMapping> mappings)
    {
        _logger.LogDebug("Loading mappings into registry...");

        try
        {
            // Build new registry
            var newRegistry = new Dictionary<string, List<ActionMapping>>();
            var newSysExMappings = new List<ActionMapping>();
            int totalMappings = 0;
            int enabledMappings = 0;

            foreach (var mapping in mappings)
            {
                totalMappings++;

                if (!mapping.IsEnabled)
                {
                    if (_logger.IsEnabled(LogLevel.Trace))
                    {
                        _logger.LogTrace("Skipping disabled mapping: {Description}", mapping.Description ?? "No description");
                    }
                    continue;
                }

                enabledMappings++;

                // Handle SysEx mappings separately for pattern matching
                if (mapping.Input.InputType == ActionMidiInputType.SysEx)
                {
                    newSysExMappings.Add(mapping);
                    if (_logger.IsEnabled(LogLevel.Trace))
                    {
                        _logger.LogTrace("Registered SysEx mapping: {Description}",
                            mapping.Description ?? mapping.Action.Description);
                    }
                }
                else
                {
                    // Pre-compute lookup key to avoid string allocation during MIDI processing
                    string lookupKey = mapping.GetLookupKey();

                    if (!newRegistry.TryGetValue(lookupKey, out var mappingList))
                    {
                        mappingList = new List<ActionMapping>();
                        newRegistry[lookupKey] = mappingList;
                    }

                    mappingList.Add(mapping);

                    _logger.LogTrace("Registered mapping: {LookupKey} -> {Description}",
                        lookupKey, mapping.Description ?? mapping.Action.Description);
                }
            }

            // Convert to read-only for immutability
            var readOnlyRegistry = newRegistry.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.AsReadOnly().ToList()
            );

            // Atomic swap
            _mappings = readOnlyRegistry;
            _sysExMappings = newSysExMappings;

            _logger.LogInformation("Loaded {EnabledMappings} enabled mappings out of {TotalMappings} total mappings into registry",
                enabledMappings, totalMappings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load mappings into registry: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Finds actions for the given MIDI input using optimized 4-step lookup strategy.
    /// Lock-free operation for maximum performance in MIDI event processing.
    /// </summary>
    /// <param name="input">The MIDI input to find actions for</param>
    /// <returns>List of matching actions, empty if no matches found</returns>
    public List<IAction> FindActions(ActionMidiInput input)
    {
        var results = new List<IAction>();

        // Handle SysEx pattern matching separately
        if (input.InputType == ActionMidiInputType.SysEx)
        {
            return FindSysExActions(input);
        }

        // Get current registry snapshot (atomic read)
        var currentRegistry = _mappings;

        // 4-step lookup strategy for optimal performance:
        // 1. Exact device + channel match
        // 2. Exact device + wildcard channel
        // 3. Wildcard device + exact channel
        // 4. Wildcard device + wildcard channel

        var lookupKeys = new[]
        {
            $"{input.DeviceName ?? "*"}|{input.Channel?.ToString() ?? "*"}|{input.InputNumber}|{input.InputType}",
            $"{input.DeviceName ?? "*"}|*|{input.InputNumber}|{input.InputType}",
            $"*|{input.Channel?.ToString() ?? "*"}|{input.InputNumber}|{input.InputType}",
            $"*|*|{input.InputNumber}|{input.InputType}"
        };

        foreach (var lookupKey in lookupKeys)
        {
            if (currentRegistry.TryGetValue(lookupKey, out var mappings))
            {
                foreach (var mapping in mappings)
                {
                    if (mapping.IsEnabled)
                    {
                        results.Add(mapping.Action);
                    }
                }

                // Prioritize exact matches - stop after first successful lookup level
                if (results.Count > 0)
                {
                    if (_logger.IsEnabled(LogLevel.Trace))
                    {
                        _logger.LogTrace("Found {Count} actions for input {Input} using lookup key: {LookupKey}",
                            results.Count, input, lookupKey);
                    }
                    break;
                }
            }
        }

        if (results.Count == 0)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("No actions found for input: {Input}", input);
            }
        }

        return results;
    }

    /// <summary>
    /// Finds SysEx actions using pattern matching.
    /// Performs byte-for-byte comparison of received SysEx data against configured patterns.
    /// </summary>
    /// <param name="input">The SysEx input with received data</param>
    /// <returns>List of matching actions, empty if no matches found</returns>
    private List<IAction> FindSysExActions(ActionMidiInput input)
    {
        var results = new List<IAction>();
        var currentSysExMappings = _sysExMappings;

        if (input.SysExPattern == null)
        {
            _logger.LogTrace("No SysEx data provided for pattern matching");
            return results;
        }

        foreach (var mapping in currentSysExMappings)
        {
            if (!mapping.IsEnabled)
                continue;

            // Check device name match (exact or wildcard)
            if (mapping.Input.DeviceName != null && mapping.Input.DeviceName != "*" &&
                mapping.Input.DeviceName != input.DeviceName)
                continue;

            // Check channel match (exact or wildcard) - SysEx is typically channel-independent but we support it
            if (mapping.Input.Channel.HasValue && mapping.Input.Channel != input.Channel)
                continue;

            // Check SysEx pattern match
            if (mapping.Input.SysExPattern != null &&
                _sysExMatcher.Matches(input.SysExPattern, mapping.Input.SysExPattern))
            {
                results.Add(mapping.Action);
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("SysEx pattern matched for mapping: {Description}",
                        mapping.Description ?? mapping.Action.Description);
                }
            }
        }

        if (results.Count == 0)
        {
            _logger.LogTrace("No SysEx pattern matches found for {Length} byte message", input.SysExPattern.Length);
        }
        else
        {
            _logger.LogTrace("Found {Count} SysEx pattern matches", results.Count);
        }

        return results;
    }

    /// <summary>
    /// Gets all registered mappings for debugging and diagnostics.
    /// Returns a snapshot of the current registry state.
    /// </summary>
    /// <returns>All registered mappings</returns>
    public IEnumerable<ActionMapping> GetAllMappings()
    {
        var currentRegistry = _mappings;
        var currentSysExMappings = _sysExMappings;
        return currentRegistry.Values.SelectMany(list => list).Concat(currentSysExMappings);
    }

    /// <summary>
    /// Clears all mappings from the registry
    /// </summary>
    public void Clear()
    {
        _logger.LogDebug("Clearing all mappings from registry");
        _mappings = new Dictionary<string, List<ActionMapping>>();
        _sysExMappings = new List<ActionMapping>();
        _logger.LogInformation("Registry cleared");
    }

    /// <summary>
    /// Gets registry statistics for monitoring and debugging
    /// </summary>
    /// <returns>Registry statistics</returns>
    public RegistryStatistics GetStatistics()
    {
        var currentRegistry = _mappings;
        var allMappings = currentRegistry.Values.SelectMany(list => list).ToList();

        return new RegistryStatistics
        {
            TotalMappings = allMappings.Count,
            EnabledMappings = allMappings.Count(m => m.IsEnabled),
            DisabledMappings = allMappings.Count(m => !m.IsEnabled),
            UniqueDevices = allMappings.Select(m => m.Input.DeviceName ?? "*").Distinct().Count(),
            UniqueChannels = allMappings.Select(m => m.Input.Channel?.ToString() ?? "*").Distinct().Count(),
            LookupKeys = currentRegistry.Keys.Count()
        };
    }
}

/// <summary>
/// Statistics about the registry state for monitoring and debugging
/// </summary>
public class RegistryStatistics
{
    public int TotalMappings { get; set; }
    public int EnabledMappings { get; set; }
    public int DisabledMappings { get; set; }
    public int UniqueDevices { get; set; }
    public int UniqueChannels { get; set; }
    public int LookupKeys { get; set; }

    public override string ToString()
    {
        return $"Registry: {EnabledMappings}/{TotalMappings} enabled mappings, {LookupKeys} lookup keys, {UniqueDevices} devices, {UniqueChannels} channels";
    }
}
