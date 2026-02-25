using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Discoverer class for <see cref="CulturedFactAttribute"/>.
/// </summary>
public class CulturedFactAttributeDiscoverer : IXunitTestCaseDiscoverer
{
	/// <inheritdoc/>
	public ValueTask<IReadOnlyCollection<IXunitTestCase>> Discover(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		IFactAttribute factAttribute)
	{
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(factAttribute);

		var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, factAttribute);

		if (factAttribute is not CulturedFactAttribute culturedFactAttribute)
			return Error(
				details,
				"{0} was decorated on [{1}] which is not compatible with {2}",
				typeof(CulturedFactAttributeDiscoverer).SafeName(),
				factAttribute.GetType().SafeName(),
				typeof(CulturedFactAttribute).SafeName()
			);

		var cultures = culturedFactAttribute.Cultures;
		if (cultures is null || cultures.Length == 0)
			return Error(
				details,
				"{0} did not provide any cultures",
				factAttribute.GetType().SafeName()
			);

		return new(
			cultures
				.Select(
					culture => new CulturedXunitTestCase(
						culture,
						details.ResolvedTestMethod,
						details.TestCaseDisplayName,
						details.UniqueID,
						details.Explicit,
						details.SkipExceptions,
						details.SkipReason,
						details.SkipType,
						details.SkipUnless,
						details.SkipWhen,
						testMethod.Traits.ToReadWrite(StringComparer.OrdinalIgnoreCase),
						sourceFilePath: details.SourceFilePath,
						sourceLineNumber: details.SourceLineNumber,
						timeout: details.Timeout
					)
				)
				.CastOrToReadOnlyCollection()
		);
	}

	static ValueTask<IReadOnlyCollection<IXunitTestCase>> Error(
		(string TestCaseDisplayName, bool, Type[]?, string?, Type?, string?, string?, string? SourceFilePath, int? SourceLineNumber, int, string UniqueID, IXunitTestMethod ResolvedTestMethod) details,
		string format,
		params object?[] args) =>
			new([
#pragma warning disable CA2000 // The disposable object is returned via the ValueTask
				new ExecutionErrorTestCase(
					details.ResolvedTestMethod,
					details.TestCaseDisplayName,
					details.UniqueID,
					details.SourceFilePath,
					details.SourceLineNumber,
					string.Format(CultureInfo.CurrentCulture, format, args)
				)
#pragma warning restore CA2000
			]);
}
