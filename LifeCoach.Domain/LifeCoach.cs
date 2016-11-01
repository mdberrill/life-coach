using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace LifeCoach.Domain
{
    public class LifeCoach
    {
        public Client CurrentClient { get; set; }

        public Client MeetWithNewClient(string clientName)
        {
            CurrentClient = new Client(clientName);
            return CurrentClient;
        }

        public Client GetCurrentClientMeetingWith()
        {
            return CurrentClient;
        }

        public void ResumeMeetingWith(Client client)
        {
            CurrentClient = client;
        }

        public void NoteClientActivity(string activityName)
        {
            var activity = new Activity(activityName);
            activity.DueDateTime = DateTime.Today.AddDays(1);
            CurrentClient.AddActivity(activity);
        }

        public void NoteClientActivity(Activity activity)
        {
            CurrentClient.AddActivity(activity);
        }

        public void EndMeetingWith(string clientName)
        {
            CurrentClient = null;
        }
    }

    public class LifeCoachRepository
    {

        public LifeCoach WakeUp()
        {
            XmlSerializer serialiser = new XmlSerializer(typeof(LifeCoach));
            if (File.Exists(".lifecoach/lifecoach.xml"))
            {
                using (StreamReader sr = new StreamReader(".lifecoach/lifecoach.xml"))
                using (XmlReader reader = XmlReader.Create(sr))
                {
                    return serialiser.Deserialize(reader) as LifeCoach;
                }
            }
            else return null;
        }

        public void PutToSleep(LifeCoach lifeCoach)
        {
            XmlSerializer serialiser = new XmlSerializer(typeof(LifeCoach));
            if (!Directory.Exists(".lifecoach"))
                Directory.CreateDirectory(".lifecoach");

            using (StreamWriter sw = new StreamWriter(".lifecoach/lifecoach.xml"))
            using (XmlWriter writer = XmlWriter.Create(sw))
            {
                serialiser.Serialize(writer, lifeCoach);
                writer.Close();
                sw.Close();
            }
            if (lifeCoach.CurrentClient == null) return;
            // save client in file too
            XmlSerializer clientSerialiser = new XmlSerializer(typeof(Client));
            using (StreamWriter sw = new StreamWriter(".lifecoach/" + lifeCoach.CurrentClient.Name + ".xml"))
            using (XmlWriter writer = XmlWriter.Create(sw))
            {
                clientSerialiser.Serialize(writer, lifeCoach.CurrentClient);
                writer.Close();
                sw.Close();
            }
        }

        public void DeleteClient(string clientName)
        {
            if (File.Exists(".lifecoach/" + clientName + ".xml"))
            {
                File.Delete(".lifecoach/" + clientName + ".xml");
            }
        }

        public Client WakeUpInMeetingWith(string clientName)
        {
            XmlSerializer clientSerialiser = new XmlSerializer(typeof(Client));
            if (File.Exists(".lifecoach/" + clientName + ".xml"))
            {
                using (StreamReader sr = new StreamReader(".lifecoach/" + clientName + ".xml"))
                using (XmlReader reader = XmlReader.Create(sr))
                {
                    return clientSerialiser.Deserialize(reader) as Client;
                }
            }
            else return null;
        }
    }


}
