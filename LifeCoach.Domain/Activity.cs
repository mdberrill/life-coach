using System;

namespace LifeCoach.Domain
{
    public class Activity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime DueDateTime { get; set; }
        public bool IsComplete { get; set; }

        protected Activity() { }
        public Activity(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }
    }
}