using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Kirkin.Mapping.Engine.Compilers
{
    /// <summary>
    /// Type responsible for producing compiled mapping delegates.
    /// </summary>
    internal class MappingCompiler<TSource, TTarget>
    {
        /// <summary>
        /// Returns a compiled delegate which performs the mapping from source to target.
        /// </summary>
        public virtual Func<TSource, TTarget, TTarget> CompileMapping(MemberMapping<TSource, TTarget>[] memberMappings)
        {
            Debug.Print($"Compiling mapping from {typeof(TSource).Name} to {typeof(TTarget).Name}");

            return CreateMappingExpression(memberMappings).Compile();
        }

        /// <summary>
        /// Produces the expression tree which describes mapping from source
        /// to target object according to the rules of this instance.
        /// </summary>
        private Expression<Func<TSource, TTarget, TTarget>> CreateMappingExpression(MemberMapping<TSource, TTarget>[] memberMappings)
        {
            ParameterExpression sourceParam = Expression.Parameter(typeof(TSource), "source");
            ParameterExpression targetParam = Expression.Parameter(typeof(TTarget), "target");
            List<Expression> mapExpressions = new List<Expression>(memberMappings.Length + 1);

            foreach (MemberMapping<TSource, TTarget> memberMapping in memberMappings)
            {
                Expression source = memberMapping.GetSourceValueExpression(sourceParam);
                Expression target = memberMapping.TargetMember.ResolveSetter(targetParam);

                mapExpressions.Add(Expression.Assign(target, source));
            }

            mapExpressions.Add(targetParam); // "Return target" expression.

            BlockExpression body = Expression.Block(mapExpressions);

            return Expression.Lambda<Func<TSource, TTarget, TTarget>>(body, sourceParam, targetParam);
        }
    }
}