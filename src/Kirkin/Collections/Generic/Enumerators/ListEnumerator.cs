// *NOT* original work by Kirill Shlenskiy.
// Copied from Microsoft's open-sourced System.Collections.Immutable.ImmutableArray<T>.Enumerator and adapted for our purposes.
using System.Collections.Generic;

namespace Kirkin.Collections.Generic.Enumerators
{
    /// <summary>
    /// List enumerator stuct.
    /// </summary>
    /// <remarks>
    /// It is important that this enumerator does NOT implement <see cref="System.IDisposable"/>.
    /// We want the iterator to inline when we do foreach and to not result in
    /// a try/finally frame in the client.
    /// </remarks>
    public struct ListEnumerator<T>
    {
        /// <summary> 
        /// The list being enumerated.
        /// </summary>
        private readonly List<T> _list;

        /// <summary>
        /// The currently enumerated position.
        /// </summary>
        /// <value>
        /// -1 before the first call to <see cref="MoveNext"/>.
        /// >= this.list.Count after <see cref="MoveNext"/> returns false.
        /// </value>
        private int _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListEnumerator{T}"/> struct.
        /// </summary>
        /// <param name="list">The list to enumerate.</param>
        internal ListEnumerator(List<T> list)
        {
            _list = list;
            _index = -1;
        }

        /// <summary>
        /// Gets the currently enumerated value.
        /// </summary>
        public T Current
        {
            get
            {
                // PERF: no need to do a range check, we already did in MoveNext.
                // if user did not call MoveNext or ignored its result (incorrect use)
                // he will still get an exception from the list access range check.
                return _list[_index];
            }
        }

        /// <summary>
        /// Advances to the next value to be enumerated.
        /// </summary>
        /// <returns><c>true</c> if another item exists in the list; <c>false</c> otherwise.</returns>
        public bool MoveNext()
        {
            return ++_index < _list.Count;
        }
    }
}