using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit.Extensions
{
    /// <summary>
    /// Marks a test method as being a data theory. Data theories are tests which are fed
    /// various bits of data from a data source, mapping to parameters on the test method.
    /// If the data source contains multiple rows, then the test method is executed
    /// multiple times (once with each data row).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TheoryAttribute : FactAttribute
    {
        /// <summary>
        /// Creates instances of <see cref="TheoryCommand"/> which represent individual intended
        /// invocations of the test method, one per data row in the data source.
        /// </summary>
        /// <param name="method">The method under test</param>
        /// <returns>An enumerator through the desired test method invocations</returns>
        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            List<ITestCommand> results = new List<ITestCommand>();

            try
            {
                foreach (object[] dataItems in GetData(method.MethodInfo))
                {
                    IMethodInfo testMethod = method;
                    Type[] resolvedTypes = null;

                    if (method.MethodInfo != null && method.MethodInfo.IsGenericMethodDefinition)
                    {
                        resolvedTypes = ResolveGenericTypes(method, dataItems);
                        testMethod = Reflector.Wrap(method.MethodInfo.MakeGenericMethod(resolvedTypes));
                    }

                    results.Add(new TheoryCommand(testMethod, dataItems, resolvedTypes));
                }

                if (results.Count == 0)
                    results.Add(new LambdaTestCommand(method, () =>
                    {
                        throw new InvalidOperationException(string.Format("No data found for {0}.{1}",
                                                                          method.TypeName,
                                                                          method.Name));
                    }));
            }
            catch (Exception ex)
            {
                results.Clear();
                results.Add(new LambdaTestCommand(method, () =>
                {
                    throw new InvalidOperationException(string.Format("An exception was thrown while getting data for theory {0}.{1}:\r\n" + ex.ToString(),
                                                                      method.TypeName,
                                                                      method.Name));
                }));
            }

            return results;
        }

        static IEnumerable<object[]> GetData(MethodInfo method)
        {
            foreach (DataAttribute attr in method.GetCustomAttributes(typeof(DataAttribute), false))
            {
                ParameterInfo[] parameterInfos = method.GetParameters();
                Type[] parameterTypes = new Type[parameterInfos.Length];

                for (int idx = 0; idx < parameterInfos.Length; idx++)
                    parameterTypes[idx] = parameterInfos[idx].ParameterType;

                IEnumerable<object[]> attrData = attr.GetData(method, parameterTypes);

                if (attrData != null)
                    foreach (object[] dataItems in attrData)
                        yield return dataItems;
            }
        }

        static Type ResolveGenericType(Type genericType, object[] parameters, ParameterInfo[] parameterInfos)
        {
            bool sawNullValue = false;
            Type matchedType = null;

            for (int idx = 0; idx < parameterInfos.Length; ++idx)
                if (parameterInfos[idx].ParameterType == genericType)
                {
                    object parameterValue = parameters[idx];

                    if (parameterValue == null)
                        sawNullValue = true;
                    else if (matchedType == null)
                        matchedType = parameterValue.GetType();
                    else if (matchedType != parameterValue.GetType())
                        return typeof(object);
                }

            if (matchedType == null)
                return typeof(object);

            return sawNullValue && matchedType.IsValueType ? typeof(object) : matchedType;
        }

        static Type[] ResolveGenericTypes(IMethodInfo method, object[] parameters)
        {
            Type[] genericTypes = method.MethodInfo.GetGenericArguments();
            Type[] resolvedTypes = new Type[genericTypes.Length];
            ParameterInfo[] parameterInfos = method.MethodInfo.GetParameters();

            for (int idx = 0; idx < genericTypes.Length; ++idx)
                resolvedTypes[idx] = ResolveGenericType(genericTypes[idx], parameters, parameterInfos);

            return resolvedTypes;
        }

        class LambdaTestCommand : TestCommand
        {
            readonly Assert.ThrowsDelegate lambda;

            public LambdaTestCommand(IMethodInfo method, Assert.ThrowsDelegate lambda)
                : base(method, null, 0)
            {
                this.lambda = lambda;
            }

            public override bool ShouldCreateInstance
            {
                get { return false; }
            }

            public override MethodResult Execute(object testClass)
            {
                try
                {
                    lambda();
                    return new PassedResult(testMethod, DisplayName);
                }
                catch (Exception ex)
                {
                    return new FailedResult(testMethod, ex, DisplayName);
                }
            }
        }
    }
}