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
        static Action<TestCase, string, string> addTraitThunk = GetAddTraitThunk();

        static Uri uri = new Uri(Constants.ExecutorUri);

        readonly Func<bool> cancelThunk;
        readonly DiaSessionWrapper diaSession;
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

            diaSession = new DiaSessionWrapper(source);
        }

        public override void Dispose()
        {
            if (diaSession != null)
                diaSession.Dispose();

            base.Dispose();
        }

        public static TestCase CreateVsTestCase(IMessageLogger logger, string source, ITestFrameworkDiscoverer discoverer, ITestCase xunitTestCase, DiaSessionWrapper diaSession = null)
        {
            string typeName = xunitTestCase.Class.Name;
            string methodName = xunitTestCase.Method.Name;
            string serializedTestCase = discoverer.Serialize(xunitTestCase);
            string uniqueName = String.Format("{0}.{1} ({2})", xunitTestCase.Class.Name, xunitTestCase.Method.Name, xunitTestCase.UniqueID);

            var result = new TestCase(uniqueName, uri, source) { DisplayName = xunitTestCase.DisplayName };
            result.SetPropertyValue(VsTestRunner.SerializedTestCaseProperty, serializedTestCase);

            if (addTraitThunk != null)
                foreach (var trait in xunitTestCase.Traits)
                    addTraitThunk(result, trait.Key, trait.Value);

            // TODO: This code belongs in xunit2
            if (diaSession != null)
            {
                DiaNavigationData navigationData = diaSession.GetNavigationData(typeName, methodName);
                if (navigationData != null)
                {
                    result.CodeFilePath = navigationData.FileName;
                    result.LineNumber = navigationData.MinLineNumber;
                }
            }

            return result;
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
            discoverySink.SendTestCase(CreateVsTestCase(logger, source, discoverer, discovery.TestCase, diaSession));

            return !cancelThunk();
        }
    }
}