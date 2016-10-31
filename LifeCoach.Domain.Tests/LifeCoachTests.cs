using NUnit.Framework;

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
    }
}
