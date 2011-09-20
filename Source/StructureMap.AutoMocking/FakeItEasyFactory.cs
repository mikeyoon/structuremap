using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FakeItEasy;
using System.Reflection;
using System.Linq.Expressions;

namespace StructureMap.AutoMocking
{
    public class FakeItEasyFactory
    {
        private readonly Type mockOpenType;

        public FakeItEasyFactory()
        {
            mockOpenType = typeof(A);
            if (mockOpenType == null)
                throw new InvalidOperationException("Unable to find Type A in assembly FakeItEasy");
        }

        public object CreateMock(Type type)
        {
            MethodInfo mi = mockOpenType.GetMethod("Fake", System.Type.EmptyTypes);
            MethodInfo createMethod = mi.MakeGenericMethod(new[] { type });

            return createMethod.Invoke(null, null);
        }

        protected static void DoSomething<T>(FakeItEasy.Creation.IFakeOptionsBuilder<T> builder, IEnumerable<object> args)
        {
            builder.WithArgumentsForConstructor(args);
        }

        public object CreateMockThatCallsBase(Type type, object[] args)
        {
            var methods = mockOpenType.GetMethods(BindingFlags.Static | BindingFlags.Public);
            
            var optionsBuilderType = typeof(FakeItEasy.Creation.IFakeOptionsBuilder<>);
            var optionsBuilderGeneric = optionsBuilderType.MakeGenericType(new[] { type } );
            var optionsBuilderWrapped = typeof(FakeItEasy.Creation.IFakeOptionsBuilder<>).MakeGenericType(new[] {
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
            
            return fakeWithArgsMethodGeneric.Invoke(null, new object[] { lambda.Compile() });
        }
    }
}
