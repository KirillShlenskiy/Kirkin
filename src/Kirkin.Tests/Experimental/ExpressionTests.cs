using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

using Kirkin.Linq.Expressions;

using Xunit;

namespace Kirkin.Tests.Experimental
{
    public class ExpressionTests
    {
        [Fact]
        public void MutateStruct()
        {
            var size = new Size { Width = 2, Height = 1 };

            ParameterExpression param = Expression.Parameter(typeof(Size), "value");

            BlockExpression block = Expression.Block(
                Expression.Assign(Expression.MakeMemberAccess(param, typeof(Size).GetProperty("Width")), Expression.Constant(3)),
                param
            );

            Expression<Func<Size, Size>> lambda = Expression.Lambda<Func<Size, Size>>(block, param);

            size = lambda.Compile().Invoke(size);

            Assert.Equal(3, size.Width);
            Assert.Equal(1, size.Height);
        }

        //private void Mutate<TObj, TProperty>(TObj obj, Expression<Func<TObj, TProperty>> propertyExpr, Expression<Action<TProperty>> mutation)
        //{
        //    PropertyInfo property = ExpressionUtil.Property(propertyExpr);
        //    ParameterExpression objParam = Expression.Parameter(typeof(TObj), "obj");
        //    Expression paramSubstitutedMutation = new MutationParameterSubstitution(objParam).Visit(mutation);

        //    // Rewrite mutation.
        //    var e = Expression.Assign(Expression.MakeMemberAccess(objParam, property), paramSubstitutedMutation);

        //    Action<TObj> del;
        //}

        sealed class MutationParameterSubstitution : ExpressionVisitor
        {
            private readonly ParameterExpression Parameter;

            internal MutationParameterSubstitution(ParameterExpression parameter)
            {
                Parameter = parameter;
            }

            public override Expression Visit(Expression node)
            {
                return node is ParameterExpression ? Parameter : node;
            }
        }

        struct Size
        {
            public int Width { get; set; }
            public int Height { get; set; }
        }

        [Fact]
        public void FuncExecute()
        {
            Func<string, int> func = input => int.Parse(input);

            ParameterExpression inputParam = Expression.Parameter(typeof(string), "input");
            MethodInfo invokeMethod = typeof(Func<string, int>).GetMethod("Invoke");
            Expression call = Expression.Call(Expression.Constant(func), invokeMethod, inputParam);
            Expression<Func<string, int>> lambda = Expression.Lambda<Func<string, int>>(call, inputParam);

            Debug.Print(lambda.ToString());

            Func<string, int> funcRebuilt = lambda.Compile();

            Assert.Equal(5, funcRebuilt("5"));
        }

        [Fact]
        public void Basic()
        {
            Expression<Func<Dummy, int>> expr = d => d.ID;

            string res = expr.ToString();

            Debug.Print(res);
        }

        [Fact]
        public void ManuallyCreate()
        {
            Expression<Func<Dummy, int>> expected = d => d.ID;

            ParameterExpression param = Expression.Parameter(typeof(Dummy), "d");
            MemberExpression member = Expression.MakeMemberAccess(param, ExpressionUtil.Property<Dummy>(d => d.ID));
            LambdaExpression lambda = Expression.Lambda<Func<Dummy, int>>(member, param);

            Debug.Print(expected.ToString());
        }

        [Fact]
        public void Coalesce()
        {
            Expression<Func<int?, int>> expr = i => i ?? default(int);

            Debug.Print(expr.ToString());
        }

        sealed class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }

        sealed class LowercaseDummy
        {
            public int id { get; set; }
            public string value { get; set; }
        }
    }
}