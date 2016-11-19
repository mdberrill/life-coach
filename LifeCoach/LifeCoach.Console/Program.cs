using CommandLine;
using LifeCoach.Domain;
using MarkdownLog;
using System;

namespace LifeCoach.Console
{
    class Program
    {
        [Verb("note-task", HelpText = "Ask the life coach to note down a new task")]
        class NoteTask
        {
            [Value(0, MetaName = "Task Name", Required = true, HelpText = "the name of the task")]
            public string TaskName { get; set; }

            [Option('d', "due", HelpText ="The due date & time that the task is due e.g 2016-11-19 14:30")]
            public DateTime? DueDateTime { get; set;}
        }

        [Verb("list-tasks", HelpText = "Ask the life coach to list all the tasks you currently have")]
        class TaskList
        {
            [Option('d', "due", HelpText ="The date on which the tasks are due, e.g. 2016-11-19")]
            public DateTime? DueDate { get; set; }
        }

        static int Main(string[] args)
        {
            var taskRepo = new GoogleCalendarGateway.GoogleTaskRepository("client_secret.json", "Life Coach");
            Domain.LifeCoach lifeCoach = new Domain.LifeCoach(taskRepo);

            return Parser.Default.ParseArguments<NoteTask, TaskList>(args)
            .MapResult(
               (NoteTask opts) =>
               {
                   lifeCoach.NoteTask(Task.CreateTask(opts.TaskName, opts.DueDateTime));
                   return 0;
               },
               (TaskList opts) =>
               {
                   if (opts.DueDate.HasValue)
                   {
                       System.Console.Write(lifeCoach.GetTasksDueOn(opts.DueDate.Value.Date).ToMarkdownTable());
                   }
                   else
                   {
                       System.Console.Write(lifeCoach.GetTasksWithNoDates().ToMarkdownTable());
                   }
                   return 0;
               },
              errs => 1);
        }
    }
}
