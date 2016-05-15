using System;

namespace Kirkin.Mapping
{
    /// <summary>
    /// Exception thrown when mapping is unsuccessful.
    /// </summary>
    [Serializable]
    public class MappingException : Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="MappingException"/>.
        /// </summary>
        public MappingException(string message)
            : base(message)
        {
        }
    }
}