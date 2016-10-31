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

        [Verb("new", HelpText = "New activities")]
        class NewOptions
        {
            //normal options here
            [Option('a', "activity")]
            public string ActivityName { get; set; }
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

            return Parser.Default.ParseArguments<NewOptions, RemoveOptions, StatusOptions, MeetOptions>(args)
              .MapResult(
                (NewOptions opts) => { System.Console.WriteLine("add " + opts.ActivityName); return 0; },
                (RemoveOptions opts) => { System.Console.WriteLine("Remove"); return 0; },
                (StatusOptions opts) => { System.Console.WriteLine("Status:");
                    if (lifeCoach.CurrentClient != null)
                        System.Console.WriteLine(" Currently meeting with " + lifeCoach.CurrentClient.Name);
                    else
                        System.Console.WriteLine(" No meeting");
                    return 0; },
                (MeetOptions opts) => {
                    // if we have met before lets carry on where we left off
                    var client = repo.LoadPreviousClient(opts.ClientName);
                    if (client != null)
                        lifeCoach.ResumeMeetingWith(client);
                    else
                        lifeCoach.MeetWithNewClient(opts.ClientName);

                    repo.SaveClient(lifeCoach);
                    System.Console.WriteLine("Meeting with "+lifeCoach.CurrentClient.Name); return 0; },
                errs => 1);
        }
    }

}
