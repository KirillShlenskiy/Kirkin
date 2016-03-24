using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace Kirkin.Tests.Experimental
{
    public class NonNullableStringTests
    {
        [Fact]
        public void ImplicitConversions()
        {
            NonNullableString s = "hey!";

            Assert.False(string.IsNullOrEmpty(s));
            Assert.True(default(NonNullableString) == "");
        }

        [Fact]
        public void TornRead()
        {
            // See if we can tear the struct with an in-place update
            // by reassigning the local variable which contains it,
            // forcing an instance method to see different values
            // of "this" during execution.

            var s = new NonNullableString();
            var cts = new CancellationTokenSource(1000); // Cancel after 1 sec.

            var reassignment = Task.Run(() =>
            {
                int i = 0;

                while (!cts.IsCancellationRequested)
                {
                    // 0 or 1 or 2.
                    i = (i + 1) % 3;

                    // If i = 0, OriginalValue = null;
                    // otherwise, meaningful value.
                    s = new NonNullableString(i == 0 ? null : i.ToString());
                }
            }, cts.Token);

            while (!reassignment.IsCompleted) {
                Assert.NotNull(s.Value);
            }
        }
    }
}
