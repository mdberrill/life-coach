﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeCoach.Domain
{
    public class Task
    {
        public string Description { get; private set; }
        public Guid Id { get; private set; }

        public static Task CreateTask(string description)
        {
            return new Task(Guid.NewGuid(), description);
        }
        public Task(Guid id, string description)
        {
            Id = id;
            Description = description;
        }
    }
}