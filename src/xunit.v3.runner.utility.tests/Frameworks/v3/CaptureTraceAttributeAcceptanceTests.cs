#if NETFRAMEWORK

using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;

public class CaptureTraceAttributeAcceptanceTests
{
	[Fact]
	public async ValueTask CaptureAvailableInConstructor()
	{
		// We use Debug rather than Trace, because we're making a Debug build. Using Trace requires
		// setting the TRACE symbol.
		var code = /* lang=c#-test */ """
			using System.Diagnostics;
			using Xunit;

			[assembly: CaptureTrace]

			public class TestClass
			{
				public TestClass() { Debug.WriteLine("Hello from TestClass"); }

				[Fact]
				public void TestMethod() { }
			}
			""";
		using var testAssembly = await CSharpAcceptanceTestV3Assembly.Create(code);

		var assemblyMetadata = AssemblyUtility.GetAssemblyMetadata(testAssembly.FileName);
		Assert.NotNull(assemblyMetadata);

		var projectAssembly = new XunitProjectAssembly(new XunitProject(), testAssembly.FileName, assemblyMetadata);
		var frontController = XunitFrontController.Create(projectAssembly);
		Assert.NotNull(frontController);

		var messageSink = SpyMessageSink<ITestAssemblyFinished>.Create();
		var settings = new FrontControllerFindAndRunSettings(TestData.TestFrameworkDiscoveryOptions(), TestData.TestFrameworkExecutionOptions());
		frontController.FindAndRun(messageSink, settings);

		if (!messageSink.Finished.WaitOne(30_000))
			throw new InvalidOperationException("Execution did not complete in time");

		var passed = Assert.Single(messageSink.Messages.OfType<ITestPassed>());
		Assert.Equal("Hello from TestClass" + Environment.NewLine, passed.Output);
	}
}

#endif
