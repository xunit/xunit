using Xunit;
using Xunit.v3;

public class ExtensibilityPointFactoryTests
{
	public class BeforeAfterAttributeOrdering
	{
		[Fact]
		public void TestCollectionComesAfterTestAssembly()
		{
			var assemblyAttributes = new[] { new AssemblyBeforeAfter() };

			var result = ExtensibilityPointFactory.GetCollectionBeforeAfterTestAttributes(typeof(MyCollection), assemblyAttributes);

			Assert.Collection(
				result,
				attr => Assert.IsType<AssemblyBeforeAfter>(attr),
				attr => Assert.IsType<CollectionBeforeAfter>(attr)
			);
		}

		[Fact]
		public void TestClassComesAfterTestCollection()
		{
			var collectionAttributes = new[] { new CollectionBeforeAfter() };

			var result = ExtensibilityPointFactory.GetClassBeforeAfterTestAttributes(typeof(MyTestClass), collectionAttributes);

			Assert.Collection(
				result,
				attr => Assert.IsType<CollectionBeforeAfter>(attr),
				attr => Assert.IsType<ClassBeforeAfter>(attr)
			);
		}

		[Fact]
		public void TestMethodComesAfterTestClass()
		{
			var classAttributes = new[] { new ClassBeforeAfter() };
			var methodInfo = typeof(MyTestClass).GetMethod(nameof(MyTestClass.TestMethod));
			Assert.NotNull(methodInfo);

			var result = ExtensibilityPointFactory.GetMethodBeforeAfterTestAttributes(methodInfo, classAttributes);

			Assert.Collection(
				result,
				attr => Assert.IsType<ClassBeforeAfter>(attr),
				attr => Assert.IsType<MethodBeforeAfter>(attr)
			);
		}

		[CollectionBeforeAfter]
		class MyCollection { }

		[ClassBeforeAfter]
		class MyTestClass
		{
			[MethodBeforeAfter]
			public void TestMethod() { }
		}

		class AssemblyBeforeAfter : BeforeAfterTestAttribute { }
		class CollectionBeforeAfter : BeforeAfterTestAttribute { }
		class ClassBeforeAfter : BeforeAfterTestAttribute { }
		class MethodBeforeAfter : BeforeAfterTestAttribute { }
	}
}
