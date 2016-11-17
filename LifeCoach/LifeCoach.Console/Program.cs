namespace LifeCoach.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var taskRepo = new GoogleCalendarGateway.GoogleTaskRepository("client_secret.json", "Life Coach");
            Domain.LifeCoach lifeCoach = new Domain.LifeCoach(taskRepo);


        }
    }
}
