using CommandLine;
using LifeCoach.Domain;
using MarkdownLog;

namespace LifeCoach.Console
{
    class Program
    {
        [Verb("note-task", HelpText = "Ask the life coach to note down a new task")]
        class NoteTask
        {
            [Value(0, MetaName = "Task Name", Required = true, HelpText = "the name of the task")]
            public string TaskName { get; set; }
        }

        [Verb("list-tasks", HelpText = "Ask the life coach to list all the tasks you currently have")]
        class TaskList
        {
        }

        static int Main(string[] args)
        {
            var taskRepo = new GoogleCalendarGateway.GoogleTaskRepository("client_secret.json", "Life Coach");
            Domain.LifeCoach lifeCoach = new Domain.LifeCoach(taskRepo);

            return Parser.Default.ParseArguments<NoteTask, TaskList>(args)
            .MapResult(
               (NoteTask opts) =>
               {
                   lifeCoach.NoteTask(new Task(opts.TaskName));
                   return 0;
               },
               (TaskList opts) =>
               {
                   System.Console.Write(lifeCoach.GetTasksWithNoDates().ToMarkdownTable());
                   return 0;
               },
              errs => 1);
        }
    }
}
