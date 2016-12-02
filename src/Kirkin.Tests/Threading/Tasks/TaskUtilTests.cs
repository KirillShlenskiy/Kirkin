using System.Threading.Tasks;

using Kirkin.Threading.Tasks;

using Xunit;

namespace Kirkin.Tests.Threading.Tasks
{
    public class TaskUtilTests
    {
        [Fact]
        public void CompletedVoidTask()
        {
            Task task = TaskUtil.CompletedTask;

            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
        }
    }
}