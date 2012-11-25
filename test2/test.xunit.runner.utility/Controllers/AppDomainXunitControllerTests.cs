using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

public class AppDomainXunitControllerTests
{
    public class Construction
    {
        [Fact]
        public void GuardClauses()
        {
            var aex = Assert.Throws<ArgumentException>(
                () => new TestableAppDomainXunitController(assemblyFileName: null, configFileName: null, shadowCopy: true, testFrameworkFileName: "foo.dll")
            );
            Assert.Contains("Could not find file: foo.dll", aex.Message);
            Assert.Equal("testFrameworkFileName", aex.ParamName);
        }

        [Fact]
        public void CreatesAppDomainWithGivenAssemblyNameAndConfigFileName()
        {
            var assemblyFileName = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;

            using (var configFile = new TempFile("foo.config"))
            using (var controller = new TestableAppDomainXunitController(assemblyFileName, configFile.FileName, false, "xunit2.dll"))
            {
                var appDomainSetup = controller.AppDomain.SetupInformation;

                Assert.Equal(Path.GetDirectoryName(assemblyFileName), appDomainSetup.ApplicationBase);
                Assert.Equal(configFile.FileName, appDomainSetup.ConfigurationFile);
                Assert.Null(appDomainSetup.ShadowCopyFiles);
                Assert.True(controller.XunitVersion.StartsWith("2."));
            }
        }

        [Fact]
        public void CreatesAppDomainWithCorrectParametersWhenShadowCopyIsRequested()
        {
            var assemblyFileName = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;

            using (var controller = new TestableAppDomainXunitController(assemblyFileName, null, true, "xunit2.dll"))
            {
                var appDomainSetup = controller.AppDomain.SetupInformation;

                Assert.Equal("true", appDomainSetup.ShadowCopyFiles);
                Assert.Equal(Path.GetDirectoryName(assemblyFileName), appDomainSetup.ShadowCopyDirectories);
                Assert.Contains(Path.GetTempPath(), appDomainSetup.CachePath);
            }
        }
    }

    class TestableAppDomainXunitController : AppDomainXunitController
    {
        public TestableAppDomainXunitController(string assemblyFileName, string configFileName, bool shadowCopy, string testFrameworkFileName)
            : base(assemblyFileName, configFileName, shadowCopy, testFrameworkFileName) { }

        public new AppDomain AppDomain
        {
            get { return base.AppDomain; }
        }

        public override void Find(bool includeSourceInformation, IMessageSink messageSink)
        {
            throw new NotImplementedException();
        }

        public override void Find(ITypeInfo type, bool includeSourceInformation, IMessageSink messageSink)
        {
            throw new NotImplementedException();
        }

        public override void Run(IEnumerable<ITestCase> testMethods, IMessageSink messageSink)
        {
            throw new NotImplementedException();
        }
    }
}
