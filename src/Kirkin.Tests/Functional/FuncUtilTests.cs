namespace Kirkin.Tests.Functional
{
    public class FuncUtilTests
    {
        //[Test]
        //public void ExecuteWithRetrySucceeds()
        //{
        //    var i = 0;
        //    var a = FuncUtil.WithRetry(3, () =>
        //    {
        //        i++;

        //        return 42;
        //    });

        //    Assert.AreEqual(1, i);
        //    Assert.AreEqual(42, a);
        //}

        //[Test]
        //public void ExecuteWithRetrySucceedsOnLastRetry()
        //{
        //    var i = 0;

        //    var a = FuncUtil.WithRetry(3, () =>
        //    {
        //        if (++i < 3) throw new InvalidOperationException();

        //        return 42;
        //    });

        //    Assert.AreEqual(3, i);
        //    Assert.AreEqual(42, a);
        //}

        //[Test]
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
        //    Assert.AreEqual(3, i);
        //}
    }
}