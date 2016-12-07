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

        public void DeleteTask(Task task)
        {
            var calendarService = getCalendarService();
            var calendarId = getLifeCoachCalendarId(calendarService, _calendarName);

            var deleteRequest = calendarService.Events.Delete(calendarId, task.Id);
            deleteRequest.Execute();
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

        private static Event BuildEventFromTask(Task task)
        {
            Event evt = new Event();
            evt.Summary = task.Description;
            evt.ExtendedProperties = new Event.ExtendedPropertiesData();
            evt.ExtendedProperties.Private__ = new Dictionary<string, string>();
            evt.Start = new EventDateTime() { DateTime = NoDateTimeDate };
            evt.End = new EventDateTime() { DateTime = NoDateTimeDate };

            if (task.IsComplete)
            {
                evt.ExtendedProperties.Private__["Done"] = "true";
            }

            if (task.IsDeleted)
            {
                evt.ExtendedProperties.Private__["Deleted"] = "true";
            }

          

            if (task.DueDateTime != null)
            {                
                evt.End = new EventDateTime() { DateTime = task.DueDateTime.Value };
            }            
            if (task.StartDateTime != null)
            {
                evt.Start = new EventDateTime() { DateTime = task.StartDateTime.Value };
            }
            // make sure that if either the start and end is defined that the NoDateTimeDate is not used at all
            if (evt.End.DateTime == NoDateTimeDate && evt.Start.DateTime != NoDateTimeDate)
                evt.End = evt.Start;

            if (evt.Start.DateTime == NoDateTimeDate && evt.End.DateTime != NoDateTimeDate)
                evt.Start = evt.End;

            return evt;
        }

        private static Task BuildTaskFromEvent(Event evt)
        {
            bool isComplete = false;
            bool isDeleted = false;
            string summary = evt.Summary;

            if (evt.ExtendedProperties != null && evt.ExtendedProperties.Private__!= null && evt.ExtendedProperties.Private__.ContainsKey("Done") 
                && evt.ExtendedProperties.Private__["Done"] == "true")
            {
                isComplete = true;
            }

            if (evt.ExtendedProperties != null && evt.ExtendedProperties.Private__ != null && evt.ExtendedProperties.Private__.ContainsKey("Deleted") 
                && evt.ExtendedProperties.Private__["Deleted"] == "true")
            {
                isDeleted = true;
            }
            
            return Task.CreateTask(summary,
                evt.Id,
                evt.End.DateTime != NoDateTimeDate ? evt.End.DateTime : null,
                isComplete,
                isDeleted,
                evt.Start.DateTime != NoDateTimeDate ? evt.Start.DateTime : null);
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
