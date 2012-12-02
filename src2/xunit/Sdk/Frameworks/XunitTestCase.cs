using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class XunitTestCase : LongLivedMarshalByRefObject, IMethodTestCase
    {
        public XunitTestCase(IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo factAttribute, IEnumerable<object> arguments = null)
        {
            Arguments = arguments ?? Enumerable.Empty<object>();
            Assembly = assembly;
            Class = type;
            Method = method;
            DisplayName = factAttribute.GetPropertyValue<string>("DisplayName") ?? type.Name + "." + method.Name;
            SkipReason = factAttribute.GetPropertyValue<string>("Skip");

            if (arguments != null)
            {
                var Parameters = arguments.ToArray();

                IParameterInfo[] parameterInfos = method.GetParameters().ToArray();
                string[] displayValues = new string[Math.Max(Parameters.Length, parameterInfos.Length)];
                int idx;

                for (idx = 0; idx < Parameters.Length; idx++)
                    displayValues[idx] = ParameterToDisplayValue(GetParameterName(parameterInfos, idx), Parameters[idx]);

                for (; idx < parameterInfos.Length; idx++)  // Fill-in any missing parameters with "???"
                    displayValues[idx] = GetParameterName(parameterInfos, idx) + ": ???";

                DisplayName = String.Format(CultureInfo.CurrentCulture, "{0}({1})", DisplayName, string.Join(", ", displayValues));
            }

            Traits = new Dictionary<string, string>();

            foreach (IAttributeInfo traitAttribute in Method.GetCustomAttributes(typeof(Trait2Attribute)))
                Traits.Add(traitAttribute.GetPropertyValue<string>("Name"), traitAttribute.GetPropertyValue<string>("Value"));
        }

        public IEnumerable<object> Arguments { get; private set; }

        public IAssemblyInfo Assembly { get; private set; }

        public ITypeInfo Class { get; private set; }

        public string DisplayName { get; private set; }

        public IMethodInfo Method { get; private set; }

        public string SkipReason { get; private set; }

        public int? SourceFileLine { get; internal set; }

        public string SourceFileName { get; internal set; }

        public ITestCollection TestCollection { get; private set; }

        public IDictionary<string, string> Traits { get; private set; }

        static string ConvertToSimpleTypeName(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            Type[] genericTypes = type.GetGenericArguments();
            string[] simpleNames = new string[genericTypes.Length];

            for (int idx = 0; idx < genericTypes.Length; idx++)
                simpleNames[idx] = ConvertToSimpleTypeName(genericTypes[idx]);

            string baseTypeName = type.Name;
            int backTickIdx = type.Name.IndexOf('`');

            return baseTypeName.Substring(0, backTickIdx) + "<" + String.Join(", ", simpleNames) + ">";
        }

        static string GetParameterName(IParameterInfo[] parameters, int index)
        {
            if (index >= parameters.Length)
                return "???";

            return parameters[index].Name;
        }

        static string ParameterToDisplayValue(object parameterValue)
        {
            if (parameterValue == null)
                return "null";

            if (parameterValue is char)
                return "'" + parameterValue + "'";

            string stringParameter = parameterValue as string;
            if (stringParameter != null)
            {
                if (stringParameter.Length > 50)
                    return "\"" + stringParameter.Substring(0, 50) + "\"...";

                return "\"" + stringParameter + "\"";
            }

            return Convert.ToString(parameterValue, CultureInfo.CurrentCulture);
        }

        static string ParameterToDisplayValue(string parameterName, object parameterValue)
        {
            return parameterName + ": " + ParameterToDisplayValue(parameterValue);
        }

        public virtual void Run(IMessageSink messageSink)
        {
            int totalFailed = 0;
            int totalRun = 0;
            int totalSkipped = 0;
            decimal executionTime = 0M;

            messageSink.OnMessage(new TestCaseStarting { TestCase = this });

            var delegatingSink = new DelegatingMessageSink(messageSink, msg =>
            {
                if (msg is ITestFinished)
                {
                    totalRun++;
                    executionTime += ((ITestFinished)msg).ExecutionTime;
                }
                else if (msg is ITestFailed)
                    totalFailed++;
                else if (msg is ITestSkipped)
                    totalSkipped++;
            });

            RunTests(delegatingSink);

            messageSink.OnMessage(new TestCaseFinished
            {
                Assembly = Assembly,
                ExecutionTime = executionTime,
                TestCase = this,
                TestsRun = totalRun,
                TestsFailed = totalFailed,
                TestsSkipped = totalSkipped
            });
        }

        /// <summary>
        /// Run the tests in the test case.
        /// </summary>
        /// <param name="messageSink">The message sink to send results to.</param>
        protected virtual void RunTests(IMessageSink messageSink)
        {
            messageSink.OnMessage(new TestStarting { TestCase = this, DisplayName = DisplayName });

            try
            {
                object testClass = Activator.CreateInstance(((IReflectionTypeInfo)Method.Type).Type);

                if (!String.IsNullOrEmpty(SkipReason))
                    messageSink.OnMessage(new TestSkipped { TestCase = this, DisplayName = DisplayName, Reason = SkipReason });
                else
                {
                    ((IReflectionMethodInfo)Method).MethodInfo.Invoke(testClass, new object[0]);

                    messageSink.OnMessage(new TestPassed { TestCase = this, DisplayName = DisplayName });
                }
            }
            catch (Exception ex)
            {
                while (true)
                {
                    TargetInvocationException tiex = ex as TargetInvocationException;
                    if (tiex == null)
                        break;
                    ex = tiex.InnerException;
                }

                messageSink.OnMessage(new TestFailed { TestCase = this, DisplayName = DisplayName, Exception = ex });
            }

            messageSink.OnMessage(new TestFinished { TestCase = this, DisplayName = DisplayName });
        }
    }
}
