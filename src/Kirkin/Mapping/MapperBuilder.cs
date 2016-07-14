using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using Kirkin.Linq.Expressions;
using Kirkin.Mapping.Engine;
using Kirkin.Mapping.Engine.MemberMappings;
using Kirkin.Reflection;

namespace Kirkin.Mapping
{
    /// <summary>
    /// <see cref="MapperBuilder{TSource, TTarget}"/> factory methods.
    /// </summary>
    public static class MapperBuilder
    {
        /// <summary>
        /// Expression-based <see cref="MapperBuilder{TSource, TTarget}"/> factory placeholder.
        /// </summary>
        internal static MapperBuilder<TSource, TTarget> FromExpression<TSource, TTarget>(Expression<Func<TSource, TTarget>> expression)
        {
            throw new NotImplementedException(); // TODO.
        }

        /// <summary>
        /// Creates a new <see cref="MapperBuilder{TSource, TTarget}"/> instance with the
        /// same source and target type, mapping all the properties in the given list.
        /// </summary>
        public static MapperBuilder<T, T> FromPropertyList<T>(PropertyList<T> propertyList)
        {
            if (propertyList == null) throw new ArgumentNullException(nameof(propertyList));

            MapperBuilder<T, T> builder = new MapperBuilder<T, T> {
                MappingMode = MappingMode.Relaxed // No need to validate mapping.
            };

            builder.IgnoreAllTargetMembers();

            foreach (IPropertyAccessor propertyAccessor in propertyList.PropertyAccessors) {
                builder.TargetMember(propertyAccessor.Property.Name).Reset();
            }

            return builder;
        }
    }

    /// <summary>
    /// Type which configures mapping between objects of source and target types.
    /// </summary>
    public partial class MapperBuilder<TSource, TTarget>
    {
        #region Fields and properties

        /// <summary>
        /// Delegates invoked to produce a custom <see cref="MemberMapping{TSource, TTarget}"/> for
        /// the appropriate target member when generating/validating full mapping from source to target.
        /// </summary>
        private readonly Dictionary<Member, Func<MapperBuilder<TSource, TTarget>, MemberMapping<TSource, TTarget>>> CustomTargetMappingFactories
            = new Dictionary<Member, Func<MapperBuilder<TSource, TTarget>, MemberMapping<TSource, TTarget>>>();

        /// <summary>
        /// Source members marked to be ignored.
        /// </summary>
        private readonly HashSet<Member> IgnoredSourceMembers = new HashSet<Member>();

        /// <summary>
        /// Target members marked to be ignored.
        /// </summary>
        private readonly HashSet<Member> IgnoredTargetMembers = new HashSet<Member>();

        /// <summary>
        /// <see cref="MappingMode"/> used when validating the
        /// results of auto-mapping source and target members.
        /// </summary>
        public MappingMode MappingMode { get; set; }

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

        private Member[] _sourceMembers;

        /// <summary>
        /// Source member list.
        /// </summary>
        internal Member[] SourceMembers
        {
            get
            {
                if (_sourceMembers == null) {
                    _sourceMembers = GetSourceMembers();
                }

                return _sourceMembers;
            }
        }

        private Member[] _targetMembers;

