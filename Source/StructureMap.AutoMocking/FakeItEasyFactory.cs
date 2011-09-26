using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace StructureMap.AutoMocking
{
    public class FakeItEasyFactory
    {
        private readonly Type mockOpenType;
        private readonly Type fakeOptionsBuilderType;
        private readonly MethodInfo fakeMethod;

        public FakeItEasyFactory()
        {
            Assembly fakeItEasy = Assembly.Load("FakeItEasy");
            mockOpenType = fakeItEasy.GetType("FakeItEasy.A");
            fakeOptionsBuilderType = fakeItEasy.GetType("FakeItEasy.Creation.IFakeOptionsBuilder`1");

            //mockOpenType = typeof(A);
            if (mockOpenType == null)
                throw new InvalidOperationException("Unable to find Type A in assembly FakeItEasy");

            fakeMethod = mockOpenType.GetMethod("Fake", System.Type.EmptyTypes);
        }

        public object CreateMock(Type type)
        {
            MethodInfo createMethod = fakeMethod.MakeGenericMethod(new[] { type });

            return createMethod.Invoke(null, null);
        }

        public object CreateMockThatCallsBase(Type type, object[] args)
        {
            var optionsBuilderGeneric = fakeOptionsBuilderType.MakeGenericType(new[] { type });
            var optionsBuilderWrapped = fakeOptionsBuilderType.MakeGenericType(new[] {
                    typeof(Action<>).MakeGenericType(new[] {
                        optionsBuilderGeneric
                    })
                });
            var actionTypeWrapped = typeof(Action<>).MakeGenericType(new[] { optionsBuilderWrapped });

            var actionType = typeof(Action<>).MakeGenericType(new[] { 
                optionsBuilderGeneric
            });

            var fakeWithArgsMethod = mockOpenType.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(p => p.Name == "Fake" && p.GetParameters().Count() > 0).FirstOrDefault();
            var fakeWithArgsMethodGeneric = fakeWithArgsMethod.MakeGenericMethod(new[] { type }); //Action<IFakeOptionsBuilder<type>>

            var withArgsMethod = optionsBuilderGeneric.GetMethod("WithArgumentsForConstructor", new Type[] { typeof(IEnumerable<object>) });
            
            var paramValues = Expression.Constant(args);
            var genType = Expression.Parameter(type);
            var param = Expression.Parameter(optionsBuilderGeneric, "var1");
            var arguments = Expression.Parameter(args.GetType(), "var2"); //args.Select(p => Expression.Parameter(p.GetType())).ToArray();
            var invoke = Expression.Call(param, withArgsMethod, paramValues);
            var lambda = Expression.Lambda(actionType, invoke, param);
            
            var fake = fakeWithArgsMethodGeneric.Invoke(null, new object[] { lambda.Compile() });
            var callToMethod = mockOpenType.GetMethod("CallTo", new[] { typeof(object) });
            var result = callToMethod.Invoke(null, new[] { fake });
            var callsBaseTypeMethod = result.GetType().GetMethod("CallsBaseMethod");
            callsBaseTypeMethod.Invoke(result, null);

            //A.CallTo(fake).CallsBaseMethod();

            return fake;
        }
    }
}
