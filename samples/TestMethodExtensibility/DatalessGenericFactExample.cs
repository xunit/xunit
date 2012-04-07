using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

namespace TestMethodExtensibility
{
    public class DatalessGenericFactExample
    {
        [DatalessGenericFact(typeof(string), typeof(object), typeof(int))]
        public void Generic<T>()
        {
            Console.WriteLine(typeof(T));
        }
    }

    class DatalessGenericFactAttribute : FactAttribute
    {
        Type[] types;

        public DatalessGenericFactAttribute(params Type[] types)
        {
            this.types = types;
        }

        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            foreach (Type type in types)
                yield return new GenericFactCommand(GetTestMethod(method, type), "<" + type.Name + ">");
        }

        private IMethodInfo GetTestMethod(IMethodInfo genericMethod, Type genericType)
        {
            return Reflector.Wrap(genericMethod.MethodInfo.MakeGenericMethod(genericType));
        }

        class GenericFactCommand : FactCommand
        {
            public GenericFactCommand(IMethodInfo method, string displaySuffix)
                : base(method)
            {
                DisplayName = DisplayName + displaySuffix;
            }
        }
    }
}
