using System;
using System.Linq.Expressions;
using FakeItEasy;
using NUnit.Framework;
using StructureMap.AutoMocking;

namespace StructureMap.Testing.AutoMocking
{
    [TestFixture]
    public class example_FakeItEasyAutoMocker_usage
    {
        [Test]
        public void verify_an_expected_calls()
        {
            var autoMocker = new FakeItEasyAutoMocker<AutoMockerTester.ConcreteClass>();
            var mockedService = autoMocker.Get<AutoMockerTester.IMockedService>();
            
            int something = 0;

            A.CallTo(() => mockedService.Go()).Invokes(p => something = 4);

            autoMocker.ClassUnderTest.CallService();

            something.ShouldEqual(4);
        }
    }

    [TestFixture]
    public class FakeItEasyAutoMockerTester : AutoMockerTester
    {
        protected override AutoMocker<T> createAutoMocker<T>()
        {
            return new FakeItEasyAutoMocker<T>();
        }

        protected override void setExpectation<T, TResult>(T mock, Expression<Func<T, TResult>> functionCall,
                                                           TResult expectedResult)
        {
            dynamic func = functionCall.Body;
            string name = func.Member.Name;
            string type = func.Member.MemberType.ToString();
            string methodName = type == "Property" ? "get_" + name : name;

            A.CallTo(mock).Where(p => p.Method.Name == methodName).WithReturnType<TResult>().Returns(expectedResult);
        }
    }
}