#if !NET_40

using System;

namespace Kirkin
{
    /// <summary>
    /// Utilities for hooking up event handlers that
    /// don't interfere with natural garbage collection.
    /// </summary>
    public static class WeakEvents
    {
        /// <summary>
        /// Hooks up an event handler which does not prevent the dependency from being collected.
        /// </summary>
        public static void HookUpWeakHandlerAction<TDependency>(TDependency dependency, Action<Action> addHandler, Action<Action> removeHandler, Action<TDependency> eventHandler)
            where TDependency : class
        {
            if (dependency == null) throw new ArgumentNullException(nameof(dependency));

            WeakReference<TDependency> weakRef = new WeakReference<TDependency>(dependency);
            Action finalEventHandler = null;

            finalEventHandler = () =>
            {
                if (weakRef.TryGetTarget(out TDependency strongRef))
                {
                    eventHandler(strongRef);
                }
                else
                {
                    removeHandler(finalEventHandler);
                }
            };

            addHandler(finalEventHandler);
        }
    }
}

#endif