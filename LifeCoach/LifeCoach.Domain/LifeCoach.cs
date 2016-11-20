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

        public IEnumerable<Task> GetUnplannedTasks()
        {
            return _taskRepo.GetTaskWithNoDates();
        }

        public IEnumerable<Task> GetTasksDueOn(DateTime value)
        {
            return _taskRepo.GetTasksDueOn(value);
        }
                
        public IEnumerable<Task> GetTasksDueBetween(DateTime from, DateTime to)
        {
            return _taskRepo.GetTasksDueBetween(from, to);
        }
    }
}
