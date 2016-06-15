using System;

namespace Kirkin.Mapping
{
    /// <summary>
    /// Exception thrown when mapping is unsuccessful.
    /// </summary>
#if !__MOBILE__
    [Serializable]
#endif
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