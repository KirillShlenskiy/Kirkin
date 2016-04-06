namespace Kirkin.Diff
{
    public abstract class DiffResult
    {
        public bool AreSame { get; }
        public string Message { get; }

        internal DiffResult(bool areSame, string message)
        {
            AreSame = areSame;
            Message = message;
        }

        public void AssertSame()
        {
            if (!AreSame) {
                throw new DiffException(Message);
            }
        }
    }
}