        /// <summary>
        /// Target member list.
        /// </summary>
        internal Member[] TargetMembers
        {
            get
            {
                if (_targetMembers == null) {
                    _targetMembers = GetTargetMembers();
                }

                return _targetMembers;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="MapperBuilder{TSource, TTarget}"/> instance.
        /// </summary>
        public MapperBuilder()
        {
            // Defaults.
            MappingMode = MappingMode.Strict;
            MemberNameComparer = StringComparer.Ordinal;
            NullableBehaviour = NullableBehaviour.DefaultMapsToNull;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Creates a <see cref="Mapper{TSource, TTarget}"/> instance
        /// configured accoring to the rules of this builder.
        /// </summary>
        public Mapper<TSource, TTarget> BuildMapper()
        {
            return new Mapper<TSource, TTarget>(ProduceValidMemberMappings());
        }

        /// <summary>
        /// Validates member mapping according to this instance's <see cref="MappingMode"/>.
        /// </summary>
        public void Validate()
        {
            ProduceValidMemberMappings();
        }

        #endregion

        #region Member list

        /// <summary>
        /// Gets the default source member list for this instance.
        /// </summary>
        protected virtual Member[] GetSourceMembers()
        {
            return PropertyMember.PublicInstanceProperties<TSource>();
        }

        /// <summary>
        /// Gets the default target member list for this instance.
        /// </summary>
        protected virtual Member[] GetTargetMembers()
        {
            return PropertyMember.PublicInstanceProperties<TTarget>();
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
            Dictionary<string, Member> sourceMemberDict = BuildMemberDictionary(SourceMembers, IgnoredSourceMembers, MemberNameComparer, mustBeReadable: true);
            Dictionary<string, Member> targetMemberDict = BuildMemberDictionary(TargetMembers, IgnoredTargetMembers, MemberNameComparer, mustBeWriteable: true);

            foreach (Member targetMember in targetMemberDict.Values)
            {
                Func<MapperBuilder<TSource, TTarget>, MemberMapping<TSource, TTarget>> customTargetMappingFactory;

                if (CustomTargetMappingFactories.TryGetValue(targetMember, out customTargetMappingFactory))
                {
                    memberMappings.Add(customTargetMappingFactory(this));
                }
                else
                {
                    Member sourceMember;

                    if (sourceMemberDict.TryGetValue(targetMember.Name, out sourceMember))
                    {
                        memberMappings.Add(new DefaultMemberMapping<TSource, TTarget>(sourceMember, targetMember, NullableBehaviour));
                        sourceMemberDict.Remove(sourceMember.Name); // Saves some validation work down the track.
                    }
                    else if (MappingMode == MappingMode.Strict || MappingMode == MappingMode.AllTargetMembers)
                    {
                        throw new MappingException(
                            $"Entity mapping has failed. The following target member is unmapped: {typeof(TTarget).Name}.{targetMember.Name}."
                        );
                    }
                }
            }

            if (MappingMode == MappingMode.Strict || MappingMode == MappingMode.AllSourceMembers)
            {
                // Inspect generated mappings to see if source members which
                // were not auto-mapped participate in user-defined mappings.
                ParameterExpression param = Expression.Parameter(typeof(TSource), "source");

                foreach (Member sourceMember in sourceMemberDict.Values)
                {
                    bool found = false;

                    if (sourceMember is PropertyMember)
                    {
                        foreach (MemberMapping<TSource, TTarget> mapping in memberMappings)
                        {
                            Expression sourceExpr = mapping.GetSourceValueExpression(param);

                            foreach (PropertyInfo potentialMatch in ExpressionHelpers.ExtractProperties<TSource>(sourceExpr))
                            {
                                if (sourceMember.Equals(new PropertyMember(potentialMatch)))
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
        private static Dictionary<string, Member> BuildMemberDictionary(
            Member[] members,
            HashSet<Member> ignoredMembers,
            StringComparer memberNameComparer,
            bool mustBeReadable = false,
            bool mustBeWriteable = false)
        {
            Dictionary<string, Member> dict = new Dictionary<string, Member>(members.Length, memberNameComparer);

            foreach (Member member in members)
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
            foreach (Member targetMember in TargetMembers)
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
            foreach (Member sourceMember in SourceMembers)
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
            foreach (Member targetMember in TargetMembers)
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
            PropertyMember sourceMemberCandidate = new PropertyMember(sourceMemberProperty);

            // This is less restrictive than source member selection
            // for the purpose of auto mapping. Here we are not checking
            // CanRead, because the member is explicitly provided to us.
            foreach (Member sourceMember in SourceMembers)
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
            PropertyMember targetMemberCandidate = new PropertyMember(targetMemberProperty);

            // This is less restrictive than target member selection
            // for the purpose of auto mapping. Here we are not checking
            // CanWrite, because the member is explicitly provided to us.
            foreach (Member targetMember in TargetMembers)
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