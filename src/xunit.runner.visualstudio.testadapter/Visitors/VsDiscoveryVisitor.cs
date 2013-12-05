using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Xunit.Abstractions;
using Xunit.Runner.VisualStudio.Settings;

namespace Xunit.Runner.VisualStudio.TestAdapter
{
    public class VsDiscoveryVisitor : TestMessageVisitor<IDiscoveryCompleteMessage>
    {
        static readonly Action<TestCase, string, string> addTraitThunk = GetAddTraitThunk();
        static readonly Uri uri = new Uri(Constants.ExecutorUri);

        readonly Func<bool> cancelThunk;
        readonly ITestFrameworkDiscoverer discoverer;
        readonly ITestCaseDiscoverySink discoverySink;
        readonly IMessageLogger logger;
        readonly XunitVisualStudioSettings settings;
        readonly string source;

        public VsDiscoveryVisitor(string source, ITestFrameworkDiscoverer discoverer, IMessageLogger logger, ITestCaseDiscoverySink discoverySink, Func<bool> cancelThunk)
        {
            this.source = source;
            this.discoverer = discoverer;
            this.logger = logger;
            this.discoverySink = discoverySink;
            this.cancelThunk = cancelThunk;

            settings = SettingsProvider.Load();
        }

        public int TotalTests { get; private set; }

        public static TestCase CreateVsTestCase(string source, ITestFrameworkDiscoverer discoverer, ITestCase xunitTestCase, XunitVisualStudioSettings settings)
        {
            var serializedTestCase = discoverer.Serialize(xunitTestCase);
            var fqTestMethodName = String.Format("{0}.{1}", xunitTestCase.Class.Name, xunitTestCase.Method.Name);
            var uniqueName = String.Format("{0} ({1})", fqTestMethodName, xunitTestCase.UniqueID);
            var displayName = GetDisplayName(xunitTestCase.DisplayName, xunitTestCase.Method.Name, fqTestMethodName, settings);

            var result = new TestCase(uniqueName, uri, source) { DisplayName = Escape(displayName) };
            result.SetPropertyValue(VsTestRunner.SerializedTestCaseProperty, serializedTestCase);

            if (addTraitThunk != null)
                foreach (var key in xunitTestCase.Traits.Keys)
                    foreach (var value in xunitTestCase.Traits[key])
                        addTraitThunk(result, key, value);

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

        static string GetDisplayName(string displayName, string shortMethodName, string fullyQualifiedMethodName, XunitVisualStudioSettings settings)
        {
            if (settings.NameDisplay == NameDisplay.Full)
                return displayName;

            return displayName == fullyQualifiedMethodName ? shortMethodName : displayName;
        }

        protected override bool Visit(ITestCaseDiscoveryMessage discovery)
        {
            discoverySink.SendTestCase(CreateVsTestCase(source, discoverer, discovery.TestCase, settings));
            TotalTests++;

            return !cancelThunk();
        }

        public static string fqTestMethodName { get; set; }
    }
}