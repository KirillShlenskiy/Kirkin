using System;

namespace Kirkin.CommandLine
{
    public sealed class Arg<T>
    {
        private readonly Func<T> _resolver;

        public T Value
        {
            get
            {
                return _resolver();
            }
        }

        internal Arg(Func<T> resolver)
        {
            _resolver = resolver;
        }
    }
}