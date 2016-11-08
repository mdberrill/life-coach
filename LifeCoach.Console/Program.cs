using CommandLine;
using LifeCoach.Domain;
using MarkdownLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LifeCoach.Console
{
    class Program
    {
        [Verb("status", HelpText ="Understand the current status of the life coach")]
        class StatusOptions
        {

        }

        [Verb("note-activity", HelpText ="Ask the life coach to note down a new activity")]
        class NoteActivity
        {
            [Value(0, MetaName ="Activity Name", Required = true, HelpText ="the name of the activity")]
            public string ActivityName { get; set; }

            [Option('d', HelpText ="Due Date")]
            public DateTime? DueTime { get; set; }
        }

        [Verb("update-activity", HelpText ="Ask the life coach to update an activity they previously noted down")]
        class UpdateActivity
        {
            [Value(0, Required = true)]
            public string Id { get; set; }

            [Option('n', HelpText = "the new activity name")]
            public string Name { get; set; }

            [Option('d', HelpText ="Due Date")]
            public DateTime? DueTime { get; set; }

            [Option("note", HelpText ="Write a note for the activity")]
            public bool WriteNote { get; set; }
        }

        [Verb("complete-activity", HelpText = "complete an activity")]
        public class CompleteActivity
        {
            [Value(0, Required = true)]
            public string Id { get; set; }
        }

        [Verb("list-activities", HelpText ="Ask the life coach to list all the activities you currently have (defaults to today only)")]
        class ActivityList
        {
            [Option('a', HelpText ="Show all activities")]
            public bool ShowAll { get; set; }
        }

        [Verb("forget-client")]
        class ForgetClient
        {
            [Value(0, Required = true)]
            public string ClientName { get; set; }
        }

        [Verb("meet", HelpText = "Create a meeting with the life coach. The life coach will record everything that happens in the meeting.")]
        class MeetOptions
        {
            [Value(0, Required = true)]
            public string ClientName { get; set; }

            [Value(1)]
            public string TooManyArgs { get; set; }
        }

        static int Main(string[] args)
        {

            Domain.LifeCoachRepository repo = new Domain.LifeCoachRepository();
            Domain.LifeCoach lifeCoach = repo.WakeUp();
            if (lifeCoach == null)
                lifeCoach = new Domain.LifeCoach();

            return Parser.Default.ParseArguments<NoteActivity, ForgetClient, StatusOptions, MeetOptions, ActivityList, UpdateActivity, CompleteActivity>(args)
              .MapResult(
                  (MeetOptions opts) =>
                  {
                      // if we have met before lets carry on where we left off
                      var client = repo.WakeUpInMeetingWith(opts.ClientName);
                      if (client != null)
                          lifeCoach.ResumeMeetingWith(client);
                      else
                          lifeCoach.MeetWithNewClient(opts.ClientName);

                      repo.PutToSleep(lifeCoach);
                      System.Console.WriteLine("Meeting with " + lifeCoach.CurrentClient.Name); return 0;
                  },
                (NoteActivity opts) =>
                {
                    if (opts.DueTime.HasValue)
                    {
                        lifeCoach.NoteClientActivity(new Activity(opts.ActivityName) { DueDateTime = opts.DueTime.Value });
                    }
                    else
                        lifeCoach.NoteClientActivity(opts.ActivityName);
                    repo.PutToSleep(lifeCoach);
                    System.Console.WriteLine("new activity: " + opts.ActivityName);
                    return 0;
                },
                (ForgetClient opts) =>
                {
                    repo.DeleteClient(opts.ClientName);
                    lifeCoach.EndMeetingWith(opts.ClientName);
                    repo.PutToSleep(lifeCoach);
                    return 0;
                },
                (StatusOptions opts) =>
                {
                    System.Console.WriteLine("Status:");
                    if (lifeCoach.CurrentClient != null)
                        System.Console.WriteLine(" Currently meeting with " + lifeCoach.CurrentClient.Name);
                    else
                        System.Console.WriteLine(" No meeting");
                                        
                    return 0;
                },
                (ActivityList opts) =>
                {
                    if (opts.ShowAll)
                        System.Console.Write(lifeCoach.CurrentClient.Activities.ToMarkdownTable());
                    else
                    {
                        System.Console.Write(lifeCoach.CurrentClient.Activities.Where(x => x.DueDateTime.Day == DateTime.Today.Day).ToMarkdownTable());
                    }
                    return 0;
                },
                (UpdateActivity opts) =>
                {
                    var activityCount = lifeCoach.CurrentClient.Activities.Count(x => x.Id.ToString().StartsWith(opts.Id));
                    if (activityCount > 1)
                    {
                        System.Console.WriteLine("More than one activity with the same id");
                        return -1;
                    }
                    var activity = lifeCoach.CurrentClient.Activities.First(x => x.Id.ToString().StartsWith(opts.Id));
                    if (activity == null)
                    {
                        System.Console.WriteLine("I don't have that activity id on record");
                        return -1;
                    }

                    if (opts.DueTime.HasValue)
                    {
                        activity.DueDateTime = opts.DueTime.Value;                      
                        System.Console.WriteLine("Updated due time to : " + opts.DueTime.Value.ToString("G"));
                    }
                    if (!string.IsNullOrEmpty(opts.Name))
                    {
                        activity.Name = opts.Name;
                        System.Console.WriteLine("Updated name to : " + opts.Name);
                    }

                    if (opts.WriteNote)
                    {
                        if (string.IsNullOrEmpty(activity.NoteFile))
                        {
                            activity.NoteFile = activity.Id.ToString()+".txt";
                            File.Create(".lifecoach/" + activity.NoteFile).Close();
                        }
                        using (Process cmd = new Process())
                        {
                            cmd.StartInfo.FileName = "notepad.exe";
                            cmd.StartInfo.Arguments = ".lifecoach/" + activity.NoteFile;
                            cmd.StartInfo.RedirectStandardInput = true;
                            cmd.StartInfo.RedirectStandardOutput = true;
                            cmd.StartInfo.CreateNoWindow = false;
                            cmd.StartInfo.UseShellExecute = false;

                            cmd.Start();

                            cmd.WaitForExit();
                            cmd.StandardInput.Close();
                        }
                    }
                    repo.PutToSleep(lifeCoach);
                    return 0;
                },
                (CompleteActivity opts) =>
                {
                    var activityCount = lifeCoach.CurrentClient.Activities.Count(x => x.Id.ToString().StartsWith(opts.Id));
                    if (activityCount > 1)
                    {
                        System.Console.WriteLine("More than one activity with the same id");
                        return -1;
                    }
                    var activity = lifeCoach.CurrentClient.Activities.First(x => x.Id.ToString().StartsWith(opts.Id));
                    if (activity == null)
                    {
                        System.Console.WriteLine("I don't have that activity id on record");
                        return -1;
                    }                    
                    activity.IsComplete = true;
                    repo.PutToSleep(lifeCoach);
                    return 0;
                },
                errs => 1);
        }
    }

}
