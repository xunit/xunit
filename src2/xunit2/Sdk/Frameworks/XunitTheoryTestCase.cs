using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Represents a test case which runs multiple tests for theory data, either because the
    /// data was not enumerable or because the data was not serializable.
    /// </summary>
    public class XunitTheoryTestCase : XunitTestCase
    {
        public XunitTheoryTestCase(IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo factAttribute)
            : base(assembly, type, method, factAttribute) { }

        protected override bool RunTestsOnMethod(IMessageSink messageSink,
                                                 Type classUnderTest,
                                                 MethodInfo methodUnderTest,
                                                 List<BeforeAfterTestAttribute> beforeAfterAttributes,
                                                 ref decimal executionTime)
        {
            try
            {
                var testMethod = Reflector.Wrap(methodUnderTest);

                var dataAttributes = testMethod.GetCustomAttributes(typeof(DataAttribute));
                foreach (var dataAttribute in dataAttributes)
                {
                    var discovererAttribute = dataAttribute.GetCustomAttributes(typeof(DataDiscovererAttribute)).First();
                    var args = discovererAttribute.GetConstructorArguments().Cast<string>().ToList();
                    var discovererType = Reflector.GetType(args[0], args[1]);
                    IDataDiscoverer discoverer = (IDataDiscoverer)Activator.CreateInstance(discovererType);

                    foreach (object[] dataRow in discoverer.GetData(dataAttribute, testMethod))
                        if (RunTestWithArguments(messageSink, classUnderTest, methodUnderTest, dataRow, GetDisplayNameWithArguments(DisplayName, dataRow), beforeAfterAttributes, ref executionTime))
                            return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                var cancelled = false;

                if (!messageSink.OnMessage(new TestStarting { TestCase = this, TestDisplayName = DisplayName }))
                    cancelled = true;
                else
                {
                    if (!messageSink.OnMessage(new TestFailed(ex.Unwrap()) { TestCase = this, TestDisplayName = DisplayName }))
                        cancelled = true;
                }

                if (!messageSink.OnMessage(new TestFinished { TestCase = this, TestDisplayName = DisplayName }))
                    cancelled = true;

                return cancelled;
            }
        }
    }
}