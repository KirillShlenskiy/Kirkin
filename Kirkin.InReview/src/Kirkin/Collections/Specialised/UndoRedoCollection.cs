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
                return this.Index >= 0;
            }
        }

        /// <summary>
        /// Returns true if there are items on the redo stack.
        /// </summary>
        public bool CanRedo
        {
            get
            {
                return this.Index < this.Count - 1;
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

                this.TrimToCapacity();
            }
        }

        /// <summary>
        /// Numer of items in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return this.Items.Count;
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
                if (this.Index < 0)
                {
                    throw new InvalidOperationException("Current is undefined when the Index is negative.");
                }

                return this.Items[this.Index];
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
            this.Index = -1;
        }
        
        /// <summary>
        /// Clears the collection.
        /// </summary>
        public void Clear()
        {
            this.Index = -1;
            this.Items.Clear();
        }

        /// <summary>
        /// Rolls back the last add operation.
        /// </summary>
        public void Undo()
        {
            if (!this.CanUndo) throw new InvalidOperationException();

            this.Index--;
        }

        /// <summary>
        /// Replays the next add operation.
        /// </summary>
        public void Redo()
        {
            if (!this.CanRedo) throw new InvalidOperationException();

            this.Index++;
        }

        /// <summary>
        /// Adds the given item to the collection after the
        /// current item destroying any existing redo entries.
        /// </summary>
        public void Add(T value)
        {
            int newIndex = this.Index + 1;

            if (newIndex == this.Items.Count)
            {
                // There is nothing on the redo
                // stack. Just add the new item.
                this.Items.Add(value);

                this.Index = newIndex;

                this.TrimToCapacity();
            }
            else
            {
                // Replace item at next index.
                this.Items[newIndex] = value;

                if (newIndex < this.Items.Count - 1)
                {
                    // Truncate the redo portion.
                    this.Items.RemoveRange(newIndex + 1, this.Items.Count - (newIndex + 1));
                }

                this.Index = newIndex;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Trims excess items at the beginning of the list if necessary.
        /// </summary>
        private void TrimToCapacity()
        {
            if (this.Capacity == 0)
                return;

            if (this.Items.Count > this.Capacity)
            {
                int removeCount = this.Items.Count - this.Capacity;

                this.Index -= removeCount;
                this.Items.RemoveRange(0, removeCount);
            }
        }
    }
}