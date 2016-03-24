using System.Collections.Generic;

namespace Kirkin.Linq
{
    /// <summary>
    /// Common collection filter contract.
    /// </summary>
    /// <remarks>
    /// The reason for having this interface is composing complex
    /// queries with multipart Where clauses. The interface definition
    /// ensures that filters are composable and their implementations remain inheritance-friendly.
    /// </remarks>
    public interface ICollectionFilter<T>
    {
        /// <summary>
        /// Returns the items which satisfy the filter condition.
        /// </summary>
        IEnumerable<T> Filter(IEnumerable<T> collection);
    }
}