#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Internal;
using Xunit.Runner.v1;
using Xunit.v3;

public class Xunit1TestCaseTests
{
	public class UniqueID
	{
		[Fact]
		public static void UniqueIDIsStable()
		{
			var typeUnderTest = typeof(ClassUnderTest);
			var assemblyFileName = typeUnderTest.Assembly.GetLocalCodeBase();

			var result = Create(typeUnderTest, "TestMethod");

			Assert.Equal($"Xunit1TestCaseTests+UniqueID+ClassUnderTest.TestMethod ({assemblyFileName})", ((_ITestCase)result).UniqueID);
		}

		class ClassUnderTest
		{
			[Fact]
			public static void TestMethod()
			{ }
		}
	}

	static Xunit1TestCase Create(
		Type typeUnderTest,
		string methodName,
		Dictionary<string, List<string>>? traits = null)
	{
		var assemblyFileName = typeUnderTest.Assembly.GetLocalCodeBase();
		var assembly = new Xunit1TestAssembly(assemblyFileName);
		var collection = new Xunit1TestCollection(assembly);
		var @class = new Xunit1TestClass(collection, typeUnderTest.FullName!);

		return new Xunit1TestCase(@class, methodName, null, traits);
	}
}

#endif
