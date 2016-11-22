using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using LifeCoach.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace LifeCoach.GoogleCalendarGateway
{
    public class GoogleTaskRepository : ITaskRepository
    {
        private string _clientSecretFilePath;
        private string _calendarName;
        private static DateTime NoDateTimeDate = new DateTime(2100, 1, 1);
        private const string DONE_TEXT = "Done : ";

        public GoogleTaskRepository(string clientSecretFilePath, string calendarName)
        {
            _clientSecretFilePath = clientSecretFilePath;
            _calendarName = calendarName;
        }

        public Task GetTaskById(string id)
        {
            var calendarService = getCalendarService();
            var calendarId = getLifeCoachCalendarId(calendarService, _calendarName);

            if (calendarId == null)
                throw new Exception("No tasks managed by the life coach exist");
            
            
            var getListRequest = calendarService.Events.List(calendarId);
            var evts = getListRequest.Execute();

            var evtsTasksWithId = getTasksMatchingId(evts, id).ToArray();

            if (evtsTasksWithId.Length == 0)
                throw new Exception("Could not find task with the Id : " + id);

            if (evtsTasksWithId.Length > 1)
                throw new Exception("More than one task was found matching the Id : " + id);

            return evtsTasksWithId[0];
        }

        private IEnumerable<Task> getTasksMatchingId(Events events, string id)
        {
            foreach (Event evt in events.Items)
            {
                if (evt.Id.StartsWith(id))
                {
                    yield return BuildTaskFromEvent(evt);
                }
            }
        }

        public void AddTask(Task task)
        {
            var calendarService = getCalendarService();
            var calendarId = getLifeCoachCalendarId(calendarService, _calendarName);

            if (calendarId == null)
                calendarId = createCalendar(calendarService);

            var evt = BuildEventFromTask(task);

            var insertEvtReq = calendarService.Events.Insert(evt, calendarId);
            var evtRet = insertEvtReq.Execute();
            task.Id = evtRet.Id;
        }

        private static Event BuildEventFromTask(Task task)
        {
            Event evt = new Event();
            evt.Summary = task.Description;

            if (task.IsComplete)
            {
                evt.Summary = DONE_TEXT + evt.Summary;
            }

            if (task.DueDateTime == null)  // then set start and end date far into the future
            {
                evt.Start = new EventDateTime() { DateTime = NoDateTimeDate };
                evt.End = new EventDateTime() { DateTime = NoDateTimeDate };
            }
            else
            {
                evt.Start = new EventDateTime() { DateTime = task.DueDateTime.Value };
                evt.End = new EventDateTime() { DateTime = task.DueDateTime.Value };
            }
            return evt;
        }

        public void UpdateTask(Task task)
        {
            var calendarService = getCalendarService();
            var calendarId = getLifeCoachCalendarId(calendarService, _calendarName);

            if (calendarId == null)
                throw new Exception("No tasks managed by the life coach exist");

            Event evt = BuildEventFromTask(task);
            var updateRequest = calendarService.Events.Update(evt, calendarId, task.Id);
            updateRequest.Execute();
        }

        public IEnumerable<Task> GetTaskWithNoDates()
        {
            return GetTaskDueWithin(NoDateTimeDate.AddMinutes(-1), NoDateTimeDate.AddMinutes(1));
        }

        public IEnumerable<Task> GetTasksDueOn(DateTime value)
        {
            return GetTaskDueWithin(value, value.AddDays(1));
        }

        public IEnumerable<Task> GetTasksDueBetween(DateTime from, DateTime to)
        {
            return GetTaskDueWithin(from, to);
        }

        public IEnumerable<Task> GetTaskDueWithin(DateTime fromDueDateTime, DateTime toDueDateTime)
        {
            var calendarService = getCalendarService();
            var calendarId = getLifeCoachCalendarId(calendarService, _calendarName);

            var getEventsListRequest = calendarService.Events.List(calendarId);
            getEventsListRequest.TimeMin = fromDueDateTime;
            getEventsListRequest.TimeMax = toDueDateTime;

            var events = getEventsListRequest.Execute();

            foreach (var evt in events.Items)
            {
                yield return BuildTaskFromEvent(evt);
            }
        }

        private static Task BuildTaskFromEvent(Event evt)
        {
            bool isComplete = false;
            string summary = evt.Summary;


            if (summary.StartsWith(DONE_TEXT))
            {
                isComplete = true;
                summary = summary.Remove(0, DONE_TEXT.Length);
            }

            if (evt.End.DateTime == NoDateTimeDate)
                return Task.CreateTask(summary, evt.Id, null, isComplete);
            else
                return Task.CreateTask(summary, evt.Id, evt.End.DateTime, isComplete);
        }


        private string createCalendar(CalendarService calendarService)
        {
            var calender = new Calendar();
            calender.Summary = _calendarName;

            var insertNewCalenderRequest = calendarService.Calendars.Insert(calender);
            var cal = insertNewCalenderRequest.Execute();

            CalendarListEntry cle = new CalendarListEntry();
            cle.Id = cal.Id;

            CalendarListResource.InsertRequest insertCalendarRequest = calendarService.CalendarList.Insert(cle);
            var item = insertCalendarRequest.Execute();
            return cal.Id;
        }

        private static string getLifeCoachCalendarId(CalendarService service, string calendarName)
        {
            CalendarListResource.ListRequest listRequest = service.CalendarList.List();
            var calendarList = listRequest.Execute();
            foreach (var calendar in calendarList.Items)
            {
                if (calendar.Summary == calendarName)
                {
                    return calendar.Id;
                }
            }
            return null;
        }


        private CalendarService getCalendarService()
        {
            UserCredential credential;

            var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            using (var stream = new FileStream(_clientSecretFilePath, FileMode.Open, FileAccess.Read))
            {
                string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/calendar-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                     new[] { CalendarService.Scope.Calendar },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Google Calendar LifeCoach Integration Tests",
            });
            return service;
        }


    }
}
