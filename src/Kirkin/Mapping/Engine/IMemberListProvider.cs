using Kirkin.Mapping.Engine;

namespace Kirkin.Mapping
{
    /// <summary>
    /// Contract for types which resolve collections of
    /// <see cref="Member"/> instances appropriate for the given type.
    /// </summary>
    internal interface IMemberListProvider<T>
    {
        /// <summary>
        /// Resolves the collection of <see cref="Member"/> instances appropriate for the given type.
        /// </summary>
        Member<T>[] GetMembers();
    }
}