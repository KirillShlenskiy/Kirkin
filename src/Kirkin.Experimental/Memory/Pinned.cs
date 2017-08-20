using System;
using System.Runtime.InteropServices;

namespace Kirkin.Memory
{
    /// <summary>
    /// Allows pinning the memory of managed reference types.
    /// </summary>
    internal unsafe struct Pinned : IDisposable
    {
        private readonly GCHandle _handle;

        /// <summary>
        /// Gets the address of the pinned object.
        /// </summary>
        public IntPtr Address
        {
            get
            {
                return _handle.AddrOfPinnedObject();
            }
        }

        /// <summary>
        /// Gets the pointer to the memory location of the pinned object.
        /// </summary>
        public void* Pointer
        {
            get
            {
                return (void*)Address;
            }
        }

        /// <summary>
        /// Pins the given object. The behaviour is undefined if
        /// the object gets boxed as part of this constructor call.
        /// </summary>
        public Pinned(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            _handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
        }

        /// <summary>
        /// Unpins the target object.
        /// </summary>
        public void Dispose()
        {
            if (_handle.IsAllocated) {
                _handle.Free();
            }
        }
    }
}