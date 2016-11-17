using System;
using System.Collections.Generic;

namespace LifeCoach.Domain
{
    public interface ITaskRepository
    {
        void AddTask(Task task);
        IEnumerable<Task> GetTaskWithNoDates();
    }
}
