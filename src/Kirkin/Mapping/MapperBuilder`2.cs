using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using Kirkin.Linq.Expressions;
using Kirkin.Mapping.Engine;
using Kirkin.Mapping.Engine.MemberMappings;

namespace Kirkin.Mapping
{
    /// <summary>
    /// Type which configures mapping between objects of source and target types.
    /// </summary>
    public sealed partial class MapperBuilder<TSource, TTarget>
    {
        #region Fields and properties

        /// <summary>
        /// Source member list.
        /// </summary>
        internal readonly Member<TSource>[] SourceMembers;

        /// <summary>
        /// Target member list.
        /// </summary>
        internal readonly Member<TTarget>[] TargetMembers;

        /// <summary>
        /// Source members marked to be ignored.
        /// </summary>
        private readonly HashSet<Member<TSource>> IgnoredSourceMembers = new HashSet<Member<TSource>>();

        /// <summary>
        /// Target members marked to be ignored.
        /// </summary>
        private readonly HashSet<Member<TTarget>> IgnoredTargetMembers = new HashSet<Member<TTarget>>();

        /// <summary>
        /// Delegates invoked to produce a custom <see cref="MemberMapping{TSource, TTarget}"/> for
        /// the appropriate target member when generating/validating full mapping from source to target.
        /// </summary>
        private readonly Dictionary<Member<TTarget>, Func<MapperBuilder<TSource, TTarget>, MemberMapping<TSource, TTarget>>> CustomTargetMappingFactories
            = new Dictionary<Member<TTarget>, Func<MapperBuilder<TSource, TTarget>, MemberMapping<TSource, TTarget>>>();

        /// <summary>
        /// Gets or sets the value that determines whether an exception
        /// will be thrown if there are unmapped source members when the
        /// result of auto-mapping source and target members is validated.
        /// </summary>
        public bool MapAllSourceMembers { get; set; }

        /// <summary>
        /// Gets or sets the value that determines whether an exception
        /// will be thrown if there are unmapped source members when the
        /// result of auto-mapping source and target members is validated.
        /// </summary>
        public bool MapAllTargetMembers { get; set; }

        /// <summary>
        /// <see cref="StringComparer"/> used when comparing
        /// member names for equality for the purpose of auto-mapping.
        /// </summary>
        public StringComparer MemberNameComparer { get; set; }

        /// <summary>
        /// Determines the behaviour of the mapper when performing
        /// nullable to non-nullable (and vice versa) value conversions.
        /// </summary>
        public NullableBehaviour NullableBehaviour { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="MapperBuilder{TSource, TTarget}"/>
        /// instance using the given source and target member lists.
        /// By default will use the list of public properties defined by source and target types.
        /// </summary>
        internal MapperBuilder(Member<TSource>[] sourceMembers = null, Member<TTarget>[] targetMembers = null)
        {
            SourceMembers = sourceMembers ?? PropertyMember<TSource>.PublicInstanceProperties();
            TargetMembers = targetMembers ?? PropertyMember<TTarget>.PublicInstanceProperties();

            // Defaults.
            MapAllSourceMembers = true;
            MapAllTargetMembers = true;
            MemberNameComparer = StringComparer.Ordinal;
            NullableBehaviour = NullableBehaviour.DefaultMapsToNull;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Creates an immutable <see cref="Mapper{TSource, TTarget}"/>
        /// instance configured accoring to the rules defined by this builder.
        /// </summary>
        public Mapper<TSource, TTarget> BuildMapper()
        {
            return new Mapper<TSource, TTarget>(ProduceValidMemberMappings());
        }

        /// <summary>
        /// Validates member mapping according to this instance's configuration.
        /// </summary>
        internal void ValidateMapping()
        {
            ProduceValidMemberMappings();
        }

        #endregion

        #region Auto mapping

        /// <summary>
        /// Returns a validated collection of member mappings
        /// created according to the rules defined by this instance.
        /// </summary>
        internal MemberMapping<TSource, TTarget>[] ProduceValidMemberMappings()
        {
            // Produce member map.
            List<MemberMapping<TSource, TTarget>> memberMappings = new List<MemberMapping<TSource, TTarget>>(TargetMembers.Length);
            Dictionary<string, Member<TSource>> sourceMemberDict = BuildMemberDictionary(SourceMembers, IgnoredSourceMembers, MemberNameComparer, mustBeReadable: true);
            Dictionary<string, Member<TTarget>> targetMemberDict = BuildMemberDictionary(TargetMembers, IgnoredTargetMembers, MemberNameComparer, mustBeWriteable: true);

            foreach (Member<TTarget> targetMember in targetMemberDict.Values)
            {
                Func<MapperBuilder<TSource, TTarget>, MemberMapping<TSource, TTarget>> customTargetMappingFactory;

                if (CustomTargetMappingFactories.TryGetValue(targetMember, out customTargetMappingFactory))
                {
                    memberMappings.Add(customTargetMappingFactory(this));
                }
                else
                {
                    Member<TSource> sourceMember;

                    if (sourceMemberDict.TryGetValue(targetMember.Name, out sourceMember))
                    {
                        memberMappings.Add(new DefaultMemberMapping<TSource, TTarget>(sourceMember, targetMember, NullableBehaviour));
                        sourceMemberDict.Remove(sourceMember.Name); // Saves some validation work down the track.
                    }
                    else if (MapAllTargetMembers)
                    {
                        throw new MappingException(
                            $"Entity mapping has failed. The following target member is unmapped: {typeof(TTarget).Name}.{targetMember.Name}."
                        );
                    }
                }
            }

            if (MapAllSourceMembers)
            {
                // Inspect generated mappings to see if source members which
                // were not auto-mapped participate in user-defined mappings.
                ParameterExpression param = Expression.Parameter(typeof(TSource), "source");

                foreach (Member<TSource> sourceMember in sourceMemberDict.Values)
                {
                    bool found = false;

                    if (sourceMember is PropertyMember<TSource>)
                    {
                        foreach (MemberMapping<TSource, TTarget> mapping in memberMappings)
                        {
                            Expression sourceExpr = mapping.GetSourceValueExpression(param);

                            foreach (PropertyInfo potentialMatch in ExpressionHelpers.ExtractProperties<TSource>(sourceExpr))
                            {
                                if (sourceMember.Equals(new PropertyMember<TSource>(potentialMatch)))
                                {
                                    found = true;
                                    break;
                                }
                            }

                            if (found) break;
                        }
                    }

                    if (!found)
                    {
                        throw new MappingException(
                            $"Entity mapping has failed. The following source member is unmapped: {typeof(TSource).Name}.{sourceMember.Name}."
                        );
                    }
                }
            }

            return memberMappings.ToArray();
        }
        
        /// <summary>
        /// Creates member dictionaries used for auto mapping.
        /// </summary>
        private static Dictionary<string, Member<T>> BuildMemberDictionary<T>(
            Member<T>[] members,
            HashSet<Member<T>> ignoredMembers,
            StringComparer memberNameComparer,
            bool mustBeReadable = false,
            bool mustBeWriteable = false)
        {
            Dictionary<string, Member<T>> dict = new Dictionary<string, Member<T>>(members.Length, memberNameComparer);

            foreach (Member<T> member in members)
            {
                if (ignoredMembers.Contains(member)) {
                    continue;
                }

                // Ambiguous members are not allowed even if they
                // will be eliminated by CanRead/CanWrite check.
                // This may be relaxed later if deemed safe.
                if (dict.ContainsKey(member.Name)) {
                    throw new MappingException($"Ambiguous member: {member.Name}.");
                }

                if ((!mustBeReadable || member.CanRead) && (!mustBeWriteable || member.CanWrite)) {
                    dict.Add(member.Name, member);
                }
            }

            return dict;
        }

        internal void IgnoreAllTargetMembers()
        {
            foreach (Member<TTarget> targetMember in TargetMembers)
            {
                IgnoredTargetMembers.Add(targetMember);
                CustomTargetMappingFactories.Remove(targetMember);
            }
        }

        static class ExpressionHelpers
        {
            /// <summary>
            /// Extracts all properties defined on the given type that are read by the expression.
            /// </summary>
            public static IEnumerable<PropertyInfo> ExtractProperties<T>(Expression expression)
            {
                PropertyExtractVisitor<T> extractor = new PropertyExtractVisitor<T>();

                extractor.Visit(expression);

                return extractor.Properties;
            }

            sealed class PropertyExtractVisitor<T> : ExpressionVisitor
            {
                internal readonly HashSet<PropertyInfo> Properties = new HashSet<PropertyInfo>();

                protected override Expression VisitMember(MemberExpression node)
                {
                    if (node.NodeType == ExpressionType.MemberAccess)
                    {
                        MemberInfo member = node.Member;

                        if (member.MemberType == MemberTypes.Property && member.DeclaringType.IsAssignableFrom(typeof(T))) {
                            Properties.Add((PropertyInfo)member);
                        }
                    }

                    return base.VisitMember(node);
                }
            }
        }

        #endregion

        #region Fluent API

        /// <summary>
        /// Returns a mapping configuration object for the member with the
        /// given name using this instance's <see cref="MemberNameComparer"/>.
        /// </summary>
        public SourceMemberConfig SourceMember(string name)
        {
            return SourceMember(name, MemberNameComparer);
        }

        /// <summary>
        /// Returns a mapping configuration object for the
        /// member with the given name using the given comparer.
        /// </summary>
        public SourceMemberConfig SourceMember(string name, StringComparer nameComparer)
        {
            foreach (Member<TSource> sourceMember in SourceMembers)
            {
                if (nameComparer.Equals(name, sourceMember.Name)) {
                    return new SourceMemberConfig(this, sourceMember);
                }
            }

            throw new MappingException($"Member {typeof(TSource).Name}.{name} cannot be mapped by this instance.");
        }

        /// <summary>
        /// Returns a mapping configuration object for the member with the
        /// given name using this instance's <see cref="MemberNameComparer"/>.
        /// </summary>
        public TargetMemberConfig TargetMember(string name)
        {
            return TargetMember(name, MemberNameComparer);
        }

        /// <summary>
        /// Returns a mapping configuration object for the
        /// member with the given name using the given comparer.
        /// </summary>
        public TargetMemberConfig TargetMember(string name, StringComparer nameComparer)
        {
            foreach (Member<TTarget> targetMember in TargetMembers)
            {
                if (nameComparer.Equals(name, targetMember.Name)) {
                    return new TargetMemberConfig(this, targetMember);
                }
            }

            throw new MappingException($"Member {typeof(TTarget).Name}.{name} cannot be mapped by this instance.");
        }

        /// <summary>
        /// Returns a mapping configuration object for the given member.
        /// </summary>
        public SourceMemberConfig SourceMember<TValue>(Expression<Func<TSource, TValue>> sourceMemberSelector)
        {
            // Only supporting property-based Members.
            PropertyInfo sourceMemberProperty = ExpressionUtil.Property(sourceMemberSelector);
            PropertyMember<TSource> sourceMemberCandidate = new PropertyMember<TSource>(sourceMemberProperty);

            // This is less restrictive than source member selection
            // for the purpose of auto mapping. Here we are not checking
            // CanRead, because the member is explicitly provided to us.
            foreach (Member<TSource> sourceMember in SourceMembers)
            {
                if (sourceMemberCandidate.Equals(sourceMember)) {
                    return new SourceMemberConfig(this, sourceMember);
                }
            }

            throw new MappingException($"Member {typeof(TSource).Name}.{sourceMemberProperty.Name} cannot be mapped by this instance.");
        }

        /// <summary>
        /// Returns a mapping configuration object for the given member.
        /// </summary>
        public TargetMemberConfig<TValue> TargetMember<TValue>(Expression<Func<TTarget, TValue>> targetMemberSelector)
        {
            // Only supporting property-based Members.
            PropertyInfo targetMemberProperty = ExpressionUtil.Property(targetMemberSelector);
            PropertyMember<TTarget> targetMemberCandidate = new PropertyMember<TTarget>(targetMemberProperty);

            // This is less restrictive than target member selection
            // for the purpose of auto mapping. Here we are not checking
            // CanWrite, because the member is explicitly provided to us.
            foreach (Member<TTarget> targetMember in TargetMembers)
            {
                if (targetMemberCandidate.Equals(targetMember)) {
                    return new TargetMemberConfig<TValue>(this, targetMember);
                }
            }

            throw new MappingException($"Member {typeof(TTarget).Name}.{targetMemberProperty.Name} cannot be mapped by this instance.");
        }

        #endregion
    }
}