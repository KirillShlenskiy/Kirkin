namespace Kirkin.src.Kirkin.Diff
{
    public abstract class DiffResult
    {
        public bool AreSame { get; }
        public string DiffMessage { get; }

        internal DiffResult(bool areSame, string diffMessage)
        {
            AreSame = areSame;
            DiffMessage = diffMessage;
        }

        public void AssertSame()
        {
            if (!AreSame) {
                throw new DiffException(DiffMessage);
            }
        }
    }
}