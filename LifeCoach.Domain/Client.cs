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
        private List<string> activities = new List<string>();
        public string[] Activities
        {
            get { return activities.ToArray();}
            set
            {
                activities = new List<string>(value);
            }
        }

        protected Client() { }

        public Client(string clientName)
        {
            Name = clientName;
        }
        public void AddActivity(string name)
        {
            activities.Add(name);
        }
    }
}
