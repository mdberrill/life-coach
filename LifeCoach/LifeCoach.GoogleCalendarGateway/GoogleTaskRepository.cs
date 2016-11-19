using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using LifeCoach.Domain;
using System;
using System.Collections.Generic;
using System.IO;
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

        public void AddTask(Task task)
        {
            var calendarService = getCalendarService();
            var calendarId = getLifeCoachCalendarId(calendarService, _calendarName);

            if (calendarId == null)
                calendarId = createCalendar(calendarService);

            Event evt = new Event();
            evt.Summary = task.Description;
            // set start and end date far into the future

            if (task.DueDateTime == null)
            {
                evt.Start = new EventDateTime() { DateTime = NoDateTimeDate };
                evt.End = new EventDateTime() { DateTime = NoDateTimeDate };
            }
            else
            {
                evt.Start = new EventDateTime() { DateTime = task.DueDateTime.Value };
                evt.End = new EventDateTime() { DateTime = task.DueDateTime.Value };
            }

            var insertEvtReq = calendarService.Events.Insert(evt, calendarId);
            var evtRet =  insertEvtReq.Execute();
            task.Id = evtRet.Id;            
        }

        public IEnumerable<Task> GetTaskWithNoDates()
        {
            var calendarService = getCalendarService();
            var calendarId = getLifeCoachCalendarId(calendarService, _calendarName);

            var getEventsListRequest = calendarService.Events.List(calendarId);
            getEventsListRequest.TimeMin = NoDateTimeDate.AddMinutes(-1);
            getEventsListRequest.TimeMax = NoDateTimeDate.AddMinutes(1);

            var events = getEventsListRequest.Execute();

            foreach (var evt in events.Items)
            {
                yield return new Task(evt.Id, evt.Summary, null) ;
            }
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
