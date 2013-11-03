using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Xunit.Abstractions;

namespace Xunit.Runner.VisualStudio
{
    public class VsDiscoveryVisitor : TestMessageVisitor<IDiscoveryCompleteMessage>
    {
        static readonly Action<TestCase, string, string> addTraitThunk = GetAddTraitThunk();

        static readonly Uri uri = new Uri(Constants.ExecutorUri);

        readonly Func<bool> cancelThunk;
        readonly ITestFrameworkDiscoverer discoverer;
        readonly ITestCaseDiscoverySink discoverySink;
        readonly IMessageLogger logger;
        readonly string source;

        public VsDiscoveryVisitor(string source, ITestFrameworkDiscoverer discoverer, IMessageLogger logger, ITestCaseDiscoverySink discoverySink, Func<bool> cancelThunk)
        {
            this.source = source;
            this.discoverer = discoverer;
            this.logger = logger;
            this.discoverySink = discoverySink;
            this.cancelThunk = cancelThunk;
        }

        public static TestCase CreateVsTestCase(IMessageLogger logger, string source, ITestFrameworkDiscoverer discoverer, ITestCase xunitTestCase)
        {
            string serializedTestCase = discoverer.Serialize(xunitTestCase);
            string uniqueName = String.Format("{0}.{1} ({2})", xunitTestCase.Class.Name, xunitTestCase.Method.Name, xunitTestCase.UniqueID);

            var result = new TestCase(uniqueName, uri, source) { DisplayName = Escape(xunitTestCase.DisplayName) };
            result.SetPropertyValue(VsTestRunner.SerializedTestCaseProperty, serializedTestCase);

            if (addTraitThunk != null)
                foreach (var trait in xunitTestCase.Traits)
                    addTraitThunk(result, trait.Key, trait.Value);

            result.CodeFilePath = xunitTestCase.SourceInformation.FileName;
            result.LineNumber = xunitTestCase.SourceInformation.LineNumber.GetValueOrDefault();

            return result;
        }

        static string Escape(string value)
        {
            if (value == null)
                return String.Empty;

            return value.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
        }

        static Action<TestCase, string, string> GetAddTraitThunk()
        {
            try
            {
                Type testCaseType = typeof(TestCase);
                Type stringType = typeof(string);
                PropertyInfo property = testCaseType.GetProperty("Traits");

                if (property == null)
                    return null;

                MethodInfo method = property.PropertyType.GetMethod("Add", new[] { typeof(string), typeof(string) });
                if (method == null)
                    return null;

                var thisParam = Expression.Parameter(testCaseType, "this");
                var nameParam = Expression.Parameter(stringType, "name");
                var valueParam = Expression.Parameter(stringType, "value");
                var instance = Expression.Property(thisParam, property);
                var body = Expression.Call(instance, method, new[] { nameParam, valueParam });

                return Expression.Lambda<Action<TestCase, string, string>>(body, thisParam, nameParam, valueParam).Compile();
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected override bool Visit(ITestCaseDiscoveryMessage discovery)
        {
            discoverySink.SendTestCase(CreateVsTestCase(logger, source, discoverer, discovery.TestCase));

            return !cancelThunk();
        }
    }
}