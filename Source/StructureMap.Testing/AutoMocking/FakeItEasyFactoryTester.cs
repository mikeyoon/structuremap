using FakeItEasy;
using NUnit.Framework;
using StructureMap.AutoMocking;

namespace StructureMap.Testing.AutoMocking
{
    [TestFixture]
    public class FakeItEasyFactoryTester
    {
        [Test]
        public void can_make_dynamic_mocks()
        {
            var moqFactory = new MoqFactory();
            object fooMock = moqFactory.CreateMock(typeof (ITestMocks));

            fooMock.ShouldNotBeNull();
        }

        [Test]
        public void can_make_partial_mocks()
        {
            var fieFactory = new FakeItEasyFactory();
            var testPartials = (TestPartials)fieFactory.CreateMockThatCallsBase(typeof(TestPartials), new object[0]);

            testPartials.ShouldNotBeNull();
            testPartials.Concrete().ShouldEqual("Concrete");
            testPartials.Virtual().ShouldEqual("Virtual");

            A.CallTo(() => testPartials.Virtual()).Returns("Faked!");
            testPartials.Virtual().ShouldEqual("Faked!");
        }

        [Test]
        public void sample_fie_usage()
        {
            var mock = A.Fake<ITestMocks>();
            A.CallTo(() => mock.Answer()).Returns("Fake");
            mock.Answer().ShouldEqual("Fake");
        }
    }
}