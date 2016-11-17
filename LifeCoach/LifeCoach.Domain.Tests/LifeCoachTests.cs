using NUnit.Framework;

namespace LifeCoach.Domain.Tests
{
    [TestFixture]
    public class LifeCoachTests
    {
        [Test]
        public void NoteTask_WithValidTask_ShouldCreateTask()
        {
            LifeCoach lifeCoach = new LifeCoach();
            Task task = Task.CreateTask("MyTestTask");
            lifeCoach.NoteTask(task);

            var taskRetrieved = lifeCoach.GetTaskById(task.Id);
            Assert.AreEqual("MyTestTask", taskRetrieved.Description);
        }
    }
}
