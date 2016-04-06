using System;

namespace Kirkin.Diff
{
    [Serializable]
    internal sealed class DiffException : Exception
    {
        public IDiffResult Result { get; }

        internal DiffException(IDiffResult result)
            : base(result.Message)
        {
            Result = result;
        }
    }
}