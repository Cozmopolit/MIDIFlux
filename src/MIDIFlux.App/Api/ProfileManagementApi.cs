using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.Helpers;

namespace MIDIFlux.App.Api;

/// <summary>
/// API for profile discovery, inspection, and metadata retrieval for external tools.
/// Provides comprehensive profile scanning and content access functionality.
/// </summary>
public class ProfileManagementApi
{
    private readonly ILogger<ProfileManagementApi> _logger;
    private readonly ConfigurationService _configurationService;

    /// <summary>
    /// Initializes a new instance of the ProfileManagementApi
    /// </summary>
    /// <param name="logger">Logger for this API</param>
    /// <param name="configurationService">Configuration service for file operations</param>
    public ProfileManagementApi(ILogger<ProfileManagementApi> logger, ConfigurationService configurationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
    }

    /// <summary>
    /// Scans profiles directory including subdirectories and returns complete list with all metadata.
    /// Returns comprehensive profile information that consumers can group/summarize as needed.
    /// </summary>
    /// <returns>List of objects with comprehensive profile information</returns>
    public List<object> GetAvailableProfiles()
    {
        try
        {
            var profilesDirectory = AppDataHelper.GetProfilesDirectory();
            var profiles = new List<object>();

            if (!Directory.Exists(profilesDirectory))
            {
                _logger.LogWarning("Profiles directory does not exist: {ProfilesDirectory}", profilesDirectory);
                return profiles;
            }

            // Scan for all .json files in profiles directory and subdirectories
            var jsonFiles = Directory.GetFiles(profilesDirectory, "*.json", SearchOption.AllDirectories);

            _logger.LogDebug("Found {FileCount} JSON files in profiles directory", jsonFiles.Length);

            foreach (var filePath in jsonFiles)
            {
                try
                {
                    // Get relative path from profiles directory
                    var relativePath = Path.GetRelativePath(profilesDirectory, filePath);
                    
                    // Get directory name (empty string for root profiles directory)
                    var directoryName = Path.GetDirectoryName(relativePath) ?? string.Empty;
                    
                    // Load profile to get metadata
                    var config = _configurationService.LoadProfileConfiguration(filePath);
                    
                    if (config != null)
                    {
                        // Calculate device and mapping counts
                        var deviceCount = config.MidiDevices?.Count ?? 0;
                        var mappingCount = config.MidiDevices?.Sum(d => d.Mappings?.Count ?? 0) ?? 0;

                        var profileInfo = new
                        {
                            Name = Path.GetFileNameWithoutExtension(filePath),
                            Path = relativePath,
                            FullPath = filePath,
                            Directory = directoryName,
                            ProfileName = config.ProfileName ?? "Unnamed Profile",
                            Description = config.Description ?? string.Empty,
                            DeviceCount = deviceCount,
                            MappingCount = mappingCount,
                            LastModified = File.GetLastWriteTime(filePath),
                            FileSize = new FileInfo(filePath).Length
                        };

                        profiles.Add(profileInfo);
                        
                        _logger.LogTrace("Added profile: {ProfileName} ({RelativePath}) - {DeviceCount} devices, {MappingCount} mappings",
                            config.ProfileName, relativePath, deviceCount, mappingCount);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to load profile configuration from {FilePath}", filePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing profile file {FilePath}: {ErrorMessage}", filePath, ex.Message);
                    
                    // Add basic file info even if profile loading failed
                    var relativePath = Path.GetRelativePath(profilesDirectory, filePath);
                    var directoryName = Path.GetDirectoryName(relativePath) ?? string.Empty;
                    
                    var errorProfileInfo = new
                    {
                        Name = Path.GetFileNameWithoutExtension(filePath),
                        Path = relativePath,
                        FullPath = filePath,
                        Directory = directoryName,
                        ProfileName = "Error Loading Profile",
                        Description = $"Error: {ex.Message}",
                        DeviceCount = 0,
                        MappingCount = 0,
                        LastModified = File.GetLastWriteTime(filePath),
                        FileSize = new FileInfo(filePath).Length,
                        HasError = true
                    };
                    
                    profiles.Add(errorProfileInfo);
                }
            }

            _logger.LogInformation("Successfully scanned {ProfileCount} profiles from {ProfilesDirectory}", 
                profiles.Count, profilesDirectory);

            return profiles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available profiles: {ErrorMessage}", ex.Message);
            return new List<object>();
        }
    }

    /// <summary>
    /// Loads profile file and returns as MappingConfig object.
    /// Validates that profile paths are within allowed directories for security.
    /// </summary>
    /// <param name="profilePath">Relative path to profile file (e.g., "examples/keyboard.json")</param>
    /// <returns>MappingConfig object or null if not found or invalid</returns>
    public MappingConfig? GetProfileContent(string profilePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(profilePath))
            {
                _logger.LogWarning("Profile path is null or empty");
                return null;
            }

            var profilesDirectory = AppDataHelper.GetProfilesDirectory();
            var fullPath = Path.Combine(profilesDirectory, profilePath);

            // Security validation: ensure path is within profiles directory
            var normalizedProfilesDir = Path.GetFullPath(profilesDirectory);
            var normalizedFullPath = Path.GetFullPath(fullPath);

            if (!normalizedFullPath.StartsWith(normalizedProfilesDir, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Profile path {ProfilePath} is outside allowed profiles directory", profilePath);
                return null;
            }

            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("Profile file does not exist: {FullPath}", fullPath);
                return null;
            }

            var config = _configurationService.LoadProfileConfiguration(fullPath);
            
            if (config != null)
            {
                _logger.LogDebug("Successfully loaded profile content from {ProfilePath}", profilePath);
            }
            else
            {
                _logger.LogWarning("Failed to load profile content from {ProfilePath}", profilePath);
            }

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile content for {ProfilePath}: {ErrorMessage}", profilePath, ex.Message);
            return null;
        }
    }
}
