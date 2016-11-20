using CommandLine;
using LifeCoach.Domain;
using MarkdownLog;
using System;
using System.Collections.Generic;

namespace LifeCoach.Console
{
    class Program
    {
        [Verb("note-task", HelpText = "Ask the life coach to note down a new task")]
        class NoteTask
        {
            [Value(0, MetaName = "Task Name", Required = true, HelpText = "the name of the task")]
            public string TaskName { get; set; }

            [Option('d', "due", HelpText = "The due date & time that the task is due e.g 2016-11-19 14:30")]
            public DateTime? DueDateTime { get; set; }
        }

        [Verb("list-tasks", HelpText = "Ask the life coach to list all the tasks you currently have")]
        class TaskList
        {

            [Value(0, HelpText = "The number of days in the future that tasks are due, e.g. 7 would show all taks due in the next 7 days", Required = false)]
            public int? DueInNextXDays { get; set; }

            [Option('d', "due", HelpText = "The date on which the tasks are due, e.g. 2016-11-19")]
            public DateTime? DueDate { get; set; }

            [Option('u', "unplanned", HelpText = "View all unplanned tasks (those without a due date")]
            public bool Unplanned { get; set; }

            [Option('f', "NoFormatting", HelpText ="Show output without any formatting")]
            public bool NoFormatting { get; set; }
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
                       System.Console.Write(FormatTable(lifeCoach.GetTasksDueOn(opts.DueDate.Value.Date), opts.NoFormatting));
                   }
                   else if (opts.Unplanned)
                   {
                       System.Console.Write(FormatTable(lifeCoach.GetUnplannedTasks(), opts.NoFormatting));
                   }
                   else if (opts.DueInNextXDays.HasValue)
                   {
                       System.Console.Write(FormatTable(lifeCoach.GetTasksDueBetween(DateTime.Today, DateTime.Today.AddDays(opts.DueInNextXDays.Value)), opts.NoFormatting));
                   }
                   else // assume they want to see todays due tasks - most common use case - what have I got to do today
                   {
                       System.Console.Write(FormatTable(lifeCoach.GetTasksDueOn(DateTime.Today), opts.NoFormatting));
                   }
                   return 0;
               },
              errs => 1);
        }


        static Table FormatTable(IEnumerable<Task> tasks, bool noFormatting = false)
        {
            if (noFormatting)
                return tasks.ToMarkdownTable();
            else
                return tasks.ToMarkdownTable(
                    x => x.Id.Substring(0, 6),
                    x => x.Description.Length > 40 ? x.Description.Substring(0, 40) + "..." : x.Description,
                    x => x.DueDateTime.HasValue ? x.DueDateTime.Value.ToString("G") : "-")
                    .WithHeaders(
                    "Id",
                    "Description",
                    "Due Date");
        }
    }
}
