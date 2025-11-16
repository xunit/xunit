using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Discoverer class for <see cref="CulturedTheoryAttribute"/>.
/// </summary>
public class CulturedTheoryAttributeDiscoverer : TheoryDiscoverer
{
	/// <remarks>
	/// For each culture, returns one instance of <see cref="CulturedXunitTestCase"/>. May also return a
	/// single <see cref="ExecutionErrorTestCase"/> for error conditions.
	/// </remarks>
	/// <inheritdoc/>
	protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForDataRow(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		ITheoryAttribute theoryAttribute,
		ITheoryDataRow dataRow,
		object?[] testMethodArguments)
	{
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(theoryAttribute);
		Guard.ArgumentNotNull(dataRow);
		Guard.ArgumentNotNull(testMethodArguments);

		var details = TestIntrospectionHelper.GetTestCaseDetailsForTheoryDataRow(discoveryOptions, testMethod, theoryAttribute, dataRow, testMethodArguments);

		if (theoryAttribute is not CulturedTheoryAttribute culturedTheoryAttribute)
			return Error(
				details,
				"{0} was decorated on [{1}] which is not compatible with {2}",
				typeof(CulturedTheoryAttributeDiscoverer).SafeName(),
				theoryAttribute.GetType().SafeName(),
				typeof(CulturedTheoryAttribute).SafeName()
			);

		var cultures = culturedTheoryAttribute.Cultures;
		if (cultures is null || cultures.Length == 0)
			return Error(
				details,
				"{0} did not provide any cultures",
				theoryAttribute.GetType().SafeName()
			);

		var traits = TestIntrospectionHelper.GetTraits(testMethod, dataRow);

		return new(
			cultures
				.Select(
					culture => new CulturedXunitTestCase(
						culture,
						details.ResolvedTestMethod,
						details.TestCaseDisplayName,
						details.UniqueID,
						details.Explicit,
						dataRow.Label,
						details.SkipExceptions,
						details.SkipReason,
						details.SkipType,
						details.SkipUnless,
						details.SkipWhen,
						traits,
						testMethodArguments,
						timeout: details.Timeout
					)
				)
			.CastOrToReadOnlyCollection()
		);
	}

	/// <remarks>
	/// For each culture, returns one instance of <see cref="CulturedXunitDelayEnumeratedTheoryTestCase"/>
	/// (which performs the data discovery at runtime, for non-skipped theories) or <see cref="CulturedXunitTestCase"/>
	/// (for skipped theories). May also return a single <see cref="ExecutionErrorTestCase"/> for error conditions.
	/// </remarks>
	/// <inheritdoc/>
	protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForTheory(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		ITheoryAttribute theoryAttribute)
	{
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(theoryAttribute);

		var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, theoryAttribute);

		if (theoryAttribute is not CulturedTheoryAttribute culturedTheoryAttribute)
			return Error(
				details,
				"{0} was decorated on [{1}] which is not compatible with {2}",
				typeof(CulturedTheoryAttributeDiscoverer).SafeName(),
				theoryAttribute.GetType().SafeName(),
				typeof(CulturedTheoryAttribute).SafeName()
			);

		var cultures = culturedTheoryAttribute.Cultures;
		if (cultures is null || cultures.Length == 0)
			return Error(
				details,
				"{0} did not provide any cultures",
				theoryAttribute.GetType().SafeName()
			);

		return new(
			cultures
				.Select(
					culture =>
						details.SkipReason is not null && details.SkipUnless is null && details.SkipWhen is null
							? new CulturedXunitTestCase(
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
							: (IXunitTestCase)new CulturedXunitDelayEnumeratedTheoryTestCase(
								culture,
								details.ResolvedTestMethod,
								details.TestCaseDisplayName,
								details.UniqueID,
								details.Explicit,
								theoryAttribute.SkipTestWithoutData,
								details.SkipExceptions,
								details.SkipReason,
								details.SkipType,
								details.SkipUnless,
								details.SkipWhen,
								testMethod.Traits.ToReadWrite(StringComparer.OrdinalIgnoreCase),
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
