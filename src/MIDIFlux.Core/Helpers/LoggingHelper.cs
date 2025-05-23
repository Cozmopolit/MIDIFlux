using Microsoft.Extensions.Logging;
using System;

namespace MIDIFlux.Core.Helpers
{
    /// <summary>
    /// Helper class for centralized logging functionality
    /// </summary>
    public static class LoggingHelper
    {
        /// <summary>
        /// The central logger factory for the application
        /// </summary>
        private static ILoggerFactory? _centralLoggerFactory;

        // No additional properties needed

        /// <summary>
        /// Sets the central logger factory for the application
        /// </summary>
        /// <param name="loggerFactory">The logger factory to use</param>
        public static void SetCentralLoggerFactory(ILoggerFactory loggerFactory)
        {
            _centralLoggerFactory = loggerFactory;
        }

        /// <summary>
        /// Gets the central logger factory for the application
        /// </summary>
        /// <returns>The central logger factory, or a new console logger factory if not set</returns>
        public static ILoggerFactory GetLoggerFactory()
        {
            // If we have a central logger factory, use it
            if (_centralLoggerFactory != null)
            {
                return _centralLoggerFactory;
            }

            // Otherwise, create a minimal logger factory
            return new LoggerFactory();
        }

        /// <summary>
        /// Creates a logger for the specified type
        /// </summary>
        /// <typeparam name="T">The type to create a logger for</typeparam>
        /// <returns>A logger for the specified type</returns>
        public static ILogger<T> CreateLogger<T>()
        {
            return GetLoggerFactory().CreateLogger<T>();
        }

        /// <summary>
        /// Creates a logger with the specified name
        /// </summary>
        /// <param name="categoryName">The category name for the logger</param>
        /// <returns>A logger with the specified name</returns>
        public static ILogger CreateLogger(string categoryName)
        {
            return GetLoggerFactory().CreateLogger(categoryName);
        }

        /// <summary>
        /// Creates a logger for the specified type
        /// </summary>
        /// <param name="type">The type to create a logger for</param>
        /// <returns>A logger for the specified type</returns>
        public static ILogger CreateLogger(Type type)
        {
            return GetLoggerFactory().CreateLogger(type);
        }

        /// <summary>
        /// Creates a fallback logger when the normal logger creation fails
        /// </summary>
        /// <param name="categoryName">The category name for the logger</param>
        /// <returns>A fallback logger</returns>
        public static ILogger CreateFallbackLogger(string categoryName)
        {
            try
            {
                return CreateLogger(categoryName);
            }
            catch
            {
                // Create a minimal console logger as a last resort
                return LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(categoryName);
            }
        }
    }
}
