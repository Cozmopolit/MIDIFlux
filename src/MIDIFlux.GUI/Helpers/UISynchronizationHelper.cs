using System;
using System.Threading;

namespace MIDIFlux.GUI.Helpers
{
    /// <summary>
    /// Helper class for synchronizing operations with the UI thread
    /// </summary>
    public static class UISynchronizationHelper
    {
        /// <summary>
        /// The synchronization context for the UI thread
        /// </summary>
        private static SynchronizationContext? _uiContext;

        /// <summary>
        /// Initializes the UI synchronization context
        /// </summary>
        /// <param name="context">The synchronization context from the UI thread</param>
        public static void Initialize(SynchronizationContext? context)
        {
            _uiContext = context;
        }

        /// <summary>
        /// Runs an action on the UI thread
        /// </summary>
        /// <param name="action">The action to run</param>
        public static void RunOnUI(Action action)
        {
            if (_uiContext == null)
            {
                throw new InvalidOperationException("UI synchronization context has not been initialized. Call Initialize() first.");
            }

            _uiContext.Post(_ => action(), null);
        }

        /// <summary>
        /// Runs a function on the UI thread and returns its result
        /// </summary>
        /// <typeparam name="T">The return type of the function</typeparam>
        /// <param name="func">The function to run</param>
        /// <returns>The result of the function</returns>
        public static T RunOnUIWithResult<T>(Func<T> func)
        {
            if (_uiContext == null)
            {
                throw new InvalidOperationException("UI synchronization context has not been initialized. Call Initialize() first.");
            }

            T result = default!;
            ManualResetEvent waitHandle = new ManualResetEvent(false);

            _uiContext.Post(_ =>
            {
                try
                {
                    result = func();
                }
                finally
                {
                    waitHandle.Set();
                }
            }, null);

            waitHandle.WaitOne();
            return result;
        }

        /// <summary>
        /// Checks if the current thread is the UI thread
        /// </summary>
        /// <returns>True if the current thread is the UI thread, false otherwise</returns>
        public static bool IsUIThread()
        {
            return SynchronizationContext.Current == _uiContext;
        }
    }
}

