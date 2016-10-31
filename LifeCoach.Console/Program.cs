using CommandLine;
using System;

namespace LifeCoach.Console
{
    class Program
    {
        [Verb("status")]
        class StatusOptions
        {

        }

        [Verb("note-activity")]
        class NoteActivity
        {
            [Value(0, Required = true)]
            public string ActivityName { get; set; }
        }

        [Verb("list-activities")]
        class ActivityList
        {

        }

        [Verb("complete", HelpText = "complete activities")]
        class RemoveOptions
        {
            //normal options here
        }

        [Verb("meet", HelpText = "Creat a meeting with the life coach. The life coach will record everything that happens in the meeting.")]
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
            Domain.LifeCoach lifeCoach = repo.LoadPreviousClient();
            if (lifeCoach == null)
                lifeCoach = new Domain.LifeCoach();

            return Parser.Default.ParseArguments<NoteActivity, RemoveOptions, StatusOptions, MeetOptions, ActivityList>(args)
              .MapResult(
                  (MeetOptions opts) =>
                  {
                      // if we have met before lets carry on where we left off
                      var client = repo.LoadPreviousClient(opts.ClientName);
                      if (client != null)
                          lifeCoach.ResumeMeetingWith(client);
                      else
                          lifeCoach.MeetWithNewClient(opts.ClientName);

                      repo.SaveClient(lifeCoach);
                      System.Console.WriteLine("Meeting with " + lifeCoach.CurrentClient.Name); return 0;
                  },
                (NoteActivity opts) => { lifeCoach.NoteClientActivity(opts.ActivityName); repo.SaveClient(lifeCoach); System.Console.WriteLine("new activity: " + opts.ActivityName); return 0; },
                (RemoveOptions opts) => { System.Console.WriteLine("Remove"); return 0; },
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
                    foreach (var activity in lifeCoach.CurrentClient.Activities)
                    {
                        System.Console.WriteLine(activity);
                    }
                    return 0;
                },
                errs => 1);
        }
    }

}
