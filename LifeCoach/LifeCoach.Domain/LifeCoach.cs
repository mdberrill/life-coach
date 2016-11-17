using System;
using System.Collections.Generic;

namespace LifeCoach.Domain
{
    public class LifeCoach
    {
        private readonly ITaskRepository _taskRepo;

        public LifeCoach(ITaskRepository taskRepo)
        {
            _taskRepo = taskRepo;
        }

        public void NoteTask(Task task)
        {
            _taskRepo.AddTask(task);
        }

        public IEnumerable<Task> GetTasksWithNoDates()
        {
            return _taskRepo.GetTaskWithNoDates();
        }
    }
}
