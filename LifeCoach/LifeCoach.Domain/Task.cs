using System;

namespace LifeCoach.Domain
{
    public class Task
    {
        public string Description { get; private set; }
        public string Id { get; set; }

        public static Task CreateTask(string description)
        {
            return new Task( description);
        }
        public Task(string description)
        {
            Description = description;
        }
    }
}
