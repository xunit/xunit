using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using NSubstitute;
using TestDriven.Framework;
using Xunit;
using Xunit.Runner.TdNet;

public class TdNetRunnerTests
{
	private static readonly Assembly thisAssembly = typeof(TdNetRunnerTests).Assembly;

	[Fact]
	public void RunAssembly()
	{
		var listener = Substitute.For<ITestListener>();
		var runner = new TestableTdNetRunner();

		runner.RunAssembly(listener, thisAssembly);

		Assert.Collection(
			runner.Operations,
			msg => Assert.Equal("RunAll(initialRunState: NoTests)", msg),
			msg => Assert.Equal("DisposeAsync()", msg)
		);
	}

	[Fact]
	public void RunMember_WithType()
	{
		var listener = Substitute.For<ITestListener>();
		var runner = new TestableTdNetRunner();

		runner.RunMember(listener, thisAssembly, typeof(RunMember_TypeUnderTest));

		Assert.Collection(
			runner.Operations,
			msg => Assert.Equal("RunClass(type: TdNetRunnerTests+RunMember_TypeUnderTest, initialRunState: NoTests)", msg),
			msg => Assert.Equal("DisposeAsync()", msg)
		);
	}

	[Fact]
	public void RunMember_WithMethod()
	{
		var listener = Substitute.For<ITestListener>();
		var runner = new TestableTdNetRunner();

		runner.RunMember(listener, thisAssembly, typeof(RunMember_TypeUnderTest).GetMethod(nameof(RunMember_TypeUnderTest.Method))!);

		Assert.Collection(
			runner.Operations,
			msg => Assert.Equal("RunMethod(method: TdNetRunnerTests+RunMember_TypeUnderTest.Method, initialRunState: NoTests)", msg),
			msg => Assert.Equal("DisposeAsync()", msg)
		);
	}

	public static TheoryData<MemberInfo> UnsupportedMemberInfoData
	{
		get
		{
			return new TheoryData<MemberInfo>(
				typeof(RunMember_TypeUnderTest).GetProperty(nameof(RunMember_TypeUnderTest.Property))!,
				typeof(RunMember_TypeUnderTest).GetField(nameof(RunMember_TypeUnderTest.Field))!,
				typeof(RunMember_TypeUnderTest).GetEvent(nameof(RunMember_TypeUnderTest.Event))!
			);
		}
	}

	[Theory]
	[MemberData(nameof(UnsupportedMemberInfoData), DisableDiscoveryEnumeration = true)]
	public void RunMember_WithUnsupportedMemberInfo(MemberInfo member)
	{
		var listener = Substitute.For<ITestListener>();
		var runner = new TestableTdNetRunner();

		var result = runner.RunMember(listener, thisAssembly, member);

		Assert.Equal(TestRunState.NoTests, result);
		var msg = Assert.Single(runner.Operations);
		Assert.Equal("DisposeAsync()", msg);
	}

	class RunMember_TypeUnderTest
	{
		public event Action? Event;
#pragma warning disable CS0649
		public int Field;
#pragma warning restore CS0649
		public int Property { get; set; }
		public void Method() { }
	}

	[Fact]
	public void RunNamespace()
	{
		var listener = Substitute.For<ITestListener>();
		var runner = new TestableTdNetRunner();

		runner.RunNamespace(listener, thisAssembly, "MyNamespace");

		Assert.Collection(
			runner.Operations,
			msg => Assert.Equal("RunNamespace(namespace: MyNamespace, initialRunState: NoTests)", msg),
			msg => Assert.Equal("DisposeAsync()", msg)
		);
	}

	class TestableTdNetRunner : TdNetRunner
	{
		public List<string> Operations = new();

		public override TdNetRunnerHelper CreateHelper(ITestListener testListener, Assembly assembly)
		{
			var helper = Substitute.For<TdNetRunnerHelper>();

			helper
				.DisposeAsync()
				.ReturnsForAnyArgs(callInfo =>
				{
					Operations.Add($"DisposeAsync()");
					return default;
				});

			helper
				.RunAll(TestRunState.NoTests)
				.ReturnsForAnyArgs(callInfo =>
				{
					Operations.Add($"RunAll(initialRunState: {callInfo[0]})");
					return TestRunState.NoTests;
				});

			helper
				.RunClass(null!, TestRunState.NoTests)
				.ReturnsForAnyArgs(callInfo =>
				{
					Operations.Add($"RunClass(type: {callInfo[0]}, initialRunState: {callInfo[1]})");
					return TestRunState.NoTests;
				});

			helper
				.RunMethod(null!, TestRunState.NoTests)
				.ReturnsForAnyArgs(callInfo =>
				{
					var method = (MethodInfo)callInfo[0];
					Operations.Add($"RunMethod(method: {method.DeclaringType!.FullName}.{method.Name}, initialRunState: {callInfo[1]})");
					return TestRunState.NoTests;
				});

			helper
				.RunNamespace(null!, TestRunState.NoTests)
				.ReturnsForAnyArgs(callInfo =>
				{
					Operations.Add($"RunNamespace(namespace: {callInfo[0]}, initialRunState: {callInfo[1]})");
					return TestRunState.NoTests;
				});

			return helper;
		}

		// Run this synchronously for testing purposes
		protected override void ExecuteOnBackgroundThread(Func<ValueTask> action) =>
			Task.Run(action).GetAwaiter().GetResult();
	}
}

namespace DummyNamespace
{
	public class ClassInNamespace
	{
		public void TestMethod()
		{ }
	}
}
