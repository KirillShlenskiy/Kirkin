using System;

using Kirkin.Functional;

using Xunit;

namespace Kirkin.Tests.Functional
{
    public class FuncUtilTests
    {
        //[Fact]
        //public void ExecuteWithRetrySucceeds()
        //{
        //    var i = 0;
        //    var a = FuncUtil.WithRetry(3, () =>
        //    {
        //        i++;

        //        return 42;
        //    });

        //    Assert.Equal(1, i);
        //    Assert.Equal(42, a);
        //}

        //[Fact]
        //public void ExecuteWithRetrySucceedsOnLastRetry()
        //{
        //    var i = 0;

        //    var a = FuncUtil.WithRetry(3, () =>
        //    {
        //        if (++i < 3) throw new InvalidOperationException();

        //        return 42;
        //    });

        //    Assert.Equal(3, i);
        //    Assert.Equal(42, a);
        //}

        //[Fact]
        //public void ExecuteWithRetryFailsWhenRetryCountReached()
        //{
        //    var i = 0;

        //    var buggyFunc = new Func<int>(() =>
        //    {
        //        i++;
        //        throw new InvalidOperationException();
        //    });

        //    var errored = false;

        //    try
        //    {
        //        FuncUtil.WithRetry(3, buggyFunc);
        //    }
        //    catch
        //    {
        //        errored = true;
        //    }

        //    Assert.True(errored);
        //    Assert.Equal(3, i);
        //}
    }
}