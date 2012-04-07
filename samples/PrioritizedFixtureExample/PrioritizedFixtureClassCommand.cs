using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit.Extensions
{
    public class PrioritizedFixtureClassCommand : ITestClassCommand
    {
        // Delegate most of the work to the existing TestClassCommand class so that we
        // can preserve any existing behavior (like supporting IUseFixture<T>).
        readonly TestClassCommand cmd = new TestClassCommand();

        public object ObjectUnderTest
        {
            get { return cmd.ObjectUnderTest; }
        }

        public ITypeInfo TypeUnderTest
        {
            get { return cmd.TypeUnderTest; }
            set { cmd.TypeUnderTest = value; }
        }

        public int ChooseNextTest(ICollection<IMethodInfo> testsLeftToRun)
        {
            // Always run the next test in the list, since the list is already ordered
            return 0;
        }

        public Exception ClassFinish()
        {
            return cmd.ClassFinish();
        }

        public Exception ClassStart()
        {
            return cmd.ClassStart();
        }

        public IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod)
        {
            return cmd.EnumerateTestCommands(testMethod);
        }

        public IEnumerable<IMethodInfo> EnumerateTestMethods()
        {
            var sortedMethods = new SortedDictionary<int, List<IMethodInfo>>();

            foreach (IMethodInfo method in cmd.EnumerateTestMethods())
            {
                int priority = 0;

                foreach (IAttributeInfo attr in method.GetCustomAttributes(typeof(TestPriorityAttribute)))
                    priority = attr.GetPropertyValue<int>("Priority");

                GetOrCreate(sortedMethods, priority).Add(method);
            }

            foreach (int priority in sortedMethods.Keys)
                foreach (IMethodInfo method in sortedMethods[priority])
                    yield return method;
        }

        public bool IsTestMethod(IMethodInfo testMethod)
        {
            return cmd.IsTestMethod(testMethod);
        }

        // Dictionary helper method

        static TValue GetOrCreate<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            TValue result;

            if (!dictionary.TryGetValue(key, out result))
            {
                result = new TValue();
                dictionary[key] = result;
            }

            return result;
        }
    }
}