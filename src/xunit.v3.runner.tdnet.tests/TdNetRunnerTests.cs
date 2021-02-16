using System;
using System.Collections.Generic;
using System.Reflection;
using NSubstitute;
using TestDriven.Framework;
using Xunit;
using Xunit.Runner.TdNet;
using Xunit.v3;

public class TdNetRunnerTests
{
	private static readonly Assembly thisAssembly = typeof(TdNetRunnerTests).Assembly;

	public class RunMember
	{
		class TypeUnderTest
		{
			public event Action? Event;
#pragma warning disable CS0649
			public int Field;
#pragma warning restore CS0649
			public int Property { get; set; }
			public void Method() { }
		}

		[Fact]
		public void WithType()
		{
			var listener = Substitute.For<ITestListener>();
			var runner = new TestableTdNetRunner();

			runner.RunMember(listener, thisAssembly, typeof(TypeUnderTest));

			Assert.Collection(
				runner.Operations,
				msg => Assert.Equal("RunClass(type: TdNetRunnerTests+RunMember+TypeUnderTest, initialRunState: NoTests)", msg)
			);
		}

		[Fact]
		public void WithMethod()
		{
			var listener = Substitute.For<ITestListener>();
			var runner = new TestableTdNetRunner();

			runner.RunMember(listener, thisAssembly, typeof(TypeUnderTest).GetMethod(nameof(TypeUnderTest.Method))!);

			Assert.Collection(
				runner.Operations,
				msg => Assert.Equal("RunMethod(method: TdNetRunnerTests+RunMember+TypeUnderTest.Method, initialRunState: NoTests)", msg)
			);
		}

		[Fact]
		public void WithUnsupportedMemberTypes()
		{
			var listener = Substitute.For<ITestListener>();
			var runner = new TestableTdNetRunner();

			runner.RunMember(listener, thisAssembly, typeof(TypeUnderTest).GetProperty(nameof(TypeUnderTest.Property))!);
			runner.RunMember(listener, thisAssembly, typeof(TypeUnderTest).GetField(nameof(TypeUnderTest.Field))!);
			runner.RunMember(listener, thisAssembly, typeof(TypeUnderTest).GetEvent(nameof(TypeUnderTest.Event))!);

			Assert.Empty(runner.Operations);
		}
	}

	public class RunNamespace
	{
		[Fact]
		public void RunsOnlyTestMethodsInTheGivenNamespace()
		{
			var listener = Substitute.For<ITestListener>();
			var runner = new TestableTdNetRunner();
			var testCaseInNamespace = TestData.TestCaseDiscovered<DummyNamespace.ClassInNamespace>("TestMethod");
			var testCaseOutsideOfNamespace = TestData.TestCaseDiscovered<RunNamespace>("RunsOnlyTestMethodsInTheGivenNamespace");
			runner.TestsToDiscover.Clear();
			runner.TestsToDiscover.Add(testCaseInNamespace);
			runner.TestsToDiscover.Add(testCaseOutsideOfNamespace);

			runner.RunNamespace(listener, typeof(DummyNamespace.ClassInNamespace).Assembly, "DummyNamespace");

			Assert.Collection(
				runner.Operations,
				msg => Assert.Equal("Discovery()", msg),
				msg => Assert.Equal("Run(initialRunState: NoTests)", msg)
			);
			Assert.Collection(
				runner.TestsRun,
				testCase => Assert.Same(testCaseInNamespace, testCase)
			);
		}
	}

	class TestableTdNetRunner : TdNetRunner
	{
		public List<string> Operations = new List<string>();
		public List<_TestCaseDiscovered> TestsRun = new();
		public List<_TestCaseDiscovered> TestsToDiscover = new() { Substitute.For<_TestCaseDiscovered>() };

		public override TdNetRunnerHelper CreateHelper(ITestListener testListener, Assembly assembly)
		{
			var helper = Substitute.For<TdNetRunnerHelper>();

			helper
				.Discover()
				.Returns(callInfo =>
				{
					Operations.Add("Discovery()");
					return TestsToDiscover;
				});

			helper
				.Run(null, TestRunState.NoTests)
				.ReturnsForAnyArgs(callInfo =>
				{
					Operations.Add($"Run(initialRunState: {callInfo[1]})");
					TestsRun.AddRange((IEnumerable<_TestCaseDiscovered>)callInfo[0]);
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

			return helper;
		}
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
