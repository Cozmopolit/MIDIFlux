using Microsoft.Extensions.Logging;
using System.Reflection;

namespace MIDIFlux.Core.Helpers
{
    /// <summary>
    /// Helper class for managing example profile files embedded as resources
    /// </summary>
    public static class ExampleProfileHelper
    {
        /// <summary>
        /// Gets the example profiles directory path
        /// </summary>
        /// <returns>The path to the example profiles directory</returns>
        public static string GetExampleProfilesDirectory()
        {
            return Path.Combine(AppDataHelper.GetProfilesDirectory(), "example");
        }

        /// <summary>
        /// Creates the example profiles directory and copies embedded example files
        /// This should only be called during first-time setup
        /// </summary>
        /// <param name="logger">The logger to use</param>
        public static void EnsureExampleProfilesExist(ILogger logger)
        {
            try
            {
                string exampleDir = GetExampleProfilesDirectory();

                // Create the example directory
                Directory.CreateDirectory(exampleDir);
                logger.LogInformation("Created example profiles directory: {ExampleDir}", exampleDir);

                // Get all embedded example resources from the MIDIFlux.App assembly
                var appAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                var resourceNames = appAssembly.GetManifestResourceNames()
                    .Where(name => name.StartsWith("Examples.") && name.EndsWith(".json"))
                    .ToArray();

                logger.LogDebug("Found {Count} embedded example resources", resourceNames.Length);

                foreach (var resourceName in resourceNames)
                {
                    // Extract the filename from the resource name (Examples.example-basic-keys.json -> example-basic-keys.json)
                    var fileName = resourceName.Substring("Examples.".Length);
                    var filePath = Path.Combine(exampleDir, fileName);

                    try
                    {
                        using var stream = appAssembly.GetManifestResourceStream(resourceName);
                        if (stream != null)
                        {
                            using var reader = new StreamReader(stream);
                            var content = reader.ReadToEnd();
                            File.WriteAllText(filePath, content);
                            logger.LogInformation("Copied example profile: {FileName}", fileName);
                        }
                        else
                        {
                            logger.LogWarning("Could not read embedded resource: {ResourceName}", resourceName);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to copy example profile {FileName}: {ErrorMessage}", fileName, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error ensuring example profiles exist: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Gets a list of all available example profile files
        /// </summary>
        /// <returns>An array of file paths to example profiles</returns>
        public static string[] GetAvailableExampleProfiles()
        {
            try
            {
                string exampleDir = GetExampleProfilesDirectory();
                if (Directory.Exists(exampleDir))
                {
                    return Directory.GetFiles(exampleDir, "*.json", SearchOption.TopDirectoryOnly);
                }
            }
            catch (Exception)
            {
                // Ignore errors and return empty array
            }

            return Array.Empty<string>();
        }

        /// <summary>
        /// Gets the list of embedded example resource names
        /// </summary>
        /// <returns>An array of embedded resource names</returns>
        public static string[] GetEmbeddedExampleResourceNames()
        {
            try
            {
                var appAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                return appAssembly.GetManifestResourceNames()
                    .Where(name => name.StartsWith("Examples.") && name.EndsWith(".json"))
                    .ToArray();
            }
            catch (Exception)
            {
                return Array.Empty<string>();
            }
        }
    }
}
