using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using LifeCoach.Domain;
using LifeCoach.GoogleCalendarGateway;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace LifeCoach.Integration.Tests
{
    [TestFixture]
    public class GoogleTaskRepositoryTests
    {
        private static string LifeCoachTestCalendarName = "Life Coach Test";

        [Test]
        public void AddTask_WithValidTask_ShouldAddTaskToLifeCoachGoogleCalendar()
        {          
            GoogleTaskRepository sut = new GoogleTaskRepository(getSecretFilePath(), LifeCoachTestCalendarName);
            Task task = Task.CreateTask("MyTestTask");
            sut.AddTask(task);

            CalendarService service = getCalendarService();

            var lifeCoachCalendarId = getLifeCoachCalendarId(service, LifeCoachTestCalendarName);

            Assert.IsNotNull(lifeCoachCalendarId, "The life coach calendar should exist");

            EventsResource.GetRequest request = service.Events.Get(lifeCoachCalendarId, task.Id.ToString());

            Event evt = request.Execute();
            Assert.IsNotNull(evt);
            Assert.AreEqual(evt.Summary, "MyTestTask");
            Console.WriteLine("{0}", evt.Summary);
        }

        [Test]
        public void GetTaskWithNoDates_WhenSeveralTasksExistWithNoDates_ShouldReturnTasks()
        {
            GoogleTaskRepository sut = new GoogleTaskRepository(getSecretFilePath(), LifeCoachTestCalendarName);
            Task task = Task.CreateTask("MyTestTask");
            sut.AddTask(task);

            Task task2 = Task.CreateTask("My second task");
            sut.AddTask(task2);

            var tasks = sut.GetTaskWithNoDates().ToArray();
            Assert.That(tasks.Any(x => x.Id == task.Id));
            Assert.That(tasks.Any(x => x.Id == task2.Id));
        }

        [Test]
        public void AddTask_WithTaskWithDate_ShouldAddTaskWithDateToGoogleCalendar()
        {
            GoogleTaskRepository sut = new GoogleTaskRepository(getSecretFilePath(), LifeCoachTestCalendarName);
            var taskDueDate = new DateTime(2016, 10, 1, 14, 05, 47);
            Task task = Task.CreateTask("MyTestTask", taskDueDate);
            sut.AddTask(task);

            CalendarService service = getCalendarService();

            var lifeCoachCalendarId = getLifeCoachCalendarId(service, LifeCoachTestCalendarName);

            Assert.IsNotNull(lifeCoachCalendarId, "The life coach calendar should exist");

            EventsResource.GetRequest request = service.Events.Get(lifeCoachCalendarId, task.Id.ToString());

            Event evt = request.Execute();
            Assert.IsNotNull(evt);
            Assert.AreEqual(evt.Summary, "MyTestTask");
            Assert.AreEqual(taskDueDate, evt.End.DateTime);
        }

        [Test]
        public void GetTaskWithDates_WithValidDates_ShouldReturnTasksWithinDates()
        {
            GoogleTaskRepository sut = new GoogleTaskRepository(getSecretFilePath(), LifeCoachTestCalendarName);
            var taskDueDate = new DateTime(2016, 10, 1, 14, 05, 47);
            Task task = Task.CreateTask("MyTestTask", taskDueDate);
            sut.AddTask(task);

            var tasks = sut.GetTaskDueWithin(new DateTime(2016, 10, 1, 0, 0, 0), new DateTime(2016, 10, 2, 0, 0, 0)).ToArray();

            Assert.That(tasks.Any(x => x.DueDateTime == taskDueDate));
        }

        [OneTimeTearDown]
        public void DeleteTestCalendarIfExists()
        {
            CalendarService service = getCalendarService();
            
            var testLifeCoachCalendarId = getLifeCoachCalendarId(service, LifeCoachTestCalendarName);

            if (testLifeCoachCalendarId != null)
            {                
                var deleteRequest = service.CalendarList.Delete(testLifeCoachCalendarId);
                deleteRequest.Execute();

                var deleteCalRequest = service.Calendars.Delete(testLifeCoachCalendarId);
                deleteCalRequest.Execute();
            }
        }

        private static string getLifeCoachCalendarId(CalendarService service, string calendarName)
        {          
            CalendarListResource.ListRequest listRequest = service.CalendarList.List();
            var calendarList = listRequest.Execute();
            foreach (var calendar in calendarList.Items)
            {
                Console.WriteLine(calendar.Summary);
                Console.WriteLine(calendar.Id);
                if (calendar.Summary == calendarName)
                {
                    return calendar.Id;                    
                }
            }
            return null;            
        }

        private static CalendarService getCalendarService()
        {
            UserCredential credential;
            string secretFile = getSecretFilePath();

            using (var stream = new FileStream(secretFile, FileMode.Open, FileAccess.Read))
            {
                string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/calendar-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                     new[] { CalendarService.Scope.Calendar },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Google Calendar LifeCoach Integration Tests",
            });
            return service;
        }

        private static string getSecretFilePath()
        {
            var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return Path.Combine(directory, "client_secret.json");
        }
    }
}
