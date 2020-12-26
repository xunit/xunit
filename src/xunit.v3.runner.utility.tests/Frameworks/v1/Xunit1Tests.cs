#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web.UI;
using NSubstitute;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Runner.v1;
using Xunit.Sdk;
using Xunit.v3;

public class Xunit1Tests
{
	static readonly string OsSpecificAssemblyPath;

	static Xunit1Tests()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			OsSpecificAssemblyPath = @"C:\Users\bradwilson\assembly.dll";
		else
			OsSpecificAssemblyPath = "/home/bradwilson/assembly.dll";
	}

	public class Constructor
	{
		[Fact]
		public void UsesConstructorArgumentsToCreateExecutor()
		{
			var folder = Path.GetDirectoryName(OsSpecificAssemblyPath);

			var xunit1 = new TestableXunit1("AssemblyName.dll", "ConfigFile.config", shadowCopy: true, shadowCopyFolder: folder);

			Assert.Equal("AssemblyName.dll", xunit1.Executor_TestAssemblyFileName);
			Assert.Equal("ConfigFile.config", xunit1.Executor_ConfigFileName);
			Assert.True(xunit1.Executor_ShadowCopy);
			Assert.Equal(folder, xunit1.Executor_ShadowCopyFolder);
		}
	}

	public class Dispose
	{
		[Fact]
		public async ValueTask DisposesExecutor()
		{
			var xunit1 = new TestableXunit1();
			_ = xunit1.TestFrameworkDisplayName;  // Ensure the executor gets created

			await xunit1.DisposeAsync();

			xunit1.Executor.Received(1).Dispose();
		}
	}

	public class TestFrameworkDisplayName
	{
		[Fact]
		public void ReturnsDisplayNameFromExecutor()
		{
			var xunit1 = new TestableXunit1();
			xunit1.Executor.TestFrameworkDisplayName.Returns("Test Framework Display Name");

			var result = xunit1.TestFrameworkDisplayName;

			Assert.Equal("Test Framework Display Name", result);
		}
	}

	public class Find
	{
		[Fact]
		public void FindByAssemblyReturnsAllTestMethodsFromExecutorXml()
		{
			var xml = @"
<assembly>
	<class name='Type1'>
		<method name='Method1 Display Name' type='Type1' method='Method1'/>
	</class>
	<class name='SpecialType'>
		<method name='SpecialType.SkippedMethod' type='SpecialType' method='SkippedMethod' skip='I am not run'/>
		<method name='SpecialType.MethodWithTraits' type='SpecialType' method='MethodWithTraits'>
			<traits>
				<trait name='Trait1' value='Value1'/>
				<trait name='Trait2' value='Value2'/>
			</traits>
		</method>
	</class>
</assembly>";

			var xunit1 = new TestableXunit1();
			xunit1
				.Executor
				.WhenForAnyArgs(x => x.EnumerateTests(null))
				.Do(callInfo => callInfo.Arg<ICallbackEventHandler>().RaiseCallbackEvent(xml));
			var sink = new TestableTestDiscoverySink();

			xunit1.Find(includeSerialization: false, includeSourceInformation: false, sink);
			sink.Finished.WaitOne();

			Assert.Collection(
				sink.TestCases,
				testCase =>
				{
					Assert.Equal("Type1", testCase.TestMethod.TestClass.Class.Name);
					Assert.Equal("Method1", testCase.TestMethod.Method.Name);
					Assert.Equal("Method1 Display Name", testCase.DisplayName);
					Assert.Null(testCase.SkipReason);
					Assert.Empty(testCase.Traits);
				},
				testCase =>
				{
					Assert.Equal("SpecialType", testCase.TestMethod.TestClass.Class.Name);
					Assert.Equal("SkippedMethod", testCase.TestMethod.Method.Name);
					Assert.Equal("SpecialType.SkippedMethod", testCase.DisplayName);
					Assert.Equal("I am not run", testCase.SkipReason);
				},
				testCase =>
				{
					Assert.Equal("SpecialType", testCase.TestMethod.TestClass.Class.Name);
					Assert.Equal("MethodWithTraits", testCase.TestMethod.Method.Name);
					Assert.Equal("SpecialType.MethodWithTraits", testCase.DisplayName);
					Assert.Collection(
						testCase.Traits.Keys,
						key =>
						{
							Assert.Equal("Trait1", key);
							Assert.Collection(
								testCase.Traits[key],
								value => Assert.Equal("Value1", value)
							);
						},
						key =>
						{
							Assert.Equal("Trait2", key);
							Assert.Collection(
								testCase.Traits[key],
								value => Assert.Equal("Value2", value)
							);
						}
					);
				}
			);
		}

		[Fact]
		public void FindByTypesReturnsOnlyMethodsInTheGivenType()
		{
			var xml = @"
<assembly>
	<class name='Type1'>
		<method name='Method1 Display Name' type='Type1' method='Method1'/>
	</class>
	<class name='Type2'>
		<method name='Type2.Method1' type='Type2' method='Method1'/>
		<method name='Type2.Method2' type='Type2' method='Method2'/>
	</class>
</assembly>";

			var xunit1 = new TestableXunit1();
			xunit1
				.Executor
				.WhenForAnyArgs(x => x.EnumerateTests(null))
				.Do(callInfo => callInfo.Arg<ICallbackEventHandler>().RaiseCallbackEvent(xml));
			var sink = new TestableTestDiscoverySink();

			xunit1.Find("Type2", includeSerialization: false, includeSourceInformation: false, sink);
			sink.Finished.WaitOne();

			Assert.Collection(
				sink.TestCases,
				testCase => Assert.Equal("Type2.Method1", testCase.DisplayName),
				testCase => Assert.Equal("Type2.Method2", testCase.DisplayName)
			);
		}

		[Fact]
		public void TestCasesUseInformationFromSourceInformationProvider()
		{
			var xml = @"
<assembly>
	<class name='Type2'>
		<method name='Type2.Method1' type='Type2' method='Method1'/>
		<method name='Type2.Method2' type='Type2' method='Method2'/>
	</class>
</assembly>";

			var xunit1 = new TestableXunit1();
			xunit1
				.Executor
				.WhenForAnyArgs(x => x.EnumerateTests(null))
				.Do(callInfo => callInfo.Arg<ICallbackEventHandler>().RaiseCallbackEvent(xml));
			xunit1.SourceInformationProvider
				.GetSourceInformation(null, null)
				.ReturnsForAnyArgs(callInfo => new _SourceInformation { FileName = $"File for {callInfo.Args()[0]}.{callInfo.Args()[1]}" });
			var sink = new TestableTestDiscoverySink();

			xunit1.Find(includeSerialization: false, includeSourceInformation: true, sink);
			sink.Finished.WaitOne();

			Assert.Collection(sink.TestCases,
				testCase => Assert.Equal("File for Type2.Method1", testCase.SourceInformation?.FileName),
				testCase => Assert.Equal("File for Type2.Method2", testCase.SourceInformation?.FileName)
			);
		}

		[Fact]
		public void CanIncludeSerializationOfTestCase()
		{
			var xml = @"
<assembly>
	<class name='Type2'>
		<method name='Type2.Method1' type='Type2' method='Method1'/>
		<method name='Type2.Method2' type='Type2' method='Method2'/>
	</class>
</assembly>";

			var xunit1 = new TestableXunit1();
			xunit1
				.Executor
				.WhenForAnyArgs(x => x.EnumerateTests(null))
				.Do(callInfo => callInfo.Arg<ICallbackEventHandler>().RaiseCallbackEvent(xml));
			var sink = new TestableTestDiscoverySink();

			xunit1.Find(includeSerialization: true, includeSourceInformation: false, sink);
			sink.Finished.WaitOne();

			Assert.Collection(
				sink.DiscoveredTestCases,
				testCase => Assert.IsType<Xunit1TestCase>(xunit1.Deserialize(testCase.Serialization!)),
				testCase => Assert.IsType<Xunit1TestCase>(xunit1.Deserialize(testCase.Serialization!))
			);
		}

		[Fact]
		public void DiscoveryIncludesStartMessage()
		{
			var xml = @"<assembly />";

			var xunit1 = new TestableXunit1();
			xunit1
				.Executor
				.WhenForAnyArgs(x => x.EnumerateTests(null))
				.Do(callInfo => callInfo.Arg<ICallbackEventHandler>().RaiseCallbackEvent(xml));
			var sink = new TestableTestDiscoverySink();

			xunit1.Find(includeSerialization: false, includeSourceInformation: true, sink);
			sink.Finished.WaitOne();

			Assert.True(sink.StartSeen);
		}
	}

	public class Run
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void RunWithTestCases(bool serializeTestCases)
		{
			var assembly = new Xunit1TestAssembly("assembly.dll", "config");
			var collection = new Xunit1TestCollection(assembly);
			var type1 = new Xunit1TestClass(collection, "type1");
			var type2 = new Xunit1TestClass(collection, "type2");
			var testCases = new[] {
				new Xunit1TestCase(new Xunit1TestMethod(type1, "passing"), "type1.passing"),
				new Xunit1TestCase(new Xunit1TestMethod(type1, "failing"), "type1.failing"),
				new Xunit1TestCase(new Xunit1TestMethod(type2, "skipping"), "type2.skipping"),
				new Xunit1TestCase(new Xunit1TestMethod(type2, "skipping_with_start"), "type2.skipping_with_start")
			};

			var xunit1 = new TestableXunit1();
			xunit1
				.Executor
				.TestFrameworkDisplayName
				.Returns("Test framework display name");
			xunit1
				.Executor
				.When(x => x.RunTests("type1", Arg.Any<List<string>>(), Arg.Any<ICallbackEventHandler>()))
				.Do(callInfo =>
				{
					var callback = callInfo.Arg<ICallbackEventHandler>();
					callback.RaiseCallbackEvent("<start name='type1.passing' type='type1' method='passing'/>");
					callback.RaiseCallbackEvent("<test name='type1.passing' type='type1' method='passing' result='Pass' time='1.000'/>");
					callback.RaiseCallbackEvent("<start name='type1.failing' type='type1' method='failing'/>");
					callback.RaiseCallbackEvent("<test name='type1.failing' type='type1' method='failing' result='Fail' time='0.234'><failure exception-type='Xunit.MockFailureException'><message>Failure message</message><stack-trace>Stack trace</stack-trace></failure></test>");
					callback.RaiseCallbackEvent("<class name='type1' time='1.234' total='2' failed='1' skipped='0'/>");
				});
			xunit1
				.Executor
				.When(x => x.RunTests("type2", Arg.Any<List<string>>(), Arg.Any<ICallbackEventHandler>()))
				.Do(callInfo =>
				{
					var callback = callInfo.Arg<ICallbackEventHandler>();
					// Note. Skip does not send a start packet, unless you use a custom Fact
					callback.RaiseCallbackEvent("<test name='type2.skipping' type='type2' method='skipping' result='Skip'><reason><message>Skip message</message></reason></test>");
					callback.RaiseCallbackEvent("<start name='type2.skipping_with_start' type='type2' method='skipping_with_start'/>");
					callback.RaiseCallbackEvent("<test name='type2.skipping_with_start' type='type2' method='skipping_with_start' result='Skip'><reason><message>Skip message</message></reason></test>");
					callback.RaiseCallbackEvent("<class name='type2' time='0.000' total='1' failed='0' skipped='1'/>");
				});
			using var sink = SpyMessageSink<_TestAssemblyFinished>.Create();

			if (serializeTestCases)
				xunit1.Run(testCases.Select(testCase => SerializationHelper.Serialize(testCase)).ToList(), sink);
			else
				xunit1.Run(testCases, sink);

			sink.Finished.WaitOne();

			var firstTestCase = testCases[0];

			var testCollection = firstTestCase.TestMethod.TestClass.TestCollection;
			Assert.Null(testCollection.CollectionDefinition);
			Assert.Equal("xUnit.net v1 Tests for assembly.dll", testCollection.DisplayName);
			Assert.Equal("afe079e5b264c1b49ec973cfeee5c84b8701337135fcd737ea00f96166f3a7aa", testCollection.UniqueID);

			var testAssembly = testCollection.TestAssembly;
			Assert.Equal("assembly.dll", testAssembly.Assembly.AssemblyPath);
			Assert.Equal("assembly", testAssembly.Assembly.Name);
			Assert.Equal("config", testAssembly.ConfigFileName);
			Assert.Equal("fea632f3664b106574ca98fd309f200770f1b0cc4b8f745abfe95a575b84c9a5", testAssembly.UniqueID);
			Assert.Equal(new Version(0, 0, 0, 0), testAssembly.Version);

			Assert.Collection(
				sink.Messages,
				message =>
				{
					var assemblyStarting = Assert.IsType<_TestAssemblyStarting>(message);
					Assert.Equal("assembly", assemblyStarting.AssemblyName);
					Assert.Equal("assembly.dll", assemblyStarting.AssemblyPath);
					Assert.Equal("fea632f3664b106574ca98fd309f200770f1b0cc4b8f745abfe95a575b84c9a5", assemblyStarting.AssemblyUniqueID);
					Assert.Equal("config", assemblyStarting.ConfigFilePath);
					Assert.Null(assemblyStarting.TargetFramework);  // Always null with v1
					Assert.Contains("-bit .NET ", assemblyStarting.TestEnvironment);
					Assert.Equal("Test framework display name", assemblyStarting.TestFrameworkDisplayName);
				},
				message =>
				{
					var collectionStarting = Assert.IsType<_TestCollectionStarting>(message);
					Assert.Equal("fea632f3664b106574ca98fd309f200770f1b0cc4b8f745abfe95a575b84c9a5", collectionStarting.AssemblyUniqueID);
					Assert.Null(collectionStarting.TestCollectionClass);
					Assert.Equal("xUnit.net v1 Tests for assembly.dll", collectionStarting.TestCollectionDisplayName);
					Assert.Equal("afe079e5b264c1b49ec973cfeee5c84b8701337135fcd737ea00f96166f3a7aa", collectionStarting.TestCollectionUniqueID);
				},
				message =>
				{
					var testClassStarting = Assert.IsType<_TestClassStarting>(message);
					Assert.Equal("fea632f3664b106574ca98fd309f200770f1b0cc4b8f745abfe95a575b84c9a5", testClassStarting.AssemblyUniqueID);
					Assert.Equal("type1", testClassStarting.TestClass);
					Assert.Equal("49fb0c20ea941ab02cf91d5efdb3ef118df57da17afceae869238653061b1747", testClassStarting.TestClassUniqueID);
					Assert.Equal("afe079e5b264c1b49ec973cfeee5c84b8701337135fcd737ea00f96166f3a7aa", testClassStarting.TestCollectionUniqueID);
				},
				message =>
				{
					var testMethodStarting = Assert.IsAssignableFrom<_TestMethodStarting>(message);
					Assert.Equal("fea632f3664b106574ca98fd309f200770f1b0cc4b8f745abfe95a575b84c9a5", testMethodStarting.AssemblyUniqueID);
					Assert.Equal("49fb0c20ea941ab02cf91d5efdb3ef118df57da17afceae869238653061b1747", testMethodStarting.TestClassUniqueID);
					Assert.Equal("afe079e5b264c1b49ec973cfeee5c84b8701337135fcd737ea00f96166f3a7aa", testMethodStarting.TestCollectionUniqueID);
					Assert.Equal("passing", testMethodStarting.TestMethod);
					Assert.Equal("f7708079f4f28f872d4ab7958e87d8bcf6ab3f33f88217cfd3f4a82222c7b7e1", testMethodStarting.TestMethodUniqueID);
				},
				message =>
				{
					var testCaseStarting = Assert.IsAssignableFrom<_TestCaseStarting>(message);
					Assert.Equal("fea632f3664b106574ca98fd309f200770f1b0cc4b8f745abfe95a575b84c9a5", testCaseStarting.AssemblyUniqueID);
					Assert.Null(testCaseStarting.SkipReason);
					Assert.Null(testCaseStarting.SourceFilePath);
					Assert.Null(testCaseStarting.SourceLineNumber);
					Assert.Equal("type1.passing", testCaseStarting.TestCaseDisplayName);
					Assert.Equal("type1.passing (assembly.dll)", testCaseStarting.TestCaseUniqueID);
					Assert.Equal("49fb0c20ea941ab02cf91d5efdb3ef118df57da17afceae869238653061b1747", testCaseStarting.TestClassUniqueID);
					Assert.Equal("afe079e5b264c1b49ec973cfeee5c84b8701337135fcd737ea00f96166f3a7aa", testCaseStarting.TestCollectionUniqueID);
					Assert.Equal("f7708079f4f28f872d4ab7958e87d8bcf6ab3f33f88217cfd3f4a82222c7b7e1", testCaseStarting.TestMethodUniqueID);
					Assert.Empty(testCaseStarting.Traits);
				},
				message =>
				{
					var testStarting = Assert.IsAssignableFrom<_TestStarting>(message);
					Assert.Equal("fea632f3664b106574ca98fd309f200770f1b0cc4b8f745abfe95a575b84c9a5", testStarting.AssemblyUniqueID);
					Assert.Equal("type1.passing (assembly.dll)", testStarting.TestCaseUniqueID);
					Assert.Equal("49fb0c20ea941ab02cf91d5efdb3ef118df57da17afceae869238653061b1747", testStarting.TestClassUniqueID);
					Assert.Equal("afe079e5b264c1b49ec973cfeee5c84b8701337135fcd737ea00f96166f3a7aa", testStarting.TestCollectionUniqueID);
					Assert.Equal("type1.passing", testStarting.TestDisplayName);
					Assert.Equal("f7708079f4f28f872d4ab7958e87d8bcf6ab3f33f88217cfd3f4a82222c7b7e1", testStarting.TestMethodUniqueID);
					Assert.Equal("10a8331ca4a3119eff2d3cc4192481750ba67e4ddb3129333f6b34e8bf419a53", testStarting.TestUniqueID);
				},
				message =>
				{
					var testPassed = Assert.IsAssignableFrom<_TestPassed>(message);
					Assert.Equal("fea632f3664b106574ca98fd309f200770f1b0cc4b8f745abfe95a575b84c9a5", testPassed.AssemblyUniqueID);
					Assert.Equal(1M, testPassed.ExecutionTime);
					Assert.Empty(testPassed.Output);
					Assert.Equal("type1.passing (assembly.dll)", testPassed.TestCaseUniqueID);
					Assert.Equal("49fb0c20ea941ab02cf91d5efdb3ef118df57da17afceae869238653061b1747", testPassed.TestClassUniqueID);
					Assert.Equal("afe079e5b264c1b49ec973cfeee5c84b8701337135fcd737ea00f96166f3a7aa", testPassed.TestCollectionUniqueID);
					Assert.Equal("f7708079f4f28f872d4ab7958e87d8bcf6ab3f33f88217cfd3f4a82222c7b7e1", testPassed.TestMethodUniqueID);
					Assert.Equal("10a8331ca4a3119eff2d3cc4192481750ba67e4ddb3129333f6b34e8bf419a53", testPassed.TestUniqueID);
				},
				message =>
				{
					var testFinished = Assert.IsAssignableFrom<_TestFinished>(message);
					Assert.Equal("fea632f3664b106574ca98fd309f200770f1b0cc4b8f745abfe95a575b84c9a5", testFinished.AssemblyUniqueID);
					Assert.Equal(1M, testFinished.ExecutionTime);
					Assert.Empty(testFinished.Output);
					Assert.Equal("type1.passing (assembly.dll)", testFinished.TestCaseUniqueID);
					Assert.Equal("49fb0c20ea941ab02cf91d5efdb3ef118df57da17afceae869238653061b1747", testFinished.TestClassUniqueID);
					Assert.Equal("afe079e5b264c1b49ec973cfeee5c84b8701337135fcd737ea00f96166f3a7aa", testFinished.TestCollectionUniqueID);
					Assert.Equal("f7708079f4f28f872d4ab7958e87d8bcf6ab3f33f88217cfd3f4a82222c7b7e1", testFinished.TestMethodUniqueID);
					Assert.Equal("10a8331ca4a3119eff2d3cc4192481750ba67e4ddb3129333f6b34e8bf419a53", testFinished.TestUniqueID);
				},
				message =>
				{
					var testCaseFinished = Assert.IsAssignableFrom<_TestCaseFinished>(message);
					Assert.Equal("fea632f3664b106574ca98fd309f200770f1b0cc4b8f745abfe95a575b84c9a5", testCaseFinished.AssemblyUniqueID);
					Assert.Equal(1M, testCaseFinished.ExecutionTime);
					Assert.Equal("type1.passing (assembly.dll)", testCaseFinished.TestCaseUniqueID);
					Assert.Equal("49fb0c20ea941ab02cf91d5efdb3ef118df57da17afceae869238653061b1747", testCaseFinished.TestClassUniqueID);
					Assert.Equal("afe079e5b264c1b49ec973cfeee5c84b8701337135fcd737ea00f96166f3a7aa", testCaseFinished.TestCollectionUniqueID);
					Assert.Equal("f7708079f4f28f872d4ab7958e87d8bcf6ab3f33f88217cfd3f4a82222c7b7e1", testCaseFinished.TestMethodUniqueID);
					Assert.Equal(0, testCaseFinished.TestsFailed);
					Assert.Equal(1, testCaseFinished.TestsRun);
					Assert.Equal(0, testCaseFinished.TestsSkipped);
				},
				message =>
				{
					var testMethodFinished = Assert.IsAssignableFrom<_TestMethodFinished>(message);
					Assert.Equal("fea632f3664b106574ca98fd309f200770f1b0cc4b8f745abfe95a575b84c9a5", testMethodFinished.AssemblyUniqueID);
					Assert.Equal(1M, testMethodFinished.ExecutionTime);
					Assert.Equal("49fb0c20ea941ab02cf91d5efdb3ef118df57da17afceae869238653061b1747", testMethodFinished.TestClassUniqueID);
					Assert.Equal("afe079e5b264c1b49ec973cfeee5c84b8701337135fcd737ea00f96166f3a7aa", testMethodFinished.TestCollectionUniqueID);
					Assert.Equal("f7708079f4f28f872d4ab7958e87d8bcf6ab3f33f88217cfd3f4a82222c7b7e1", testMethodFinished.TestMethodUniqueID);
					Assert.Equal(0, testMethodFinished.TestsFailed);
					Assert.Equal(1, testMethodFinished.TestsRun);
					Assert.Equal(0, testMethodFinished.TestsSkipped);
				},
				message =>
				{
					var testMethodStarting = Assert.IsAssignableFrom<_TestMethodStarting>(message);
					Assert.Equal("failing", testMethodStarting.TestMethod);
				},
				message =>
				{
					var testCaseStarting = Assert.IsAssignableFrom<_TestCaseStarting>(message);
					Assert.Equal("type1.failing", testCaseStarting.TestCaseDisplayName);
				},
				message =>
				{
					var testStarting = Assert.IsAssignableFrom<_TestStarting>(message);
					Assert.Equal("type1.failing", testStarting.TestDisplayName);
				},
				message =>
				{
					var testFailed = Assert.IsAssignableFrom<_TestFailed>(message);
					Assert.Equal("fea632f3664b106574ca98fd309f200770f1b0cc4b8f745abfe95a575b84c9a5", testFailed.AssemblyUniqueID);
					Assert.Equal(-1, testFailed.ExceptionParentIndices.Single());
					Assert.Equal("Xunit.MockFailureException", testFailed.ExceptionTypes.Single());
					Assert.Equal(0.234M, testFailed.ExecutionTime);
					Assert.Equal("Failure message", testFailed.Messages.Single());
					Assert.Empty(testFailed.Output);
					Assert.Equal("Stack trace", testFailed.StackTraces.Single());
					Assert.Equal("type1.failing (assembly.dll)", testFailed.TestCaseUniqueID);
					Assert.Equal("49fb0c20ea941ab02cf91d5efdb3ef118df57da17afceae869238653061b1747", testFailed.TestClassUniqueID);
					Assert.Equal("afe079e5b264c1b49ec973cfeee5c84b8701337135fcd737ea00f96166f3a7aa", testFailed.TestCollectionUniqueID);
					Assert.Equal("1714957762efe1f123e780d527b7e319c0624338e1342dc009249aa9074a73be", testFailed.TestMethodUniqueID);
					Assert.Equal("22d7a18fbc1d31a50ff103e3c3f0d6654da1dec22240fc49420449d783f9b7cd", testFailed.TestUniqueID);
				},
				message =>
				{
					var testFinished = Assert.IsAssignableFrom<_TestFinished>(message);
					Assert.Equal(0.234M, testFinished.ExecutionTime);
				},
				message =>
				{
					var testCaseFinished = Assert.IsAssignableFrom<_TestCaseFinished>(message);
					Assert.Equal(0.234M, testCaseFinished.ExecutionTime);
					Assert.Equal(1, testCaseFinished.TestsFailed);
					Assert.Equal(1, testCaseFinished.TestsRun);
					Assert.Equal(0, testCaseFinished.TestsSkipped);
				},
				message =>
				{
					var testMethodFinished = Assert.IsAssignableFrom<_TestMethodFinished>(message);
					Assert.Equal(0.234M, testMethodFinished.ExecutionTime);
					Assert.Equal(1, testMethodFinished.TestsFailed);
					Assert.Equal(1, testMethodFinished.TestsRun);
					Assert.Equal(0, testMethodFinished.TestsSkipped);
				},
				message =>
				{
					var testClassFinished = Assert.IsType<_TestClassFinished>(message);
					Assert.Equal("fea632f3664b106574ca98fd309f200770f1b0cc4b8f745abfe95a575b84c9a5", testClassFinished.AssemblyUniqueID);
					Assert.Equal(1.234M, testClassFinished.ExecutionTime);
					Assert.Equal("49fb0c20ea941ab02cf91d5efdb3ef118df57da17afceae869238653061b1747", testClassFinished.TestClassUniqueID);
					Assert.Equal("afe079e5b264c1b49ec973cfeee5c84b8701337135fcd737ea00f96166f3a7aa", testClassFinished.TestCollectionUniqueID);
					Assert.Equal(1, testClassFinished.TestsFailed);
					Assert.Equal(2, testClassFinished.TestsRun);
					Assert.Equal(0, testClassFinished.TestsSkipped);
				},
				message =>
				{
					var testClassStarting = Assert.IsType<_TestClassStarting>(message);
					Assert.Equal("type2", testClassStarting.TestClass);
				},
				message =>
				{
					var testMethodStarting = Assert.IsAssignableFrom<_TestMethodStarting>(message);
					Assert.Equal("skipping", testMethodStarting.TestMethod);
				},
				message =>
				{
					var testCaseStarting = Assert.IsAssignableFrom<_TestCaseStarting>(message);
					Assert.Equal("type2.skipping", testCaseStarting.TestCaseDisplayName);
				},
				message =>
				{
					var testStarting = Assert.IsAssignableFrom<_TestStarting>(message);
					Assert.Equal("type2.skipping", testStarting.TestDisplayName);
				},
				message =>
				{
					var testSkipped = Assert.IsType<_TestSkipped>(message);
					Assert.Equal("fea632f3664b106574ca98fd309f200770f1b0cc4b8f745abfe95a575b84c9a5", testSkipped.AssemblyUniqueID);
					Assert.Equal(0M, testSkipped.ExecutionTime);
					Assert.Empty(testSkipped.Output);
					Assert.Equal("Skip message", testSkipped.Reason);
					Assert.Equal("type2.skipping (assembly.dll)", testSkipped.TestCaseUniqueID);
					Assert.Equal("be98f543db110a1c396ee6cbba78af448a04d3e4e621bb0d07c940c1bc1020bf", testSkipped.TestClassUniqueID);
					Assert.Equal("afe079e5b264c1b49ec973cfeee5c84b8701337135fcd737ea00f96166f3a7aa", testSkipped.TestCollectionUniqueID);
					Assert.Equal("302172c3ec5473e1923fcc11571c0981166cc5124acb23b43f3d70979a6e7b5d", testSkipped.TestMethodUniqueID);
					Assert.Equal("05154f78fdcfa4d5fe17026690eafe5e753ea05d9a505cb3a4d7fb9682b239b6", testSkipped.TestUniqueID);

				},
				message =>
				{
					var testFinished = Assert.IsAssignableFrom<_TestFinished>(message);
					Assert.Equal(0M, testFinished.ExecutionTime);
				},
				message =>
				{
					var testCaseFinished = Assert.IsAssignableFrom<_TestCaseFinished>(message);
					Assert.Equal(0M, testCaseFinished.ExecutionTime);
					Assert.Equal(0, testCaseFinished.TestsFailed);
					Assert.Equal(1, testCaseFinished.TestsRun);
					Assert.Equal(1, testCaseFinished.TestsSkipped);
				},
				message =>
				{
					var testMethodFinished = Assert.IsAssignableFrom<_TestMethodFinished>(message);
					Assert.Equal(0M, testMethodFinished.ExecutionTime);
					Assert.Equal(0, testMethodFinished.TestsFailed);
					Assert.Equal(1, testMethodFinished.TestsRun);
					Assert.Equal(1, testMethodFinished.TestsSkipped);
				},
				message =>
				{
					var testMethodStarting = Assert.IsAssignableFrom<_TestMethodStarting>(message);
					Assert.Equal("skipping_with_start", testMethodStarting.TestMethod);
				},
				message =>
				{
					var testCaseStarting = Assert.IsAssignableFrom<_TestCaseStarting>(message);
					Assert.Equal("type2.skipping_with_start", testCaseStarting.TestCaseDisplayName);
				},
				message =>
				{
					var testStarting = Assert.IsAssignableFrom<_TestStarting>(message);
					Assert.Equal("type2.skipping_with_start", testStarting.TestDisplayName);
				},
				message =>
				{
					var testSkipped = Assert.IsType<_TestSkipped>(message);
					Assert.Equal(0M, testSkipped.ExecutionTime);
					Assert.Equal("Skip message", testSkipped.Reason);
				},
				message =>
				{
					var testFinished = Assert.IsAssignableFrom<_TestFinished>(message);
					Assert.Equal(0M, testFinished.ExecutionTime);
				},
				message =>
				{
					var testCaseFinished = Assert.IsAssignableFrom<_TestCaseFinished>(message);
					Assert.Equal(0M, testCaseFinished.ExecutionTime);
					Assert.Equal(0, testCaseFinished.TestsFailed);
					Assert.Equal(1, testCaseFinished.TestsRun);
					Assert.Equal(1, testCaseFinished.TestsSkipped);
				},
				message =>
				{
					var testMethodFinished = Assert.IsAssignableFrom<_TestMethodFinished>(message);
					Assert.Equal(0M, testMethodFinished.ExecutionTime);
					Assert.Equal(0, testMethodFinished.TestsFailed);
					Assert.Equal(1, testMethodFinished.TestsRun);
					Assert.Equal(1, testMethodFinished.TestsSkipped);
				},
				message =>
				{
					var testClassFinished = Assert.IsType<_TestClassFinished>(message);
					Assert.Equal(0M, testClassFinished.ExecutionTime);
					Assert.Equal(0, testClassFinished.TestsFailed);
					Assert.Equal(1, testClassFinished.TestsRun);
					Assert.Equal(1, testClassFinished.TestsSkipped);
				},
				message =>
				{
					var testCollectionFinished = Assert.IsType<_TestCollectionFinished>(message);
					Assert.Equal("fea632f3664b106574ca98fd309f200770f1b0cc4b8f745abfe95a575b84c9a5", testCollectionFinished.AssemblyUniqueID);
					Assert.Equal(1.234M, testCollectionFinished.ExecutionTime);
					Assert.Equal("afe079e5b264c1b49ec973cfeee5c84b8701337135fcd737ea00f96166f3a7aa", testCollectionFinished.TestCollectionUniqueID);
					Assert.Equal(1, testCollectionFinished.TestsFailed);
					Assert.Equal(3, testCollectionFinished.TestsRun);
					Assert.Equal(1, testCollectionFinished.TestsSkipped);
				},
				message =>
				{
					var assemblyFinished = Assert.IsType<_TestAssemblyFinished>(message);
					Assert.Equal("fea632f3664b106574ca98fd309f200770f1b0cc4b8f745abfe95a575b84c9a5", assemblyFinished.AssemblyUniqueID);
					Assert.Equal(1.234M, assemblyFinished.ExecutionTime);
					Assert.Equal(1, assemblyFinished.TestsFailed);
					Assert.Equal(3, assemblyFinished.TestsRun);
					Assert.Equal(1, assemblyFinished.TestsSkipped);
				}
			);
		}

		[CulturedFact("en-US")]
		public void ExceptionThrownDuringRunTests_ResultsInErrorMessage()
		{
			var xunit1 = new TestableXunit1("AssemblyName.dll", "ConfigFile.config");
			var testCases = new[] { CreateTestCase("assembly", "config", "type1", "passing", "type1.passing") };
			var exception = new DivideByZeroException();
			xunit1
				.Executor
				.TestFrameworkDisplayName
				.Returns("Test framework display name");
			xunit1
				.Executor
				.WhenForAnyArgs(x => x.RunTests(null!, null!, null!))
				.Do(callInfo => { throw exception; });
			using var sink = SpyMessageSink<_TestAssemblyFinished>.Create();

			xunit1.Run(testCases, sink);
			sink.Finished.WaitOne();

			var errorMessage = Assert.Single(sink.Messages.OfType<_ErrorMessage>());
			Assert.Equal("System.DivideByZeroException", errorMessage.ExceptionTypes.Single());
			Assert.Equal("Attempted to divide by zero.", errorMessage.Messages.Single());
			Assert.Equal(exception.StackTrace, errorMessage.StackTraces.Single());
		}

		[Fact]
		public void NestedExceptionsThrownDuringRunTests_ResultsInErrorMessage()
		{
			var xunit1 = new TestableXunit1("AssemblyName.dll", "ConfigFile.config");
			var testCases = new[] { CreateTestCase("assembly", "config", "type1", "passing", "type1.passing") };
			var exception = GetNestedExceptions();
			xunit1
				.Executor
				.TestFrameworkDisplayName
				.Returns("Test framework display name");
			xunit1
				.Executor
				.WhenForAnyArgs(x => x.RunTests(null!, null!, null!))
				.Do(callInfo => { throw exception; });
			using var sink = SpyMessageSink<_TestAssemblyFinished>.Create();

			xunit1.Run(testCases, sink);
			sink.Finished.WaitOne();

			var errorMessage = Assert.Single(sink.Messages.OfType<_ErrorMessage>());
			Assert.Equal(exception.GetType().FullName, errorMessage.ExceptionTypes[0]);
			Assert.NotNull(exception.InnerException);
			Assert.Equal(exception.InnerException.GetType().FullName, errorMessage.ExceptionTypes[1]);
			Assert.Equal(exception.Message, errorMessage.Messages[0]);
			Assert.Equal(exception.InnerException.Message, errorMessage.Messages[1]);
			Assert.Equal(exception.StackTrace, errorMessage.StackTraces[0]);
			Assert.Equal(exception.InnerException.StackTrace, errorMessage.StackTraces[1]);
		}

		[Fact]
		public void NestedExceptionResultFromTests_ResultsInErrorMessage()
		{
			var xunit1 = new TestableXunit1("AssemblyName.dll", "ConfigFile.config");
			var testCases = new[] { CreateTestCase("assembly", "config", "type1", "failing", "type1.failing") };
			var exception = GetNestedExceptions();
			xunit1
				.Executor
				.TestFrameworkDisplayName
				.Returns("Test framework display name");
			xunit1
				.Executor
				.WhenForAnyArgs(x => x.RunTests(null!, null!, null!))
				.Do(callInfo =>
				{
					var callback = callInfo.Arg<ICallbackEventHandler>();
					callback.RaiseCallbackEvent("<start name='type1.failing' type='type1' method='failing'/>");
					callback.RaiseCallbackEvent($"<test name='type1.failing' type='type1' method='failing' result='Fail' time='0.234'><failure exception-type='{exception.GetType().FullName}'><message>{GetMessage(exception)}</message><stack-trace><![CDATA[{GetStackTrace(exception)}]]></stack-trace></failure></test>");
					callback.RaiseCallbackEvent("<class name='type1' time='1.234' total='1' failed='1' skipped='0'/>");
				});
			using var sink = SpyMessageSink<_TestAssemblyFinished>.Create();

			xunit1.Run(testCases, sink);
			sink.Finished.WaitOne();

			var testFailed = Assert.Single(sink.Messages.OfType<_TestFailed>());
			Assert.Equal(exception.GetType().FullName, testFailed.ExceptionTypes[0]);
			Assert.NotNull(exception.InnerException);
			Assert.Equal(exception.InnerException.GetType().FullName, testFailed.ExceptionTypes[1]);
			Assert.Equal(exception.Message, testFailed.Messages[0]);
			Assert.Equal(exception.InnerException.Message, testFailed.Messages[1]);
			Assert.Equal(exception.StackTrace, testFailed.StackTraces[0]);
			Assert.Equal(exception.InnerException.StackTrace, testFailed.StackTraces[1]);
		}

		[Fact]
		public void ExceptionThrownDuringClassStart_ResultsInErrorMessage()
		{
			var xunit1 = new TestableXunit1("AssemblyName.dll", "ConfigFile.config");
			var testCases = new[] { CreateTestCase("assembly", "config", "type1", "failingclass", "type1.failingclass") };
			var exception = new InvalidOperationException("Cannot use a test class as its own fixture data");
			xunit1
				.Executor
				.TestFrameworkDisplayName
				.Returns("Test framework display name");
			xunit1
				.Executor
				.When(x => x.RunTests("type1", Arg.Any<List<string>>(), Arg.Any<ICallbackEventHandler>()))
				.Do(callInfo =>
				{
					// Ensure the exception has a callstack
					try
					{
						throw exception;
					}
					catch { }
					var callback = callInfo.Arg<ICallbackEventHandler>();
					callback.RaiseCallbackEvent($"<class name='type1' time='0.000' total='0' passed='0' failed='1' skipped='0'><failure exception-type='System.InvalidOperationException'><message>Cannot use a test class as its own fixture data</message><stack-trace><![CDATA[{exception.StackTrace}]]></stack-trace></failure></class>");
				});
			using var sink = SpyMessageSink<_TestAssemblyFinished>.Create();

			xunit1.Run(testCases, sink);
			sink.Finished.WaitOne();

			var errorMessage = Assert.Single(sink.Messages.OfType<_ErrorMessage>());
			Assert.Equal("System.InvalidOperationException", errorMessage.ExceptionTypes.Single());
			Assert.Equal("Cannot use a test class as its own fixture data", errorMessage.Messages.Single());
			Assert.Equal(exception.StackTrace, errorMessage.StackTraces.Single());
		}

		[Fact]
		public void ExceptionThrownDuringClassFinish_ResultsInErrorMessage()
		{
			var xunit1 = new TestableXunit1("AssemblyName.dll", "ConfigFile.config");
			var testCases = new[] { CreateTestCase("assembly", "config", "failingtype", "passingmethod", "failingtype.passingmethod") };
			var exception = new InvalidOperationException("Cannot use a test class as its own fixture data");
			xunit1
				.Executor
				.TestFrameworkDisplayName
				.Returns("Test framework display name");
			xunit1
				.Executor
				.When(x => x.RunTests("failingtype", Arg.Any<List<string>>(), Arg.Any<ICallbackEventHandler>()))
				.Do(callInfo =>
				{
					// Ensure the exception has a callstack
					try
					{
						throw exception;
					}
					catch { }
					var callback = callInfo.Arg<ICallbackEventHandler>();
					callback.RaiseCallbackEvent("<start name='failingtype.passingmethod' type='failingtype' method='passingmethod'/>");
					callback.RaiseCallbackEvent("<test name='failingtype.passingmethod' type='failingtype' method='passingmethod' result='Pass' time='1.000'/>");
					callback.RaiseCallbackEvent($"<class name='failingtype' time='0.000' total='0' passed='1' failed='1' skipped='0'><failure exception-type='Xunit.Some.Exception'><message>Cannot use a test class as its own fixture data</message><stack-trace><![CDATA[{exception.StackTrace}]]></stack-trace></failure></class>");
				});
			using var sink = SpyMessageSink<_TestAssemblyFinished>.Create();

			xunit1.Run(testCases, sink);
			sink.Finished.WaitOne();

			var errorMessage = Assert.Single(sink.Messages.OfType<_ErrorMessage>());
			Assert.Equal("Xunit.Some.Exception", errorMessage.ExceptionTypes.Single());
			Assert.Equal("Cannot use a test class as its own fixture data", errorMessage.Messages.Single());
			Assert.Equal(exception.StackTrace, errorMessage.StackTraces.Single());
		}

		[Fact]
		public void NestedExceptionsThrownDuringClassStart_ResultsInErrorMessage()
		{
			var xunit1 = new TestableXunit1("AssemblyName.dll", "ConfigFile.config");
			var testCases = new[] { CreateTestCase("assembly", "config", "failingtype", "passingmethod", "failingtype.passingmethod") };
			var exception = GetNestedExceptions();
			xunit1
				.Executor
				.TestFrameworkDisplayName
				.Returns("Test framework display name");
			xunit1
				.Executor
				.When(x => x.RunTests("failingtype", Arg.Any<List<string>>(), Arg.Any<ICallbackEventHandler>()))
				.Do(callInfo =>
				{
					var callback = callInfo.Arg<ICallbackEventHandler>();
					callback.RaiseCallbackEvent("<start name='failingtype.passingmethod' type='failingtype' method='passingmethod'/>");
					callback.RaiseCallbackEvent("<test name='failingtype.passingmethod' type='failingtype' method='passingmethod' result='Pass' time='1.000'/>");
					callback.RaiseCallbackEvent($"<class name='failingtype' time='0.000' total='0' passed='1' failed='1' skipped='0'><failure exception-type='System.InvalidOperationException'><message>{GetMessage(exception)}</message><stack-trace><![CDATA[{GetStackTrace(exception)}]]></stack-trace></failure></class>");
				});
			using var sink = SpyMessageSink<_TestAssemblyFinished>.Create();

			xunit1.Run(testCases, sink);
			sink.Finished.WaitOne();

			var errorMessage = Assert.Single(sink.Messages.OfType<_ErrorMessage>());
			Assert.Equal(exception.GetType().FullName, errorMessage.ExceptionTypes[0]);
			Assert.NotNull(exception.InnerException);
			Assert.Equal(exception.InnerException.GetType().FullName, errorMessage.ExceptionTypes[1]);
			Assert.Equal(exception.Message, errorMessage.Messages[0]);
			Assert.Equal(exception.InnerException.Message, errorMessage.Messages[1]);
			Assert.Equal(exception.StackTrace, errorMessage.StackTraces[0]);
			Assert.Equal(exception.InnerException.StackTrace, errorMessage.StackTraces[1]);
		}

		Exception GetNestedExceptions()
		{
			try
			{
				ThrowOuterException();
				throw new InvalidOperationException("Should've thrown an exception");
			}
			catch (Exception e)
			{
				return e;
			}

		}

		void ThrowOuterException()
		{
			try
			{
				ThrowInnerException();
			}
			catch (Exception e)
			{
				throw new InvalidOperationException("Message from outer exception", e);
			}
		}

		void ThrowInnerException()
		{
			throw new DivideByZeroException();
		}

		// From xunit1's ExceptionUtility
		static string GetMessage(Exception ex)
		{
			return GetMessage(ex, 0);
		}

		static string GetMessage(Exception ex, int level)
		{
			var result = "";

			if (level > 0)
			{
				for (var idx = 0; idx < level; idx++)
					result += "----";

				result += " ";
			}

			result += ex.GetType().FullName + " : ";
			result += ex.Message;

			if (ex.InnerException != null)
				result = result + Environment.NewLine + GetMessage(ex.InnerException, level + 1);

			return result;
		}

		const string RETHROW_MARKER = "$$RethrowMarker$$";

		static string GetStackTrace(Exception? ex)
		{
			if (ex == null)
				return "";

			var result = ex.StackTrace ?? "";
			var idx = result.IndexOf(RETHROW_MARKER);
			if (idx >= 0)
				result = result.Substring(0, idx);

			if (ex.InnerException != null)
				result += $"{Environment.NewLine}----- Inner Stack Trace -----{Environment.NewLine}{GetStackTrace(ex.InnerException)}";

			return result;
		}
	}

	public class AcceptanceTests
	{
		[Fact]
		public async void AmbiguouslyNamedTestMethods_StillReturnAllMessages()
		{
			var code = @"
using Xunit;
using Xunit.Extensions;

public class AmbiguouslyNamedTestMethods
{
	[Theory]
	[InlineData(12)]
	public void TestMethod1(int value)
	{
	}

	[Theory]
	[InlineData(""foo"")]
	public void TestMethod1(string value)
	{
	}
}";

			using var assembly = await CSharpAcceptanceTestV1Assembly.Create(code);
			var xunit1 = new Xunit1(new _NullMessageSink(), AppDomainSupport.Required, _NullSourceInformationProvider.Instance, assembly.FileName);
			using var spy = SpyMessageSink<_TestAssemblyFinished>.Create();
			xunit1.Run(spy);
			spy.Finished.WaitOne();

			Assert.Collection(
				spy.Messages,
				msg => Assert.IsType<_TestAssemblyStarting>(msg),
				msg => Assert.IsType<_TestCollectionStarting>(msg),
				msg => Assert.IsType<_TestClassStarting>(msg),
				msg => Assert.IsType<_TestClassFinished>(msg),
				msg => Assert.IsType<_TestCollectionFinished>(msg),
				msg => Assert.IsType<_ErrorMessage>(msg),
				msg => Assert.IsType<_TestAssemblyFinished>(msg)
			);
		}
	}

	static Xunit1TestCase CreateTestCase(
		string assemblyPath,
		string configFileName,
		string typeName,
		string methodName,
		string testCaseDisplayName)
	{
		var assembly = new Xunit1TestAssembly(assemblyPath, configFileName);
		var collection = new Xunit1TestCollection(assembly);
		var @class = new Xunit1TestClass(collection, typeName);
		var method = new Xunit1TestMethod(@class, methodName);
		return new Xunit1TestCase(method, testCaseDisplayName);
	}

	class TestableXunit1 : Xunit1
	{
		public readonly IXunit1Executor Executor = Substitute.For<IXunit1Executor>();
		public string Executor_TestAssemblyFileName;
		public string? Executor_ConfigFileName;
		public bool Executor_ShadowCopy;
		public string? Executor_ShadowCopyFolder;
		public readonly _ISourceInformationProvider SourceInformationProvider;

		public TestableXunit1(
			string? assemblyFileName = null,
			string? configFileName = null,
			bool shadowCopy = true,
			string? shadowCopyFolder = null,
			AppDomainSupport appDomainSupport = AppDomainSupport.Required)
				: this(appDomainSupport, assemblyFileName ?? OsSpecificAssemblyPath, configFileName, shadowCopy, shadowCopyFolder, Substitute.For<_ISourceInformationProvider>())
		{ }

		TestableXunit1(
			AppDomainSupport appDomainSupport,
			string assemblyFileName,
			string? configFileName,
			bool shadowCopy,
			string? shadowCopyFolder,
			_ISourceInformationProvider sourceInformationProvider)
				: base(new _NullMessageSink(), appDomainSupport, sourceInformationProvider, assemblyFileName, configFileName, shadowCopy, shadowCopyFolder)
		{
			Executor_TestAssemblyFileName = assemblyFileName;
			Executor_ConfigFileName = configFileName;
			Executor_ShadowCopy = shadowCopy;
			Executor_ShadowCopyFolder = shadowCopyFolder;

			SourceInformationProvider = sourceInformationProvider;
		}

		protected override IXunit1Executor CreateExecutor() => Executor;
	}

	class TestableTestDiscoverySink : TestDiscoverySink
	{
		public List<_TestCaseDiscovered> DiscoveredTestCases = new List<_TestCaseDiscovered>();
		public bool StartSeen = false;

		public TestableTestDiscoverySink(Func<bool>? cancelThunk = null)
			: base(cancelThunk)
		{
			DiscoverySink.DiscoveryStartingEvent += args => StartSeen = true;
			DiscoverySink.TestCaseDiscoveredEvent += args => DiscoveredTestCases.Add(args.Message);
		}
	}
}

#endif
