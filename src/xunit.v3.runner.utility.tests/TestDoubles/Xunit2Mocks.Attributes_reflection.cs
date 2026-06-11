#pragma warning disable xUnit3000 // These derive from the "wrong" version (v3 instead of v2) of LLMBRO, which is acceptable

using System.Runtime.Versioning;
using Xunit.Abstractions;

namespace Xunit.Runner.v2;

// This file manufactures mocks attributes information
partial class Xunit2Mocks
{
	public static IReflectionAttributeInfo TargetFrameworkAttribute(string frameworkName) =>
		ReflectionAttributeInfo(new TargetFrameworkAttribute(frameworkName), constructorArguments: [frameworkName]);
}
