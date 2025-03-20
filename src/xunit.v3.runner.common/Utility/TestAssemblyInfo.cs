using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// This is the class that wraps the data expected to be returned when invoking a v3 test project with
/// the <c>-assemblyInfo</c> switch.
/// </summary>
public class TestAssemblyInfo
{
	TestAssemblyInfo(
		string archOS,
		string archProcess,
		Version coreFramework,
		string coreFrameworkInformational,
		int pointerSize,
		string runtimeFramework,
		string targetFramework,
		string testFramework)
	{
		ArchOS = Guard.ArgumentNotNull(archOS);
		ArchProcess = Guard.ArgumentNotNull(archProcess);
		CoreFramework = Guard.ArgumentNotNull(coreFramework);
		CoreFrameworkInformational = Guard.ArgumentNotNull(coreFrameworkInformational);
		PointerSize = pointerSize;
		RuntimeFramework = Guard.ArgumentNotNull(runtimeFramework);
		TargetFramework = Guard.ArgumentNotNull(targetFramework);
		TestFramework = Guard.ArgumentNotNull(testFramework);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TestAssemblyInfo"/> class.
	/// </summary>
	/// <param name="coreFramework">The version of <c>xunit.v3.core</c></param>
	/// <param name="coreFrameworkInformational">The informational verison of <c>xunit.v3.core</c></param>
	/// <param name="targetFramework">The target framework the test assembly was built against</param>
	/// <param name="testFramework">The display name of the test framework</param>
	public TestAssemblyInfo(
		Version coreFramework,
		string coreFrameworkInformational,
		string targetFramework,
		string testFramework) :
			this(
				RuntimeInformation.OSArchitecture.ToString(),
				RuntimeInformation.ProcessArchitecture.ToString(),
				coreFramework,
				coreFrameworkInformational,
				IntPtr.Size * 8,
				RuntimeInformation.FrameworkDescription,
				targetFramework,
				testFramework
			)
	{ }

	/// <summary>
	/// Gets the value returned by <see cref="RuntimeInformation.OSArchitecture"/>.
	/// </summary>
	/// <remarks>
	/// This is returned as a string value rather than the enum value, given the variance of available values spanning
	/// across operating systems and frameworks.
	/// </remarks>
	public string ArchOS { get; }

	/// <summary>
	/// Gets the value returned by <see cref="RuntimeInformation.ProcessArchitecture"/>.
	/// </summary>
	/// <remarks>
	/// This is returned as a string value rather than the enum value, given the variance of available values spanning
	/// across operating systems and frameworks.
	/// </remarks>
	public string ArchProcess { get; }

	/// <summary>
	/// Gets the assembly version of <c>xunit.v3.core.dll</c>.
	/// </summary>
	public Version CoreFramework { get; }

	/// <summary>
	/// Gets the informational assembly version of <c>xunit.v3.core.dll</c>.
	/// </summary>
	public string CoreFrameworkInformational { get; }

	/// <summary>
	/// Gets the bit-size of pointers in the test process (i.e., <c><see cref="IntPtr.Size"/> * 8</c>).
	/// </summary>
	public int PointerSize { get; }

	/// <summary>
	/// Gets the value returned by <see cref="RuntimeInformation.FrameworkDescription"/>.
	/// </summary>
	public string RuntimeFramework { get; }

	/// <summary>
	/// Gets the target framework the test process was built for, as embedded into the assembly-level
	/// attribute <see cref="TargetFrameworkAttribute.FrameworkName"/>. In the rare condition that
	/// this attribute is missing, this will return <c>UnknownTargetFramework</c>.
	/// </summary>
	public string TargetFramework { get; }

	/// <summary>
	/// Gets the value returned by <see cref="P:ITestFramework.TestFrameworkDisplayName"/>. The default
	/// test framework will return a value like <c>"xUnit.net v3 &lt;core-framework-informational&gt;"</c>.
	/// </summary>
	public string TestFramework { get; }

	/// <summary>
	/// Rehydrates an instance of <see cref="TestAssemblyInfo"/> from the given JSON.
	/// </summary>
	/// <param name="serialization">The JSON serialization</param>
	public static TestAssemblyInfo FromJson(string serialization)
	{
		if (!JsonDeserializer.TryDeserialize(serialization, out var json) || json is not IReadOnlyDictionary<string, object?> root)
			throw new ArgumentException("non-object");

		var archOS = JsonDeserializer.TryGetString(root, "arch-os") ?? throw new ArgumentException("'arch-os' is missing or malformed");
		var archProcess = JsonDeserializer.TryGetString(root, "arch-process") ?? throw new ArgumentException("'arch-process' is missing or malformed");
		var coreFramework = JsonDeserializer.TryGetVersion(root, "core-framework") ?? throw new ArgumentException("'core-framework' is missing or malformed");
		var coreFrameworkInformational = JsonDeserializer.TryGetString(root, "core-framework-informational") ?? throw new ArgumentException("'core-framework-informational' is missing or malformed");
		var pointerSize = JsonDeserializer.TryGetInt(root, "pointer-size") ?? throw new ArgumentException("'pointer-size' is missing or malformed");
		var runtimeFramework = JsonDeserializer.TryGetString(root, "runtime-framework") ?? throw new ArgumentException("'runtime-framework' is missing or malformed");
		var targetFramework = JsonDeserializer.TryGetString(root, "target-framework") ?? throw new ArgumentException("'target-framework' is missing or malformed");
		var testFramework = JsonDeserializer.TryGetString(root, "test-framework") ?? throw new ArgumentException("'test-framework' is missing or malformed");

		return new(archOS, archProcess, coreFramework, coreFrameworkInformational, pointerSize, runtimeFramework, targetFramework, testFramework);
	}

	/// <summary>
	/// Gets this object in JSON format, which can be rehydrated with <see cref="FromJson"/>.
	/// </summary>
	public string ToJson()
	{
		var buffer = new StringBuilder();
		using (var serializer = new JsonObjectSerializer(buffer))
		{
			serializer.Serialize("arch-os", ArchOS);
			serializer.Serialize("arch-process", ArchProcess);
			serializer.Serialize("core-framework", CoreFramework);
			serializer.Serialize("core-framework-informational", CoreFrameworkInformational);
			serializer.Serialize("pointer-size", PointerSize);
			serializer.Serialize("runtime-framework", RuntimeFramework);
			serializer.Serialize("target-framework", TargetFramework);
			serializer.Serialize("test-framework", TestFramework);
		}

		return buffer.ToString();
	}
}
