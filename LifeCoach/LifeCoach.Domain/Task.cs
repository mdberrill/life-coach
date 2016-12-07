using System;

namespace LifeCoach.Domain
{
    public class Task
    {
        public string Id { get; set; }
        public string Description { get; private set; }
        public DateTime? DueDateTime { get; private set; }
        public DateTime? StartDateTime { get; private set; }
        public bool IsComplete { get; set; }
        public bool IsDeleted { get; internal set; }

        public static Task CreateTask(string description, string id = null, DateTime? dueDateTime = null, bool isComplete = false, bool isDeleted = false, DateTime? startDateTime = null)
        {
            return new Task(id, description, dueDateTime, isComplete, isDeleted, startDateTime);
        }

        public Task(string id, string description, DateTime? dueDateTime, bool isComplete, bool isDeleted, DateTime? startDateTime)
        {
            Id = id;
            Description = description;
            DueDateTime = dueDateTime;
            StartDateTime = startDateTime;
            IsComplete = isComplete;
            IsDeleted = isDeleted;
        }
    }
}
