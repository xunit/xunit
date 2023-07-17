using System.Collections.Generic;
using Xunit;

namespace System.Diagnostics.CodeAnalysis;

public class Issue1985Tests
{

	public static IEnumerable<object[]> MyTestCases(string name, int maxSzenarios = 444)
	{
		for (int i = 1; i <= maxSzenarios; i++)
		{
			yield return new object[] { name, i };
		}
	}

	public static IEnumerable<object[]> MyTestCases(string name, string maxSzenarios = "444")
	{
		yield return new object[] { name + name, int.Parse(maxSzenarios) };
	}

	// Uncomment the method below for the test to pass
	
	// public static IEnumerable<object[]> MyTestCases(string name)
	// {
	// 	yield return new object[] { name + name, 2 };
	// }
	
	[Theory]
	[MemberData(nameof(MyTestCases), "MyFirst")]
	[MemberData(nameof(MyTestCases), "MySecond")]
	public void MyTestMethod(string name, int szenario)
	{
		Assert.True(name.Length > 0);
		Assert.True(szenario > 0);
	}
}
