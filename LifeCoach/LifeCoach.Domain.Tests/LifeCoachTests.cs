using Moq;
using NUnit.Framework;

namespace LifeCoach.Domain.Tests
{
    [TestFixture]
    public class LifeCoachTests
    {
        [Test]
        public void NoteTask_WithValidTask_ShouldCreateTask()
        {
            Mock<ITaskRepository> mockTaskRepo = new Mock<ITaskRepository>();
            LifeCoach lifeCoach = new LifeCoach(mockTaskRepo.Object);
            Task task = Task.CreateTask("MyTestTask");

            mockTaskRepo.Setup(x => x.AddTask(task));

            lifeCoach.NoteTask(task);

            mockTaskRepo.VerifyAll();
        }

        [Test]
        public void NoteTask_SeveralTimesWithDifferentTasks_ShouldCreateTasks()
        {
            Mock<ITaskRepository> mockTaskRepo = new Mock<ITaskRepository>();
            LifeCoach lifeCoach = new LifeCoach(mockTaskRepo.Object);
            Task task = Task.CreateTask("MyTestTask");

            mockTaskRepo.Setup(x => x.AddTask(task));

            lifeCoach.NoteTask(task);

            Task myOtherTask = Task.CreateTask("MyOtherTask");

            mockTaskRepo.Setup(x => x.AddTask(myOtherTask));

            lifeCoach.NoteTask(myOtherTask);

            mockTaskRepo.VerifyAll();
        }
    }
}
