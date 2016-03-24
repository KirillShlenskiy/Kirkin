using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Kirkin.Collections.Generic.Enumerators;

namespace Kirkin.Mapping.Engine
{
    /// <summary>
    /// Lightweight collection of <see cref="Member"/> objects accessible by name.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal struct MemberCollection : IEnumerable<Member>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private readonly Member[] Members;

        /// <summary>
        /// Gets the number of members in the collection.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public int Count
        {
            get
            {
                return Members.Length;
            }
        }

        /// <summary>
        /// Gets the string to display in the debugger watches window for this instance.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get
            {
                return $"{{{typeof(Member).FullName}[{Count}]}}";
            }
        }

        /// <summary>
        /// Returns the member at the given index.
        /// </summary>
        public Member this[int index]
        {
            get
            {
                return Members[index];
            }
        }

        /// <summary>
        /// Returns the member with the given name. Case-sensitive.
        /// </summary>
        public Member this[string name]
        {
            get
            {
                foreach (Member member in Members)
                {
                    if (member.Name == name) {
                        return member;
                    }
                }

                throw new InvalidOperationException($"Cannot find member '{name}'.");
            }
        }

        /// <summary>
        /// Creates a new <see cref="MemberCollection"/> instance.
        /// </summary>
        internal MemberCollection(Member[] members)
        {
            Members = members;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public ArrayEnumerator<Member> GetEnumerator()
        {
            return new ArrayEnumerator<Member>(Members);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        IEnumerator<Member> IEnumerable<Member>.GetEnumerator()
        {
            return Members.AsEnumerable().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Members.GetEnumerator();
        }
    }
}