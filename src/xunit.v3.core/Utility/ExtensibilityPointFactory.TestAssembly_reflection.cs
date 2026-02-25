using System.Reflection;
using Xunit.Sdk;

namespace Xunit.v3;

// Extensibility point factories related to test assemblies (non-AOT)

partial class ExtensibilityPointFactory
{
	/// <summary>
	/// Gets the <see cref="IBeforeAfterTestAttribute"/>s attached to the given test assembly.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	public static IReadOnlyCollection<IBeforeAfterTestAttribute> GetAssemblyBeforeAfterTestAttributes(Assembly testAssembly)
	{
		var warnings = new List<string>();

		try
		{
			return Guard.ArgumentNotNull(testAssembly).GetMatchingCustomAttributes<IBeforeAfterTestAttribute>(warnings);
		}
		finally
		{
			foreach (var warning in warnings)
				TestContext.Current.SendDiagnosticMessage(warning);
		}
	}

	/// <summary>
	/// Gets the fixture types that are attached to the test assembly via <see cref="IAssemblyFixtureAttribute"/>s.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	public static IReadOnlyCollection<Type> GetAssemblyFixtureTypes(Assembly testAssembly)
	{
		var warnings = new List<string>();

		try
		{
			return
				Guard.ArgumentNotNull(testAssembly)
					.GetMatchingCustomAttributes<IAssemblyFixtureAttribute>(warnings)
					.Select(a => a.AssemblyFixtureType)
					.CastOrToReadOnlyCollection();
		}
		finally
		{
			foreach (var warning in warnings)
				TestContext.Current.SendDiagnosticMessage(warning);
		}
	}

	/// <summary>
	/// Gets the traits that are attached to the test assembly via <see cref="ITraitAttribute"/>s.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> GetAssemblyTraits(Assembly testAssembly)
	{
		Guard.ArgumentNotNull(testAssembly);

		var warnings = new List<string>();

		try
		{
			var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

			foreach (var traitAttribute in testAssembly.GetMatchingCustomAttributes<ITraitAttribute>(warnings))
				foreach (var kvp in traitAttribute.GetTraits())
					result.AddOrGet(kvp.Key).Add(kvp.Value);

			return result.ToReadOnly();
		}
		finally
		{
			foreach (var warning in warnings)
				TestContext.Current.SendDiagnosticMessage(warning);
		}
	}
}
