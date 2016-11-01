using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeCoach.Domain
{
    public class Client
    {
        public string Name { get; set; }
        private List<Activity> activities = new List<Activity>();
        public Activity[] Activities
        {
            get { return activities.ToArray();}
            set
            {
                activities = new List<Activity>(value);
            }
        }

        protected Client() { }

        public Client(string clientName)
        {
            Name = clientName;
        }
        public void AddActivity(string name)
        {
            activities.Add(new Activity(name));
        }

        internal void AddActivity(Activity activity)
        {
            activities.Add(activity);
        }
    }
}
