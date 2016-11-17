using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeCoach.Domain
{
    public class LifeCoach
    {
        public void NoteTask(Task task)
        {

        }

        public Task GetTaskById(Guid id)
        {
            return new Task(id, "MyTestTask");
        }
    }
}
