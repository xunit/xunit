using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestAssemblyRunnerContextTests
{
	public class TestFrameworkDisplayName
	{
		[Fact]
		public static async ValueTask IsXunit()
		{
			await using var context = TestableXunitTestAssemblyRunnerContext.Create();
			await context.InitializeAsync();

			var result = context.TestFrameworkDisplayName;

			Assert.StartsWith("xUnit.net v3 ", result);
		}
	}

	public class TestFrameworkEnvironment
	{
		[Fact]
		public static async ValueTask Default()
		{
			await using var context = TestableXunitTestAssemblyRunnerContext.Create();
			await context.InitializeAsync();

			var result = context.TestFrameworkEnvironment;

			Assert.EndsWith($"[collection-per-class, parallel ({Environment.ProcessorCount} threads)]", result);
		}

		[Fact]
		public static async ValueTask Attribute_NonParallel()
		{
			var attribute = Mocks.CollectionBehaviorAttribute(disableTestParallelization: true);
			var assembly = Mocks.TestAssembly("assembly.dll", assemblyAttributes: new[] { attribute });
			await using var context = TestableXunitTestAssemblyRunnerContext.Create(assembly: assembly);
			await context.InitializeAsync();

			var result = context.TestFrameworkEnvironment;

			Assert.EndsWith("[collection-per-class, non-parallel]", result);
		}

		[Theory]
		[InlineData(1, null, "1 thread")]
		[InlineData(3, ParallelAlgorithm.Conservative, "3 threads")]
		[InlineData(42, ParallelAlgorithm.Aggressive, "42 threads/aggressive")]
		public static async ValueTask Attribute_MaxThreads(
			int maxThreads,
			ParallelAlgorithm? parallelAlgorithm,
			string expected)
		{
			var attribute = Mocks.CollectionBehaviorAttribute(maxParallelThreads: maxThreads);
			var assembly = Mocks.TestAssembly("assembly.dll", assemblyAttributes: new[] { attribute });
			var options = TestData.TestFrameworkExecutionOptions(parallelAlgorithm: parallelAlgorithm);
			await using var context = TestableXunitTestAssemblyRunnerContext.Create(assembly: assembly, executionOptions: options);
			await context.InitializeAsync();

			var result = context.TestFrameworkEnvironment;

			Assert.EndsWith($"[collection-per-class, parallel ({expected})]", result);
		}

		[Fact]
		public static async ValueTask Attribute_Unlimited()
		{
			var attribute = Mocks.CollectionBehaviorAttribute(maxParallelThreads: -1);
			var assembly = Mocks.TestAssembly("assembly.dll", assemblyAttributes: new[] { attribute });
			var options = TestData.TestFrameworkExecutionOptions(parallelAlgorithm: ParallelAlgorithm.Aggressive);  // Shouldn't show for unlimited threads
			await using var context = TestableXunitTestAssemblyRunnerContext.Create(assembly: assembly, executionOptions: options);
			await context.InitializeAsync();

			var result = context.TestFrameworkEnvironment;

			Assert.EndsWith("[collection-per-class, parallel (unlimited threads)]", result);
		}

		[Theory]
		[InlineData(CollectionBehavior.CollectionPerAssembly, "collection-per-assembly")]
		[InlineData(CollectionBehavior.CollectionPerClass, "collection-per-class")]
		public static async ValueTask Attribute_CollectionBehavior(CollectionBehavior behavior, string expectedDisplayText)
		{
			var attribute = Mocks.CollectionBehaviorAttribute(behavior, disableTestParallelization: true);
			var assembly = Mocks.TestAssembly("assembly.dll", assemblyAttributes: new[] { attribute });
			await using var context = TestableXunitTestAssemblyRunnerContext.Create(assembly: assembly);
			await context.InitializeAsync();

			var result = context.TestFrameworkEnvironment;

			Assert.EndsWith($"[{expectedDisplayText}, non-parallel]", result);
		}

		[Fact]
		public static async ValueTask Attribute_CustomCollectionFactory()
		{
			var factoryType = typeof(MyTestCollectionFactory);
			var attr = Mocks.CollectionBehaviorAttribute(factoryType.FullName!, factoryType.Assembly.FullName!, disableTestParallelization: true);
			var assembly = Mocks.TestAssembly("assembly.dll", assemblyAttributes: new[] { attr });
			await using var context = TestableXunitTestAssemblyRunnerContext.Create(assembly: assembly);
			await context.InitializeAsync();

			var result = context.TestFrameworkEnvironment;

			Assert.EndsWith("[My Factory, non-parallel]", result);
		}

		class MyTestCollectionFactory : IXunitTestCollectionFactory
		{
			public string DisplayName { get { return "My Factory"; } }

			public MyTestCollectionFactory(_ITestAssembly assembly) { }

			public _ITestCollection Get(_ITypeInfo testClass)
			{
				throw new NotImplementedException();
			}
		}

		[Fact]
		public static async ValueTask TestOptions_NonParallel()
		{
			var options = TestData.TestFrameworkExecutionOptions();
			options.SetDisableParallelization(true);
			await using var context = TestableXunitTestAssemblyRunnerContext.Create(executionOptions: options);
			await context.InitializeAsync();

			var result = context.TestFrameworkEnvironment;

			Assert.EndsWith("[collection-per-class, non-parallel]", result);
		}

		[Fact]
		public static async ValueTask TestOptions_MaxThreads()
		{
			var options = TestData.TestFrameworkExecutionOptions();
			options.SetMaxParallelThreads(3);
			await using var context = TestableXunitTestAssemblyRunnerContext.Create(executionOptions: options);
			await context.InitializeAsync();

			var result = context.TestFrameworkEnvironment;

			Assert.EndsWith("[collection-per-class, parallel (3 threads)]", result);
		}

		[Fact]
		public static async ValueTask TestOptions_Unlimited()
		{
			var options = TestData.TestFrameworkExecutionOptions();
			options.SetMaxParallelThreads(-1);
			await using var context = TestableXunitTestAssemblyRunnerContext.Create(executionOptions: options);
			await context.InitializeAsync();

			var result = context.TestFrameworkEnvironment;

			Assert.EndsWith("[collection-per-class, parallel (unlimited threads)]", result);
		}

		[Fact]
		public static async ValueTask TestOptionsOverrideAttribute()
		{
			var attribute = Mocks.CollectionBehaviorAttribute(disableTestParallelization: true, maxParallelThreads: 127);
			var options = TestData.TestFrameworkExecutionOptions();
			options.SetDisableParallelization(false);
			options.SetMaxParallelThreads(3);
			var assembly = Mocks.TestAssembly("assembly.dll", assemblyAttributes: new[] { attribute });
			await using var context = TestableXunitTestAssemblyRunnerContext.Create(assembly: assembly, executionOptions: options);
			await context.InitializeAsync();

			var result = context.TestFrameworkEnvironment;

			Assert.EndsWith("[collection-per-class, parallel (3 threads)]", result);
		}
	}

	public class AssemblyTestCaseOrderer
	{
		[Fact]
		public static async ValueTask CanSetTestCaseOrdererInAssemblyAttribute()
		{
			var ordererAttribute = Mocks.TestCaseOrdererAttribute<MyTestCaseOrderer>();
			var assembly = Mocks.TestAssembly("assembly.dll", assemblyAttributes: new[] { ordererAttribute });
			await using var context = TestableXunitTestAssemblyRunnerContext.Create(assembly: assembly);
			await context.InitializeAsync();

			var result = context.AssemblyTestCaseOrderer;

			Assert.IsType<MyTestCaseOrderer>(result);
		}

		class MyTestCaseOrderer : ITestCaseOrderer
		{
			public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
				where TTestCase : notnull, _ITestCase
			{
				throw new NotImplementedException();
			}
		}

		[Fact]
		public static async ValueTask UnknownType_HaltsProcessing()
		{
			var ordererAttribute = Mocks.TestCaseOrdererAttribute("UnknownType", "UnknownAssembly");
			var assembly = Mocks.TestAssembly("assembly.dll", assemblyAttributes: [ordererAttribute]);
			var executionSink = SpyMessageSink.Capture();
			await using var context = TestableXunitTestAssemblyRunnerContext.Create(assembly: assembly, executionMessageSink: executionSink);

			await context.InitializeAsync();

			var errorMessage = Assert.Single(executionSink.Messages.OfType<_ErrorMessage>());
			var type = Assert.Single(errorMessage.ExceptionTypes);
			Assert.Equal(typeof(XunitException).FullName, type);
			var index = Assert.Single(errorMessage.ExceptionParentIndices);
			Assert.Equal(-1, index);
			var msg = Assert.Single(errorMessage.Messages);
			Assert.Equal("Could not find type 'UnknownType' in 'UnknownAssembly' for assembly-level test case orderer", msg);
			Assert.Empty(context.TestCases);
		}

		[Fact]
		public static async ValueTask ThrowsDuringConstruction_HaltsProcessing()
		{
			var ordererAttribute = Mocks.TestCaseOrdererAttribute<MyCtorThrowingTestCaseOrderer>();
			var assembly = Mocks.TestAssembly("assembly.dll", assemblyAttributes: [ordererAttribute]);
			var executionSink = SpyMessageSink.Capture();
			await using var context = TestableXunitTestAssemblyRunnerContext.Create(assembly: assembly, executionMessageSink: executionSink);

			await context.InitializeAsync();

			var errorMessage = Assert.Single(executionSink.Messages.OfType<_ErrorMessage>());
			var type = Assert.Single(errorMessage.ExceptionTypes);
			Assert.Equal(typeof(XunitException).FullName, type);
			var index = Assert.Single(errorMessage.ExceptionParentIndices);
			Assert.Equal(-1, index);
			var msg = Assert.Single(errorMessage.Messages);
			Assert.Equal("Assembly-level test case orderer 'XunitTestAssemblyRunnerContextTests+AssemblyTestCaseOrderer+MyCtorThrowingTestCaseOrderer' threw 'System.DivideByZeroException' during construction: Attempted to divide by zero.", msg);
			Assert.Empty(context.TestCases);
		}

		class MyCtorThrowingTestCaseOrderer : ITestCaseOrderer
		{
			public MyCtorThrowingTestCaseOrderer()
			{
				throw new DivideByZeroException();
			}

			public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
				where TTestCase : notnull, _ITestCase
			{
				return Array.Empty<TTestCase>();
			}
		}
	}

	public class AssemblyTestCollectionOrderer
	{
		[Fact]
		public static async ValueTask CanSetTestCollectionOrdererInAssemblyAttribute()
		{
			var ordererAttribute = Mocks.TestCollectionOrdererAttribute<DescendingDisplayNameCollectionOrderer>();
			var assembly = Mocks.TestAssembly("assembly.dll", assemblyAttributes: new[] { ordererAttribute });
			await using var context = TestableXunitTestAssemblyRunnerContext.Create(assembly: assembly);
			await context.InitializeAsync();

			var result = context.AssemblyTestCollectionOrderer;

			Assert.IsType<DescendingDisplayNameCollectionOrderer>(result);
		}

		class DescendingDisplayNameCollectionOrderer : ITestCollectionOrderer
		{
			public IReadOnlyCollection<_ITestCollection> OrderTestCollections(IReadOnlyCollection<_ITestCollection> TestCollections) =>
				TestCollections
					.OrderByDescending(c => c.DisplayName)
					.CastOrToReadOnlyCollection();
		}

		[Fact]
		public static async ValueTask UnknownType_HaltsProcessing()
		{
			var ordererAttribute = Mocks.TestCollectionOrdererAttribute("UnknownType", "UnknownAssembly");
			var assembly = Mocks.TestAssembly("assembly.dll", assemblyAttributes: [ordererAttribute]);
			var executionSink = SpyMessageSink.Capture();
			await using var context = TestableXunitTestAssemblyRunnerContext.Create(assembly: assembly, executionMessageSink: executionSink);

			await context.InitializeAsync();

			var errorMessage = Assert.Single(executionSink.Messages.OfType<_ErrorMessage>());
			var type = Assert.Single(errorMessage.ExceptionTypes);
			Assert.Equal(typeof(XunitException).FullName, type);
			var index = Assert.Single(errorMessage.ExceptionParentIndices);
			Assert.Equal(-1, index);
			var msg = Assert.Single(errorMessage.Messages);
			Assert.Equal("Could not find type 'UnknownType' in 'UnknownAssembly' for assembly-level test collection orderer", msg);
			Assert.Empty(context.TestCases);
		}

		[Fact]
		public static async ValueTask ThrowsDuringConstruction_HaltsProcessing()
		{
			var ordererAttribute = Mocks.TestCollectionOrdererAttribute<CtorThrowingCollectionOrderer>();
			var assembly = Mocks.TestAssembly("assembly.dll", assemblyAttributes: [ordererAttribute]);
			var executionSink = SpyMessageSink.Capture();
			await using var context = TestableXunitTestAssemblyRunnerContext.Create(assembly: assembly, executionMessageSink: executionSink);

			await context.InitializeAsync();

			var errorMessage = Assert.Single(executionSink.Messages.OfType<_ErrorMessage>());
			var type = Assert.Single(errorMessage.ExceptionTypes);
			Assert.Equal(typeof(XunitException).FullName, type);
			var index = Assert.Single(errorMessage.ExceptionParentIndices);
			Assert.Equal(-1, index);
			var msg = Assert.Single(errorMessage.Messages);
			Assert.Equal("Assembly-level test collection orderer 'XunitTestAssemblyRunnerContextTests+AssemblyTestCollectionOrderer+CtorThrowingCollectionOrderer' threw 'System.DivideByZeroException' during construction: Attempted to divide by zero.", msg);
			Assert.Empty(context.TestCases);
		}

		class CtorThrowingCollectionOrderer : ITestCollectionOrderer
		{
			public CtorThrowingCollectionOrderer()
			{
				throw new DivideByZeroException();
			}

			public IReadOnlyCollection<_ITestCollection> OrderTestCollections(IReadOnlyCollection<_ITestCollection> testCollections) =>
				Array.Empty<_ITestCollection>();
		}
	}

	class ClassUnderTest
	{
		[Fact]
		public void Passing() { Thread.Sleep(0); }

		[Fact]
		public void Other() { Thread.Sleep(0); }
	}

	class TestableXunitTestAssemblyRunnerContext : XunitTestAssemblyRunnerContext
	{
		TestableXunitTestAssemblyRunnerContext(
			_ITestAssembly testAssembly,
			IReadOnlyCollection<IXunitTestCase> testCases,
			_IMessageSink executionMessageSink,
			_ITestFrameworkExecutionOptions executionOptions) :
				base(testAssembly, testCases, executionMessageSink, executionOptions)
		{ }

		public static TestableXunitTestAssemblyRunnerContext Create(
			_ITestAssembly? assembly = null,
			_IMessageSink? executionMessageSink = null,
			_ITestFrameworkExecutionOptions? executionOptions = null) =>
				new(
					assembly ?? Mocks.TestAssembly(),
					[TestData.XunitTestCase<ClassUnderTest>("Passing")],
					executionMessageSink ?? SpyMessageSink.Create(),
					executionOptions ?? TestData.TestFrameworkExecutionOptions()
				);
	}
}
