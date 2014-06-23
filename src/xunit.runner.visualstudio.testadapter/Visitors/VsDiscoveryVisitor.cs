using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using System.Text;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;
using Xunit.Abstractions;
using Xunit.Runner.VisualStudio.Settings;

#if !WINDOWS_PHONE_APP
using System.Security.Cryptography;
#else
using Xunit.Serialization;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using System.Runtime.InteropServices.WindowsRuntime;
#endif

namespace Xunit.Runner.VisualStudio.TestAdapter
{
    public class VsDiscoveryVisitor : TestMessageVisitor<IDiscoveryCompleteMessage>, IVsDiscoveryVisitor
    {
        static readonly Action<TestCase, string, string> addTraitThunk = GetAddTraitThunk();
        static readonly Uri uri = new Uri(Constants.ExecutorUri);

        readonly Func<bool> cancelThunk;
        readonly ITestFrameworkDiscoverer discoverer;
        readonly ITestCaseDiscoverySink discoverySink;
        readonly List<ITestCase> lastTestClassTestCases = new List<ITestCase>();
        readonly IMessageLogger logger;
        readonly XunitVisualStudioSettings settings;
        readonly string source;

        string lastTestClass;

        public VsDiscoveryVisitor(string source, ITestFrameworkDiscoverer discoverer, IMessageLogger logger, IDiscoveryContext discoveryContext, ITestCaseDiscoverySink discoverySink, Func<bool> cancelThunk)
        {
            this.source = source;
            this.discoverer = discoverer;
            this.logger = logger;
            this.discoverySink = discoverySink;
            this.cancelThunk = cancelThunk;

            settings = SettingsProvider.Load();

            var settingsProvider = discoveryContext.RunSettings.GetSettings(XunitTestRunSettings.SettingsName) as XunitTestRunSettingsProvider;
            if (settingsProvider != null && settingsProvider.Settings != null)
                settings.Merge(settingsProvider.Settings);
        }

        public int TotalTests { get; private set; }

        public static TestCase CreateVsTestCase(string source, ITestFrameworkDiscoverer discoverer, ITestCase xunitTestCase, XunitVisualStudioSettings settings, bool forceUniqueNames)
        {
            var serializedTestCase = discoverer.Serialize(xunitTestCase);
            var fqTestMethodName = String.Format("{0}.{1}", xunitTestCase.TestMethod.TestClass.Class.Name, xunitTestCase.TestMethod.Method.Name);
            var displayName = settings.GetDisplayName(xunitTestCase.DisplayName, xunitTestCase.TestMethod.Method.Name, fqTestMethodName);
            var uniqueName = forceUniqueNames ? String.Format("{0} ({1})", fqTestMethodName, xunitTestCase.UniqueID) : fqTestMethodName;

            var result = new TestCase(uniqueName, uri, source) { DisplayName = Escape(displayName) };
            result.SetPropertyValue(VsTestRunner.SerializedTestCaseProperty, serializedTestCase);
            result.Id = IdHasher.GuidFromString(uri + xunitTestCase.UniqueID);

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

        public int Finish()
        {
            Finished.WaitOne();
            return TotalTests;
        }

        static Action<TestCase, string, string> GetAddTraitThunk()
        {
            try
            {
                Type testCaseType = typeof(TestCase);
                Type stringType = typeof(string);
                PropertyInfo property = testCaseType.GetRuntimeProperty("Traits");

                if (property == null)
                    return null;

                MethodInfo method = property.PropertyType.GetRuntimeMethod("Add", new[] { typeof(string), typeof(string) });
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
            var testCase = discovery.TestCase;
            var testClass = String.Format("{0}.{1}", testCase.TestMethod.TestClass.Class.Name, testCase.TestMethod.Method.Name);
            if (lastTestClass != testClass)
                SendExistingTestCases();

            lastTestClass = testClass;
            lastTestClassTestCases.Add(testCase);
            TotalTests++;

            return !cancelThunk();
        }

        protected override bool Visit(IDiscoveryCompleteMessage discoveryComplete)
        {
            foreach (var message in discoveryComplete.Warnings)
                logger.SendMessage(TestMessageLevel.Warning, message);

            SendExistingTestCases();

            return !cancelThunk();
        }

        private void SendExistingTestCases()
        {
            var forceUniqueNames = lastTestClassTestCases.Count > 1;

            foreach (var testCase in lastTestClassTestCases)
            {
                discoverySink.SendTestCase(CreateVsTestCase(source, discoverer, testCase, settings, forceUniqueNames));
            }

            lastTestClassTestCases.Clear();
        }

        public static string fqTestMethodName { get; set; }


        internal static class IdHasher
        {
#if !WINDOWS_PHONE_APP
            readonly static HashAlgorithm Hasher = (HashAlgorithm)new SHA1CryptoServiceProvider();
            public static Guid GuidFromString(string data)
            {
                byte[] hash = Hasher.ComputeHash(Encoding.Unicode.GetBytes(data));
                byte[] b = new byte[16];
                Array.Copy((Array)hash, (Array)b, 16);
                return new Guid(b);
            }
            
#else
            readonly static HashAlgorithmProvider Hasher = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);

            public static Guid GuidFromString(string data)
            {

                var buffer = CryptographicBuffer.CreateFromByteArray(Encoding.Unicode.GetBytes(data));
                byte[] hash = Hasher.HashData(buffer).ToArray();
                byte[] b = new byte[16];
                Array.Copy((Array)hash, (Array)b, 16);
                return new Guid(b);

            }
#endif
        }
    }
}