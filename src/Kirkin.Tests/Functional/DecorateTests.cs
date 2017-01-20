using System;

using Kirkin.Functional;

using NUnit.Framework;

namespace Kirkin.Tests.Functional
{
    public class DecorateTests
    {
        [Test]
        public void Api()
        {
            Func<int, int> get = i => i;
            Func<Func<int, int>, Func<int, int>> plusOneDecorator = f => arg => f(arg) + 1;

            Dummy dummy = new Dummy();

            Func<int, int> getValuePlusOne = plusOneDecorator(dummy.AddThree);

            getValuePlusOne = Decorate.Func<int, int>(dummy.AddThree, f => arg => f(arg) + 1);

            getValuePlusOne(123);

            Func<int, int, int> add = (x, y) => x + y;
            Func<Func<int, int, int>, Func<int, int, int>> plusOneDecorator2 = f => (x, y) => f(x, y) + 1;
            Action<int> consume = i => { };

            Action<int> consumeRetry = Decorate.Action<int>(i => { }, action => i =>
            {
                try
                {
                    action(i);
                }
                catch
                {
                    action(i);
                }
            });

            Func<Action<int>, Action<int>> retryDecorator = f => i =>
            {
                try
                {
                    f(i);
                }
                catch
                {
                    f(i);
                }
            };
        }

        sealed class Dummy
        {
            public int AddThree(int i)
            {
                return i + 3;
            }
        }
    }
}