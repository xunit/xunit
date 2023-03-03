using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.v3;

public class CollectionAcceptanceTests : AcceptanceTestV3
{
	[Fact]
	public async ValueTask TwoClasses_OneInExplicitCollection_OneInDefaultCollection()
	{
		var results = await RunAsync(new[] { typeof(ClassInExplicitCollection), typeof(ClassInDefaultCollection) });

		var defaultCollectionStarting = Assert.Single(results.OfType<_TestCollectionStarting>().Where(x => x.TestCollectionDisplayName.StartsWith("Test collection for ")));
		var defaultResults = results.OfType<_TestCollectionMessage>().Where(x => x.TestCollectionUniqueID == defaultCollectionStarting.TestCollectionUniqueID);
		AssertMessageSequence(defaultResults, "CollectionAcceptanceTests+ClassInDefaultCollection.Passing");

		var explicitCollectionStarting = Assert.Single(results.OfType<_TestCollectionStarting>().Where(x => x.TestCollectionDisplayName == "Explicit Collection"));
		var explicitResults = results.OfType<_TestCollectionMessage>().Where(x => x.TestCollectionUniqueID == explicitCollectionStarting.TestCollectionUniqueID);
		AssertMessageSequence(explicitResults, "CollectionAcceptanceTests+ClassInExplicitCollection.Passing");
	}

	private void AssertMessageSequence(IEnumerable<_MessageSinkMessage> results, string testDisplayName)
	{
		Assert.Collection(
			results,
			message => Assert.IsType<_TestCollectionStarting>(message),
			message => Assert.IsType<_TestClassStarting>(message),
			message => Assert.IsType<_TestMethodStarting>(message),
			message => Assert.IsType<_TestCaseStarting>(message),
			message =>
			{
				var testStarting = Assert.IsType<_TestStarting>(message);
				Assert.Equal(testDisplayName, testStarting.TestDisplayName);
			},
			message => Assert.IsType<_TestClassConstructionStarting>(message),
			message => Assert.IsType<_TestClassConstructionFinished>(message),
			message => Assert.IsType<_BeforeTestStarting>(message),
			message => Assert.IsType<_BeforeTestFinished>(message),
			message => Assert.IsType<_AfterTestStarting>(message),
			message => Assert.IsType<_AfterTestFinished>(message),
			message => Assert.IsType<_TestPassed>(message),
			message => Assert.IsType<_TestFinished>(message),
			message => Assert.IsType<_TestCaseFinished>(message),
			message => Assert.IsType<_TestMethodFinished>(message),
			message => Assert.IsType<_TestClassFinished>(message),
			message => Assert.IsType<_TestCollectionFinished>(message)
		);
	}

	[Collection("Explicit Collection")]
	class ClassInExplicitCollection
	{
		[Fact]
		public void Passing() { }
	}

	class ClassInDefaultCollection
	{
		[Fact]
		public void Passing() { }
	}

	[Fact]
	public async void TestCasesAreOrderedOnACollectionLevel()
	{
		var results = await RunForResultsAsync(new[] { typeof(PriorityOrderExamples.ClassA), typeof(PriorityOrderExamples.ClassB) });

		Assert.All(results, result => Assert.IsType<TestPassedWithDisplayName>(result));
	}

	[CollectionDefinition(nameof(PriorityOrderCollection))]
	[TestCaseOrderer("TestOrderExamples.TestCaseOrdering.PriorityOrderer", "TestOrderExamples")]
	public class PriorityOrderCollection
	{
		// We're also stashing the statics here so there's a convenient place for them to be accessed by all
		// the test cases. This is just for this example; normally test collection classes are empty because
		// they're not instantiated or used anywhere else, just as a place to hold metadata attributes.
		public static bool Test1Called;
		public static bool Test2ACalled;
		public static bool Test2BCalled;
		public static bool Test3Called;
	}

	class PriorityOrderExamples
	{
		[Collection(nameof(PriorityOrderCollection))]
		internal class ClassA
		{
			[Fact, TestPriority(5)]
			public void Test3()
			{
				PriorityOrderCollection.Test3Called = true;

				Assert.True(PriorityOrderCollection.Test1Called);
				Assert.True(PriorityOrderCollection.Test2ACalled);
				Assert.True(PriorityOrderCollection.Test2BCalled);
			}

			[Fact, TestPriority(0)]
			public void Test2B()
			{
				PriorityOrderCollection.Test2BCalled = true;

				Assert.True(PriorityOrderCollection.Test1Called);
				Assert.True(PriorityOrderCollection.Test2ACalled);
				Assert.False(PriorityOrderCollection.Test3Called);
			}
		}

		[Collection(nameof(PriorityOrderCollection))]
		internal class ClassB
		{
			[Fact]
			public void Test2A()
			{
				PriorityOrderCollection.Test2ACalled = true;

				Assert.True(PriorityOrderCollection.Test1Called);
				Assert.False(PriorityOrderCollection.Test2BCalled);
				Assert.False(PriorityOrderCollection.Test3Called);
			}

			[Fact, TestPriority(-5)]
			public void Test1()
			{
				PriorityOrderCollection.Test1Called = true;

				Assert.False(PriorityOrderCollection.Test2ACalled);
				Assert.False(PriorityOrderCollection.Test2BCalled);
				Assert.False(PriorityOrderCollection.Test3Called);
			}
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class TestPriorityAttribute : Attribute
	{
		public TestPriorityAttribute(int priority)
		{
			Priority = priority;
		}

		public int Priority { get; private set; }
	}

	public class PriorityOrderer : ITestCaseOrderer
	{
		public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
			where TTestCase : notnull, _ITestCase
		{
			var sortedMethods = new SortedDictionary<int, List<TTestCase>>();

			foreach (TTestCase testCase in testCases)
			{
				int priority = 0;

				foreach (IAttributeInfo attr in testCase.TestMethod!.Method.GetCustomAttributes(typeof(TestPriorityAttribute).AssemblyQualifiedName!))
					priority = attr.GetNamedArgument<int>("Priority");

				GetOrCreate(sortedMethods, priority).Add(testCase);
			}

			var result = new List<TTestCase>();

			foreach (var list in sortedMethods.Keys.Select(priority => sortedMethods[priority]))
			{
				list.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.TestMethod?.Method.Name, y.TestMethod?.Method.Name));
				foreach (TTestCase testCase in list)
					result.Add(testCase);
			}

			return result;
		}

		static TValue GetOrCreate<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
		{
			if (dictionary.TryGetValue(key, out var result))
				return result;

			result = new TValue();
			dictionary[key] = result;

			return result;
		}
	}
}
