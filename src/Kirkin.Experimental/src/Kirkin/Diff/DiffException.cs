using System;

namespace Kirkin.src.Kirkin.Diff
{
    [Serializable]
    public sealed class DiffException : Exception
    {
        internal DiffException(string message)
            : base(message)
        {
        }
    }
}