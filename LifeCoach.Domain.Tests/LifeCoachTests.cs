using NUnit.Framework;
using System;
using System.Linq;

namespace LifeCoach.Domain.Tests
{
    [TestFixture]
    public class LifeCoachTests
    {
        [Test]
        public void Should_be_able_to_create_a_LifeCoach()
        {
            LifeCoach sut = new LifeCoach();
            Assert.Pass();
        }

        [Test]
        public void MeetWithNewClient_WithValidClientName_shouldStartMeetingWithClient()
        {
            LifeCoach sut = new LifeCoach();
            sut.MeetWithNewClient("mike");

            Assert.AreEqual("mike", sut.GetCurrentClientMeetingWith().Name);
        }

        [Test]
        public void NoteClientActivity_withValidActivityName_shouldCreateActivity()
        {
            LifeCoach sut = new LifeCoach();
            sut.MeetWithNewClient("bob");
            sut.NoteClientActivity("Clean the pots");
            Assert.AreEqual("Clean the pots", sut.GetCurrentClientMeetingWith().Activities.First().Name);
        }

        [Test]
        public void NoteClientActivity_withValidActivityNameAndDueDate_shouldCreateActivity()
        {
            LifeCoach sut = new LifeCoach();
            sut.MeetWithNewClient("bob");
            Activity activity = new Activity("Clean the pots");
            activity.DueDateTime = DateTime.Now.AddHours(1);
            sut.NoteClientActivity(activity);
            Assert.AreEqual("Clean the pots", sut.GetCurrentClientMeetingWith().Activities.First().Name);
        }

        [Test]
        public void NoteClientActivity_withDueDateInPast_shouldCreateActivity()
        {
            LifeCoach sut = new LifeCoach();
            sut.MeetWithNewClient("bob");
            Activity activity = new Activity("Clean the pots");
            activity.DueDateTime = DateTime.Now.AddSeconds(-1);
            sut.NoteClientActivity(activity);
            Assert.AreEqual("Clean the pots", sut.GetCurrentClientMeetingWith().Activities.First().Name);
        }

        [Test]
        public void NoteClientActivity_withNameOnly_ShouldCreateActivityWithDueDateBeingEndOfCurrentDay()
        {
            LifeCoach sut = new LifeCoach();
            sut.MeetWithNewClient("bob");
            sut.NoteClientActivity("Clean the pots");
            
            Assert.AreEqual(DateTime.Today.AddDays(1), sut.GetCurrentClientMeetingWith().Activities.First().DueDateTime);
        }
    }
}
