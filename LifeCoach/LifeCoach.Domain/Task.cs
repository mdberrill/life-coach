using System;

namespace LifeCoach.Domain
{
    public class Task
    {
        public string Id { get; set; }
        public string Description { get; private set; }
        public DateTime? DueDateTime { get; private set; }
        public bool IsComplete { get; set; }
        public bool IsDeleted { get; internal set; }

        public static Task CreateTask(string description, string id = null, DateTime? dueDateTime = null, bool isComplete = false, bool isDeleted = false)
        {
            return new Task(id, description, dueDateTime, isComplete, isDeleted);
        }

        public Task(string id, string description, DateTime? dueDateTime, bool isComplete, bool isDeleted)
        {
            Id = id;
            Description = description;
            DueDateTime = dueDateTime;
            IsComplete = isComplete;
            IsDeleted = isDeleted;
        }
    }
}
