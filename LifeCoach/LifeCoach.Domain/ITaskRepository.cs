using System;

namespace LifeCoach.Domain
{
    public interface ITaskRepository
    {
        void AddTask(Task task);
        Task GetTaskById(Guid id);
    }
}
