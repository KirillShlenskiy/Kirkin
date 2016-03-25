#if !NET_40
using System.Threading.Tasks;
#endif

namespace Kirkin.Functional
{
    /// <summary>
    /// Contract for an executable operation.
    /// </summary>
    internal interface IExecutable
    {
        /// <summary>
        /// Executes the operation.
        /// </summary>
        void Execute();
    }

#if !NET_40
    /// <summary>
    /// Contract for an asynchronous executable operation.
    /// </summary>
    internal interface IAsyncExecutable
    {
        /// <summary>
        /// Executes the asynchronous operation.
        /// </summary>
        /// <remarks>
        /// No CancellationToken support on purpose. If needed, it can easily
        /// be baked into the delegate passed to any of the factory methods.
        /// </remarks>
        Task ExecuteAsync();
    }
#endif
}