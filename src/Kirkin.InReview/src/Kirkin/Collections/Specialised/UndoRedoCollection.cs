using System;
using System.Collections;
using System.Collections.Generic;

namespace Kirkin.Collections.Specialised
{
    /// <summary>
    /// Unoptimised undo-redo collection similar to a
    /// linked list or a stack which remembers its popped
    /// items and is capable of re-adding them.
    /// </summary>
    public class UndoRedoCollection<T> : IEnumerable<T>
    {
        private readonly List<T> Items = new List<T>();
        private int _capacity = 0; // 0 means unlimited.

        /// <summary>
        /// Returns true if there are items on the undo stack.
        /// </summary>
        public bool CanUndo
        {
            get
            {
                return Index >= 0;
            }
        }

        /// <summary>
        /// Returns true if there are items on the redo stack.
        /// </summary>
        public bool CanRedo
        {
            get
            {
                return Index < Count - 1;
            }
        }

        /// <summary>
        /// Gets or sets the capacity of this instance.
        /// Zero means that the collection can grow indefinitely.
        /// </summary>
        public int Capacity
        {
            get
            {
                return _capacity;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException();

                _capacity = value;

                TrimToCapacity();
            }
        }

        /// <summary>
        /// Numer of items in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return Items.Count;
            }
        }

        /// <summary>
        /// Returns the current item or throws
        /// if the collection is empty.
        /// </summary>
        public T Current
        {
            get
            {
                if (Index < 0)
                {
                    throw new InvalidOperationException("Current is undefined when the Index is negative.");
                }

                return Items[Index];
            }
        }

        /// <summary>
        /// Current item index.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Creates a new instance of undo-redo collection.
        /// </summary>
        public UndoRedoCollection()
        {
            Index = -1;
        }
        
        /// <summary>
        /// Clears the collection.
        /// </summary>
        public void Clear()
        {
            Index = -1;
            Items.Clear();
        }

        /// <summary>
        /// Rolls back the last add operation.
        /// </summary>
        public void Undo()
        {
            if (!CanUndo) throw new InvalidOperationException();

            Index--;
        }

        /// <summary>
        /// Replays the next add operation.
        /// </summary>
        public void Redo()
        {
            if (!CanRedo) throw new InvalidOperationException();

            Index++;
        }

        /// <summary>
        /// Adds the given item to the collection after the
        /// current item destroying any existing redo entries.
        /// </summary>
        public void Add(T value)
        {
            int newIndex = Index + 1;

            if (newIndex == Items.Count)
            {
                // There is nothing on the redo
                // stack. Just add the new item.
                Items.Add(value);

                Index = newIndex;

                TrimToCapacity();
            }
            else
            {
                // Replace item at next index.
                Items[newIndex] = value;

                if (newIndex < Items.Count - 1)
                {
                    // Truncate the redo portion.
                    Items.RemoveRange(newIndex + 1, Items.Count - (newIndex + 1));
                }

                Index = newIndex;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Trims excess items at the beginning of the list if necessary.
        /// </summary>
        private void TrimToCapacity()
        {
            if (Capacity == 0)
                return;

            if (Items.Count > Capacity)
            {
                int removeCount = Items.Count - Capacity;

                Index -= removeCount;
                Items.RemoveRange(0, removeCount);
            }
        }
    }
}