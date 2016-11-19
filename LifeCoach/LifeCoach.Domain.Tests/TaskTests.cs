using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeCoach.Domain.Tests
{
    [TestFixture]
    public class TaskTests
    {
        [Test]
        public void Created_with_ValidDueDate_ShouldBeAllowed()
        {
            var taskDateTime = new DateTime(2016, 11, 19, 10, 30, 5);
            Task task = Task.CreateTask("My task", taskDateTime);
            Assert.AreEqual(task.DueDateTime, taskDateTime);
        }
    }
}
