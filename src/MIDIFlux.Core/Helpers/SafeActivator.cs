using Microsoft.Extensions.Logging;
using System;

namespace MIDIFlux.Core.Helpers
{
    /// <summary>
    /// Utility class for safe object creation and operation execution with consistent error handling and logging.
    /// Eliminates duplicate try-catch-log patterns throughout the application.
    /// </summary>
    public static class SafeActivator
    {
        /// <summary>
        /// Safely creates an instance of the specified type using the parameterless constructor.
        /// </summary>
        /// <typeparam name="T">The type to create (must be a class with parameterless constructor)</typeparam>
        /// <param name="logger">Logger for error reporting</param>
        /// <param name="contextInfo">Optional context information for error messages</param>
        /// <returns>A new instance of T, or null if creation failed</returns>
        public static T? Create<T>(ILogger logger, string? contextInfo = null) where T : class, new()
        {
            try
            {
                return new T();
            }
            catch (Exception ex)
            {
                LogCreationError<T>(logger, ex, contextInfo);
                return null;
            }
        }

        /// <summary>
        /// Safely creates an instance of the specified type using Activator.CreateInstance.
        /// </summary>
        /// <typeparam name="T">The expected return type (must be a class)</typeparam>
        /// <param name="type">The actual type to instantiate</param>
        /// <param name="logger">Logger for error reporting</param>
        /// <param name="contextInfo">Optional context information for error messages</param>
        /// <returns>A new instance of the type cast to T, or null if creation failed</returns>
        public static T? Create<T>(Type type, ILogger logger, string? contextInfo = null) where T : class
        {
            try
            {
                return Activator.CreateInstance(type) as T;
            }
            catch (Exception ex)
            {
                LogCreationError(logger, ex, type.Name, contextInfo);
                return null;
            }
        }

        /// <summary>
        /// Safely creates an instance of the specified type using Activator.CreateInstance with constructor arguments.
        /// </summary>
        /// <typeparam name="T">The expected return type (must be a class)</typeparam>
        /// <param name="type">The actual type to instantiate</param>
        /// <param name="args">Constructor arguments</param>
        /// <param name="logger">Logger for error reporting</param>
        /// <param name="contextInfo">Optional context information for error messages</param>
        /// <returns>A new instance of the type cast to T, or null if creation failed</returns>
        public static T? Create<T>(Type type, object[] args, ILogger logger, string? contextInfo = null) where T : class
        {
            try
            {
                return Activator.CreateInstance(type, args) as T;
            }
            catch (Exception ex)
            {
                LogCreationError(logger, ex, type.Name, contextInfo);
                return null;
            }
        }

        /// <summary>
        /// Safely executes an operation and returns its result, with fallback value on failure.
        /// </summary>
        /// <typeparam name="T">The return type of the operation</typeparam>
        /// <param name="operation">The operation to execute</param>
        /// <param name="logger">Logger for error reporting</param>
        /// <param name="operationName">Name of the operation for error messages</param>
        /// <param name="fallback">Fallback value to return on failure (default: default(T))</param>
        /// <returns>The operation result, or fallback value if operation failed</returns>
        public static T? Execute<T>(Func<T> operation, ILogger logger, string operationName, T? fallback = default)
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                LogExecutionError(logger, ex, operationName);
                return fallback;
            }
        }

        /// <summary>
        /// Safely executes an operation and returns its result, with fallback factory on failure.
        /// </summary>
        /// <typeparam name="T">The return type of the operation</typeparam>
        /// <param name="operation">The operation to execute</param>
        /// <param name="logger">Logger for error reporting</param>
        /// <param name="operationName">Name of the operation for error messages</param>
        /// <param name="fallbackFactory">Factory function to create fallback value on failure</param>
        /// <returns>The operation result, or result of fallback factory if operation failed</returns>
        public static T? Execute<T>(Func<T> operation, ILogger logger, string operationName, Func<T> fallbackFactory)
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                LogExecutionError(logger, ex, operationName);
                try
                {
                    return fallbackFactory();
                }
                catch (Exception fallbackEx)
                {
                    LogExecutionError(logger, fallbackEx, $"{operationName} fallback");
                    return default;
                }
            }
        }

        /// <summary>
        /// Safely executes a void operation.
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        /// <param name="logger">Logger for error reporting</param>
        /// <param name="operationName">Name of the operation for error messages</param>
        /// <returns>True if operation succeeded, false if it failed</returns>
        public static bool Execute(Action operation, ILogger logger, string operationName)
        {
            try
            {
                operation();
                return true;
            }
            catch (Exception ex)
            {
                LogExecutionError(logger, ex, operationName);
                return false;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Logs object creation errors with consistent format and safe error handling.
        /// </summary>
        private static void LogCreationError<T>(ILogger logger, Exception ex, string? contextInfo)
        {
            LogCreationError(logger, ex, typeof(T).Name, contextInfo);
        }

        /// <summary>
        /// Logs object creation errors with consistent format and safe error handling.
        /// </summary>
        private static void LogCreationError(ILogger logger, Exception ex, string typeName, string? contextInfo)
        {
            try
            {
                var message = string.IsNullOrEmpty(contextInfo)
                    ? "Failed to create instance of type {TypeName}: {Message}"
                    : "Failed to create instance of type {TypeName} ({Context}): {Message}";

                if (string.IsNullOrEmpty(contextInfo))
                {
                    logger.LogError(ex, message, typeName, ex.Message);
                }
                else
                {
                    logger.LogError(ex, message, typeName, contextInfo, ex.Message);
                }
            }
            catch
            {
                // If logging fails, we can't do much - just continue
                // This matches the existing pattern in ActionTypeRegistry
            }
        }

        /// <summary>
        /// Logs operation execution errors with consistent format and safe error handling.
        /// </summary>
        private static void LogExecutionError(ILogger logger, Exception ex, string operationName)
        {
            try
            {
                logger.LogError(ex, "Failed to execute operation {OperationName}: {Message}", operationName, ex.Message);
            }
            catch
            {
                // If logging fails, we can't do much - just continue
                // This matches the existing pattern in ActionTypeRegistry
            }
        }

        #endregion
    }
}
