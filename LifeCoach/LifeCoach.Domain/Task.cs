using System;

namespace LifeCoach.Domain
{
    public class Task
    {
        public string Id { get; set; }
        public string Description { get; private set; }
        public DateTime? DueDateTime { get; private set; }       

        public static Task CreateTask(string description, DateTime? dueDateTime = null)
        {
            return new Task(null, description, dueDateTime);
        }

        public Task(string id, string description, DateTime? dueDateTime)
        {
            Id = id;
            Description = description;
            DueDateTime = dueDateTime;
        }
    }
}
