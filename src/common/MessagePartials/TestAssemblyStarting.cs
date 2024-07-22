using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestAssemblyStarting"/>.
/// </summary>
[JsonTypeID("test-assembly-starting")]
sealed partial class TestAssemblyStarting : TestAssemblyMessage, ITestAssemblyStarting
{
	string IAssemblyMetadata.UniqueID =>
		AssemblyUniqueID;

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(AssemblyName), AssemblyName);
		serializer.Serialize(nameof(AssemblyPath), AssemblyPath);
		serializer.Serialize(nameof(ConfigFilePath), ConfigFilePath);
		serializer.Serialize(nameof(Seed), Seed);
		serializer.Serialize(nameof(StartTime), StartTime);
		serializer.Serialize(nameof(TargetFramework), TargetFramework);
		serializer.Serialize(nameof(TestEnvironment), TestEnvironment);
		serializer.Serialize(nameof(TestFrameworkDisplayName), TestFrameworkDisplayName);
		serializer.SerializeTraits(nameof(Traits), Traits);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(
			CultureInfo.CurrentCulture,
			"{0} name={1} path={2} config={3}{4}",
			base.ToString(),
			AssemblyName.Quoted(),
			AssemblyPath.Quoted(),
			ConfigFilePath.Quoted(),
			Seed is null ? "" : string.Format(CultureInfo.CurrentCulture, " seed={0}", Seed)
		);
}
