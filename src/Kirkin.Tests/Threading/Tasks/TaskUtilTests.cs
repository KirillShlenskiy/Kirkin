using System.Threading.Tasks;

using Kirkin.Threading.Tasks;

using NUnit.Framework;

namespace Kirkin.Tests.Threading.Tasks
{
    public class TaskUtilTests
    {
        [Test]
        public void CompletedVoidTask()
        {
            Task task = TaskUtil.CompletedTask;

            Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
        }
    }
}