using System;
using System.Collections.Generic;
using System.Linq;

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

        public void SetTaskCompleteStatus(string id, bool isCompleted)
        {
            var task = _taskRepo.GetTaskById(id);
            task.IsComplete = isCompleted;
            _taskRepo.UpdateTask(task);
        }

        public IEnumerable<Task> GetUnplannedTasks(bool deleted)
        {
            return _taskRepo.GetTaskWithNoDates().Where(x => x.IsDeleted == deleted);
        }

        public IEnumerable<Task> GetTasksDueOn(DateTime value, bool deleted)
        {
            return _taskRepo.GetTasksDueOn(value).Where(x=>x.IsDeleted == deleted);
        }

        public IEnumerable<Task> GetTasksDueBetween(DateTime from, DateTime to, bool deleted)
        {
            return _taskRepo.GetTasksDueBetween(from, to).Where(x => x.IsDeleted == deleted);
        }

        public void DeleteTask(string id, bool permanentDelete)
        {
            var task = _taskRepo.GetTaskById(id);

            if (permanentDelete)
                _taskRepo.DeleteTask(task);
            else
            {
                task.IsDeleted = true;
                _taskRepo.UpdateTask(task);
            }
        }

        public void RestoreTask(string id)
        {
            var task = _taskRepo.GetTaskById(id);
            task.IsDeleted = false;
            _taskRepo.UpdateTask(task);
        }
    }
}
