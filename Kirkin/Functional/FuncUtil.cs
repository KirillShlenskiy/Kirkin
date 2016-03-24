using System;

#if NET_45
using System.Threading.Tasks;
#endif

namespace Kirkin.Functional
{
    /// <summary>
    /// Common functional utility methods.
    /// </summary>
    public static class FuncUtil
    {
        /// <summary>
        /// Allows using type inference when
        /// needing to pass an instance of an
        /// anonymous type to the outside scope.
        /// </summary>
        public static T Return<T>(Func<T> func)
        {
            if (func == null) throw new ArgumentNullException("func");

            return func();
        }

        /// <summary>
        /// Invokes the given delegate passing in
        /// the disposable resource as its argument,
        /// and ensures that the disposable resource
        /// is disposed after the delegate is invoked.
        /// </summary>
        public static TReturn Using<TDisposable, TReturn>(TDisposable disposable, Func<TDisposable, TReturn> func)
            where TDisposable : IDisposable
        {
            // Null "disposable" arg allowed.
            if (func == null) throw new ArgumentNullException("func");

            try
            {
                return func(disposable);
            }
            finally
            {
                if (disposable != null) {
                    disposable.Dispose();
                }
            }
        }

#if NET_45
        /// <summary>
        /// Invokes the given delegate passing in
        /// the disposable resource as its argument,
        /// and ensures that the disposable resource
        /// is disposed after the delegate is invoked.
        /// </summary>
        public static async Task<TReturn> UsingAsync<TDisposable, TReturn>(TDisposable disposable, Func<TDisposable, Task<TReturn>> func)
            where TDisposable : IDisposable
        {
            // Null "disposable" arg allowed.
            if (func == null) throw new ArgumentNullException("func");

            try
            {
                return await func(disposable).ConfigureAwait(false);
            }
            finally
            {
                if (disposable != null) {
                    disposable.Dispose();
                }
            }
        }
#endif
    }
}