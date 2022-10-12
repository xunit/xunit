using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

public class DynamicAssemblyAcceptanceTests : IClassFixture<DynamicAssemblyFixture>
{
	readonly DynamicAssemblyFixture fixture;

	public DynamicAssemblyAcceptanceTests(DynamicAssemblyFixture fixture)
	{
		this.fixture = fixture;
	}

	[Fact]
	public async ValueTask CanDiscoverTests()
	{
		Assert.SkipWhen(EnvironmentHelper.IsMono, "Mono does not fully support dynamic assemblies");

		var assemblyInfo = new ReflectionAssemblyInfo(fixture.Assembly);

		await using var disposalTracker = new DisposalTracker();
		var testFramework = ExtensibilityPointFactory.GetTestFramework(assemblyInfo);
		disposalTracker.Add(testFramework);

		var testCases = new List<_ITestCase>();
		var testDiscoverer = testFramework.GetDiscoverer(assemblyInfo);
		await testDiscoverer.Find(testCase => { testCases.Add(testCase); return new(true); }, _TestFrameworkOptions.ForDiscovery());

		Assert.Collection(
			testCases.OrderBy(tc => tc.TestCaseDisplayName),
			testCase => Assert.Equal("UnitTests.Failing", testCase.TestCaseDisplayName),
			testCase => Assert.Equal("UnitTests.Passing", testCase.TestCaseDisplayName)
		);
	}

	[Fact]
	public async ValueTask CanRunTests()
	{
		Assert.SkipWhen(EnvironmentHelper.IsMono, "Mono does not fully support dynamic assemblies");

		var assemblyInfo = new ReflectionAssemblyInfo(fixture.Assembly);

		await using var disposalTracker = new DisposalTracker();
		var testFramework = ExtensibilityPointFactory.GetTestFramework(assemblyInfo);
		disposalTracker.Add(testFramework);

		var messages = new List<_MessageSinkMessage>();
		var testDiscoverer = testFramework.GetDiscoverer(assemblyInfo);
		var testCases = new List<_ITestCase>();
		await testDiscoverer.Find(testCase => { testCases.Add(testCase); return new(true); }, _TestFrameworkOptions.ForDiscovery());
		var testExecutor = testFramework.GetExecutor(assemblyInfo);
		await testExecutor.RunTestCases(testCases, SpyMessageSink.Create(messages: messages), _TestFrameworkOptions.ForExecution());

		var assemblyStarting = Assert.Single(messages.OfType<_TestAssemblyStarting>());
		Assert.Equal("DynamicAssembly, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", assemblyStarting.AssemblyName);
		Assert.Null(assemblyStarting.AssemblyPath);
		Assert.Null(assemblyStarting.ConfigFilePath);

		Assert.Single(messages.OfType<_TestFailed>());
		Assert.Single(messages.OfType<_TestPassed>());
		Assert.Empty(messages.OfType<_TestSkipped>());
	}
}

public class DynamicAssemblyFixture
{
	public DynamicAssemblyFixture()
	{
		var assertTrue = typeof(Assert).GetMethod(nameof(Assert.True), new[] { typeof(bool) });
		Assert.NotNull(assertTrue);

		var factAttributeCtor = typeof(FactAttribute).GetConstructor(Array.Empty<Type>());
		Assert.NotNull(factAttributeCtor);

		var assemblyName = new AssemblyName("DynamicAssembly");
		var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
		var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicAssembly");
		var typeBuilder = moduleBuilder.DefineType("UnitTests", TypeAttributes.Public);

		var passingTestBuilder = typeBuilder.DefineMethod("Passing", MethodAttributes.Public);
		passingTestBuilder.SetCustomAttribute(new CustomAttributeBuilder(factAttributeCtor, Array.Empty<object>()));
		var passingILGenerator = passingTestBuilder.GetILGenerator();
		passingILGenerator.Emit(OpCodes.Ldc_I4_1);
		passingILGenerator.Emit(OpCodes.Call, assertTrue);
		passingILGenerator.Emit(OpCodes.Ret);

		var failingTestBuilder = typeBuilder.DefineMethod("Failing", MethodAttributes.Public);
		failingTestBuilder.SetCustomAttribute(new CustomAttributeBuilder(factAttributeCtor, Array.Empty<object>()));
		var failingILGenerator = failingTestBuilder.GetILGenerator();
		failingILGenerator.Emit(OpCodes.Ldc_I4_0);
		failingILGenerator.Emit(OpCodes.Call, assertTrue);
		failingILGenerator.Emit(OpCodes.Ret);

		typeBuilder.CreateType();

		Assembly = assemblyBuilder;
	}

	public Assembly Assembly { get; }
}
