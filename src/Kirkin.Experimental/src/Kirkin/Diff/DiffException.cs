using System;

namespace Kirkin.Diff
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