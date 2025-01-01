using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Implementation of <see cref="IXunitTestCaseDiscoverer"/> that supports finding test cases
/// on methods decorated with <see cref="ITheoryAttribute"/>.
/// </summary>
public class TheoryDiscoverer : IXunitTestCaseDiscoverer
{
	/// <summary>
	/// Creates test cases for a single row of data. By default, returns a single instance of <see cref="XunitTestCase"/>
	/// with the data row inside of it.
	/// </summary>
	/// <param name="discoveryOptions">The discovery options to be used.</param>
	/// <param name="testMethod">The test method the test cases belong to.</param>
	/// <param name="theoryAttribute">The theory attribute attached to the test method.</param>
	/// <param name="dataRow">The data row that generated <paramref name="testMethodArguments"/>.</param>
	/// <param name="testMethodArguments">The arguments for the test method.</param>
	/// <returns>The test cases</returns>
	protected virtual ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForDataRow(
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
		var traits = TestIntrospectionHelper.GetTraits(testMethod, dataRow);

		var testCase = new XunitTestCase(
			details.ResolvedTestMethod,
			details.TestCaseDisplayName,
			details.UniqueID,
			details.Explicit,
			details.SkipExceptions,
			details.SkipReason,
			details.SkipType,
			details.SkipUnless,
			details.SkipWhen,
			traits,
			testMethodArguments,
			timeout: details.Timeout
		);

#pragma warning disable IDE0300 // Changes the semantics
		return new(new[] { testCase });
#pragma warning restore IDE0300
	}

