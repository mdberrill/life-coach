﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Task GetTaskById(Guid id)
        {
            return _taskRepo.GetTaskById(id);
        }
    }

    public interface ITaskRepository
    {
        void AddTask(Task task);
        Task GetTaskById(Guid id);
    }
}
