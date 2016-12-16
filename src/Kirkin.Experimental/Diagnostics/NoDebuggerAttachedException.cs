using System;

namespace Kirkin.Diagnostics
{
    /// <summary>
    /// Thrown when a debugger is expected to be attached, but isn't.
    /// </summary>
    [Serializable]
    public class NoDebuggerAttachedException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public NoDebuggerAttachedException()
        {
        }

        /// <summary>
        /// Initializes a new instance with the specified message.
        /// </summary>
        public NoDebuggerAttachedException(string message)
            : base(message)
        {
        }
    }
}