	/// <summary>
	/// Creates test cases for the entire theory. This is used when one or more of the theory data items
	/// are not serializable, or if the user has requested to skip theory pre-enumeration, or if the user
	/// has requested the entire theory be skipped. By default, returns a single instance
	/// of <see cref="XunitDelayEnumeratedTheoryTestCase"/> (which performs the
	/// data discovery at runtime, for non-skipped theories) or <see cref="XunitTestCase"/>
	/// (for skipped theories).
	/// </summary>
	/// <param name="discoveryOptions">The discovery options to be used.</param>
	/// <param name="testMethod">The test method the test cases belong to.</param>
	/// <param name="theoryAttribute">The theory attribute attached to the test method.</param>
	/// <returns>The test case</returns>
	protected virtual ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForTheory(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		ITheoryAttribute theoryAttribute)
	{
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(theoryAttribute);

		var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, theoryAttribute);
		var testCase =
			details.SkipReason is not null && details.SkipUnless is null && details.SkipWhen is null
				? new XunitTestCase(
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
					timeout: details.Timeout
				)
				: (IXunitTestCase)new XunitDelayEnumeratedTheoryTestCase(
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
				);

#pragma warning disable IDE0300 // Changes the semantics
		return new(new[] { testCase });
#pragma warning restore IDE0300
	}

	/// <summary>
	/// Discover test cases from a test method.
	/// </summary>
	/// <remarks>
	/// This method performs the following steps:<br/>
	/// - If the theory attribute is marked with Skip, or pre-enumeration is off, or any of the test data is non serializable,
	///   returns the result of <see cref="CreateTestCasesForTheory"/>;<br/>
	/// - If there is no theory data, returns a single test case of <see cref="ExecutionErrorTestCase"/> with the error in it;<br/>
	/// - Otherwise, it returns one test case per data row, created by calling <see cref="CreateTestCasesForDataRow"/>.
	/// </remarks>
	/// <param name="discoveryOptions">The discovery options to be used.</param>
	/// <param name="testMethod">The test method the test cases belong to.</param>
	/// <param name="factAttribute">The theory attribute attached to the test method.</param>
	/// <returns>Returns zero or more test cases represented by the test method.</returns>
	public virtual async ValueTask<IReadOnlyCollection<IXunitTestCase>> Discover(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		IFactAttribute factAttribute)
	{
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(factAttribute);

		if (factAttribute is not ITheoryAttribute theoryAttribute)
			throw new ArgumentException("TheoryDiscoverer.Discover must be passed an attribute that implements ITheoryAttribute", nameof(factAttribute));

		// Special case unconditional skip, because we want a single Skip (not one per data item); plus, a skipped theory may
		// not actually have any data (which is quasi-legal, since it's skipped).
		if (theoryAttribute.Skip is not null && theoryAttribute.SkipUnless is null && theoryAttribute.SkipWhen is null)
			return await CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute);

		var preEnumerate =
			discoveryOptions.PreEnumerateTheoriesOrDefault()
			&& !theoryAttribute.DisableDiscoveryEnumeration;

		if (preEnumerate)
		{
			DisposalTracker disposalTracker = new();
			try
			{
				var results = new List<IXunitTestCase>();

				foreach (var dataAttribute in testMethod.DataAttributes)
				{
					if (!dataAttribute.SupportsDiscoveryEnumeration())
						return await CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute);

					var data = await dataAttribute.GetData(testMethod.Method, disposalTracker);
					if (disposalTracker.TrackedObjects.Count > 0)
						return await CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute);

					if (data is null)
					{
						var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, theoryAttribute);

						results.Add(
							new ExecutionErrorTestCase(
								details.ResolvedTestMethod,
								details.TestCaseDisplayName,
								details.UniqueID,
								string.Format(
									CultureInfo.CurrentCulture,
									"Test data returned null for {0}.{1}. Make sure it is statically initialized before this test method is called.",
									testMethod.TestClass.TestClassName,
									testMethod.MethodName
								)
							)
						);

						continue;
					}

					foreach (var dataRow in data)
					{
						// Determine whether we can serialize the test case, since we need a way to uniquely
						// identify a test and serialization is the best way to do that. If it's not serializable,
						// this will throw and we will fall back to a single theory test case that gets its data at runtime.
						// Also, if we can, we should attempt to resolve it to its parameter type right now, because
						// the incoming data might be serializable but the actual parameter value that it gets converted
						// to might not be, and serialization uses the resolved argument and not the input argument.
						var resolvedData = testMethod.ResolveMethodArguments(dataRow.GetData());

						var nonSerializableTypes = new List<Type>();
						foreach (var argument in resolvedData.WhereNotNull())
						{
							var argumentType = argument.GetType();
							if (!SerializationHelper.Instance.IsSerializable(argument, argumentType))
								nonSerializableTypes.Add(argumentType);
						}

						if (nonSerializableTypes.Count != 0)
						{
							TestContext.Current.SendDiagnosticMessage(
								"Non-serializable data (of type{0} {1}) found for '{2}.{3}'; falling back to single test case.",
								nonSerializableTypes.Count == 1 ? string.Empty : "s",
								nonSerializableTypes.ToCommaSeparatedList(),
								testMethod.TestClass.TestClassName,
								testMethod.MethodName
							);

							return await CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute);
						}

						try
						{
							results.AddRange(await CreateTestCasesForDataRow(discoveryOptions, testMethod, theoryAttribute, dataRow, resolvedData));
						}
						catch (Exception ex)
						{
							TestContext.Current.SendDiagnosticMessage(
								"Error creating theory test case for for '{0}.{1}'; falling back to single test case. Exception message: '{2}'",
								testMethod.TestClass.TestClassName,
								testMethod.MethodName,
								ex.Message
							);

							return await CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute);
						}
					}
				}

				if (results.Count == 0)
				{
					var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, theoryAttribute);
					var message = string.Format(CultureInfo.CurrentCulture, "No data found for {0}.{1}", testMethod.TestClass.TestClassName, testMethod.MethodName);

					if (theoryAttribute.SkipTestWithoutData)
						results.Add(new XunitTestCase(details.ResolvedTestMethod, details.TestCaseDisplayName, details.UniqueID, details.Explicit, skipReason: message));
					else
						results.Add(new ExecutionErrorTestCase(details.ResolvedTestMethod, details.TestCaseDisplayName, details.UniqueID, errorMessage: message));
				}

				return results;
			}
			catch (Exception ex)    // If something goes wrong, fall through to return just the XunitTestCase
			{
				TestContext.Current.SendDiagnosticMessage(
					"Exception thrown during theory discovery on '{0}.{1}'; falling back to single test case.{2}{3}",
					testMethod.TestClass.TestClassName,
					testMethod.MethodName,
					Environment.NewLine,
					ex
				);
			}
			finally
			{
				await disposalTracker.DisposeAsync();
			}
		}

		return await CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute);
	}
}
