using System;
using System.Collections.Generic;

namespace Kirkin.Mapping.Fluent
{
    /// <summary>
    /// Partially configure mapper builder factory type.
    /// Participates in fluent mapper builder construction.
    /// </summary>
    public sealed class PartiallyConfiguredMapperBuilder<TSource> // Cannot use struct as we want the constructor to be hidden.
    {
        private readonly Member<TSource>[] SourceMembers;

        internal PartiallyConfiguredMapperBuilder(Member<TSource>[] sourceMembers)
        {
            if (sourceMembers == null) throw new ArgumentNullException(nameof(sourceMembers));

            SourceMembers = sourceMembers;
        }

        /// <summary>
        /// Creates a <see cref="MapperBuilder{TSource, TTarget}"/> which
        /// configures mapping from source to an object of the given type.
        /// </summary>
        public MapperBuilder<TSource, TTarget> ToPublicInstanceProperties<TTarget>()
        {
            Member<TTarget>[] targetMembers = PropertyMember.PublicInstanceProperties<TTarget>();

            return ToMembers(targetMembers);
        }

        /// <summary>
        /// Creates a <see cref="MapperBuilder{TSource, TTarget}"/> which
        /// configures mapping from source to the given members of the target type.
        /// </summary>
        public MapperBuilder<TSource, TTarget> ToMembers<TTarget>(Member<TTarget>[] targetMembers)
        {
            if (targetMembers == null) throw new ArgumentNullException(nameof(targetMembers));

            return new MapperBuilder<TSource, TTarget>(SourceMembers, targetMembers);
        }

        /// <summary>
        /// Creates a mapper builder which configures mapping from source to a generic dictionary.
        /// </summary>
        public MapperBuilder<TSource, Dictionary<string, object>> ToDictionary()
        {
            MemberFactory<Dictionary<string, object>> memberFactory = new MemberFactory<Dictionary<string, object>>();
            Member<Dictionary<string, object>>[] targetMembers = new Member<Dictionary<string, object>>[SourceMembers.Length];
            
            for (int i = 0; i < targetMembers.Length; i++)
            {
                Member<TSource> sourceMember = SourceMembers[i];

                targetMembers[i] = memberFactory.WriteOnlyMember<object>(sourceMember.Name, (dict, value) => dict.Add(sourceMember.Name, value));
            }

            return new MapperBuilder<TSource, Dictionary<string, object>>(SourceMembers, targetMembers);
        }
    }
}