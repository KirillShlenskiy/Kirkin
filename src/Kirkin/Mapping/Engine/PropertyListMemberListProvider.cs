using System;

using Kirkin.Reflection;

namespace Kirkin.Mapping.Engine
{
    internal sealed class PropertyListMemberListProvider<T>
        : IMemberListProvider<T>
    {
        public PropertyList<T> PropertyList { get; }

        public PropertyListMemberListProvider(PropertyList<T> propertyList)
        {
            if (propertyList == null) throw new ArgumentNullException(nameof(propertyList));

            PropertyList = propertyList;
        }

        public Member<T>[] GetMembers()
        {
            return PropertyMember<T>.MembersFromPropertyList(PropertyList);
        }
    }
}
