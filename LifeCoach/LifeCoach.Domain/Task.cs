using System;

namespace LifeCoach.Domain
{
    public class Task
    {
        public string Id { get; set; }
        public string Description { get; private set; }
        public DateTime? DueDateTime { get; private set; }
        public bool IsComplete { get; set; }

        public static Task CreateTask(string description, string id = null, DateTime? dueDateTime = null, bool isComplete = false)
        {
            return new Task(id, description, dueDateTime, isComplete);
        }

        public Task(string id, string description, DateTime? dueDateTime, bool isComplete)
        {
            Id = id;
            Description = description;
            DueDateTime = dueDateTime;
            IsComplete = isComplete;
        }
    }
}
