using CommandLine;
using LifeCoach.Domain;
using MarkdownLog;
using System;
using System.Collections.Generic;
using System.IO;

namespace LifeCoach.Console
{
    class Program
    {
        [Verb("note-task", HelpText = "Ask the life coach to note down a new task")]
        class NoteTask
        {
            [Value(0, MetaName = "Task Name", Required = true, HelpText = "the name of the task")]
            public string TaskName { get; set; }

            [Value(1, MetaName ="Due time", Required = false, Default = null, HelpText = "The due date & time that the task is due e.g 2016-11-19 14:30")]
            public DateTime? DueDateTime { get; set; }

            [Option('s', HelpText = "The start date & time that the task is planned to start e.g. 2016-11-19 14:30, or for today 14:30")]
            public DateTime? StartDateTime { get; set; }
        }

        [Verb("set-agenda", HelpText ="Ask the life coach to set a task for todays agenda")]
        class AgendaTask
        {
            [Value(0, MetaName ="Time range", Required = true, HelpText ="Time range, e.g. 13:30-14:45")]
            public string TaskTimeRange { get; set; }

            [Value(1, MetaName = "Task Name", Required = true, HelpText = "the name of the task")]
            public string TaskName { get; set; }
        }

        [Verb("list-tasks", HelpText = "Ask the life coach to list all the tasks you currently have")]
        class TaskList
        {
            [Value(0, HelpText = "The number of days in the future that tasks are due, e.g. 7 would show all tasks due in the next 7 days", Required = false)]
            public int? DueInNextXDays { get; set; }

            [Option('d', "due", HelpText = "The date on which the tasks are due, e.g. 2016-11-19")]
            public DateTime? DueDate { get; set; }

            [Option('u', "unplanned", HelpText = "View all unplanned tasks (those without a due date")]
            public bool Unplanned { get; set; }

            [Option('f', "NoFormatting", HelpText = "Show output without any formatting")]
            public bool NoFormatting { get; set; }

            [Option("deleted", HelpText = "Show those tasks that have been deleted")]
            public bool ShowDeleted { get; set; }
        }

        [Verb("complete-task")]
        class CompleteTask
        {
            [Value(0, Required = true, HelpText = "The Id of the task. This is required to be enough characters of the task ID that ensures it is unique")]
            public string Id { get; set; }

            [Option('u', "Undo", HelpText = "Undo the complete status of the task back to incomplete")]
            public bool Undo { get; set; }
        }

        [Verb("delete-task")]
        class DeleteTask
        {
            [Value(0, Required = true, HelpText = "The Id of the task. This is required to be enough characters of the task ID that ensures it is unique")]
            public string Id { get; set; }

            [Option('p', "Permanent", Default = false, HelpText = "Permanently delete task. A prompt will confirm this action.")]
            public bool PermanentDelete { get; set; }

            [Option('s', Default =false, HelpText ="Silent delete, does not prompt for any confirmations.")]
            public bool Silent { get; set;}

            [Option('u', "Undo", HelpText = "Undo the delete and restore the task")]
            public bool Undo { get; set; }
        }

        static int Main(string[] args)
        {            
            var taskRepo = new GoogleCalendarGateway.GoogleTaskRepository(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"client_secret.json"), "Life Coach");
            Domain.LifeCoach lifeCoach = new Domain.LifeCoach(taskRepo);

            return Parser.Default.ParseArguments<NoteTask, AgendaTask, TaskList, CompleteTask, DeleteTask>(args)
             .MapResult(
               (NoteTask opts) =>
               {
                   lifeCoach.NoteTask(Task.CreateTask(opts.TaskName, dueDateTime: opts.DueDateTime, startDateTime: opts.StartDateTime));
                   return 0;
               },
               (AgendaTask opts) =>
               {
                   var times = opts.TaskTimeRange.Split('-');
                   var startTime = DateTime.Parse(times[0]);
                   var endTime = DateTime.Parse(times[1]);
                   System.Console.WriteLine("StartTime: " + startTime);
                   System.Console.WriteLine("EndTime: " + endTime);
                   lifeCoach.NoteTask(Task.CreateTask(opts.TaskName, dueDateTime: endTime, startDateTime: startTime));
                   return 0;
               },
               (TaskList opts) =>
               {
                   if (opts.DueDate.HasValue)
                   {
                       System.Console.Write(FormatTable(lifeCoach.GetTasksDueOn(opts.DueDate.Value.Date, opts.ShowDeleted), opts.NoFormatting));
                   }
                   else if (opts.Unplanned)
                   {
                       System.Console.Write(FormatTable(lifeCoach.GetUnplannedTasks(opts.ShowDeleted), opts.NoFormatting));
                   }
                   else if (opts.DueInNextXDays.HasValue)
                   {
                       if (opts.DueInNextXDays.Value < 0)
                           System.Console.Write(FormatTable(lifeCoach.GetTasksDueBetween(DateTime.Today.AddDays(opts.DueInNextXDays.Value), DateTime.Today, opts.ShowDeleted), opts.NoFormatting));
                       else
                           System.Console.Write(FormatTable(lifeCoach.GetTasksDueBetween(DateTime.Today, DateTime.Today.AddDays(opts.DueInNextXDays.Value), opts.ShowDeleted), opts.NoFormatting));
                   }
                   else // assume they want to see today's due tasks - most common use case - what have I got to do today
                   {
                       System.Console.Write(FormatTable(lifeCoach.GetTasksDueOn(DateTime.Today, opts.ShowDeleted), opts.NoFormatting));
                   }
                   return 0;
               },
               (CompleteTask opts) =>
               {
                   if (!string.IsNullOrEmpty(opts.Id))
                   {
                       if (opts.Undo)
                           lifeCoach.SetTaskCompleteStatus(opts.Id, false);
                       else
                           lifeCoach.SetTaskCompleteStatus(opts.Id, true);
                   }
                   return 0;
               },
               (DeleteTask opts) =>
               {
                   if (!string.IsNullOrEmpty(opts.Id))
                   {
                       if (opts.Undo)
                       {
                           lifeCoach.RestoreTask(opts.Id);
                           return 0;
                       }

                       if (opts.PermanentDelete && !opts.Silent)
                       {
                           System.Console.WriteLine("Are you sure Y/N?");
                           var confirmation = System.Console.ReadLine().ToUpper() == "Y";
                           if (!confirmation)
                           {
                               System.Console.WriteLine("Nothing was deleted.");
                               return -1;
                           }
                       }

                       lifeCoach.DeleteTask(opts.Id, opts.PermanentDelete);
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
                    x => x.StartDateTime.HasValue ? x.StartDateTime.Value.ToString("dd/MM/yyyy HH:mm") : "-", 
                    x => x.DueDateTime.HasValue ? x.DueDateTime.Value.ToString("dd/MM/yyyy HH:mm") : "-",                    
                    x => x.IsComplete ? "Yes" : "No")
                    .WithHeaders(
                    "Id",
                    "Description",
                    "Start Time", 
                    "Due Date",                    
                    "Done");
        }
    }
}
