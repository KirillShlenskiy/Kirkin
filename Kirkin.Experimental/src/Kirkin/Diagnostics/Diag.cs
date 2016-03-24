using System.Diagnostics;

namespace Kirkin.Diagnostics
{
    /// <summary>
    /// Diagnostic tools.
    /// </summary>
    public static class Diag
    {
        /// <summary>
        /// If a debugger is attached, causes it to break so that
        /// the code can be stepped through. Otherwise throws.
        /// </summary>
        [DebuggerNonUserCode]
        public static void BreakOrThrow()
        {
            if (!Debugger.IsAttached) {
                throw new NoDebuggerAttachedException();
            }

            Debugger.Break();
        }

        /// <summary>
        /// If a debugger is attached, causes it to break so that
        /// the code can be stepped through. Otherwise throws.
        /// </summary>
        [DebuggerNonUserCode]
        public static void BreakOrThrow(string message)
        {
            if (!Debugger.IsAttached) {
                throw new NoDebuggerAttachedException(message);
            }

            Debugger.Break();
        }
    }
}