using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk;

/// <summary>
/// Implementation of <see cref="IXunitTestCaseDiscoverer"/> that supports finding test cases
/// on methods decorated with <see cref="TheoryAttribute"/>.
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
		_ITestFrameworkDiscoveryOptions discoveryOptions,
		_ITestMethod testMethod,
		_IAttributeInfo theoryAttribute,
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

		// TODO: How do we get source information in here?
		var testCase = new XunitTestCase(
			details.ResolvedTestMethod,
			details.TestCaseDisplayName,
			details.UniqueID,
			details.Explicit,
			details.SkipReason,
			traits,
			testMethodArguments,
			timeout: details.Timeout
		);

		return new(new[] { testCase });
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
		_ITestFrameworkDiscoveryOptions discoveryOptions,
		_ITestMethod testMethod,
		_IAttributeInfo theoryAttribute)
	{
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(theoryAttribute);

		var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, theoryAttribute);
		var traits = TestIntrospectionHelper.GetTraits(testMethod);

		IXunitTestCase testCase;

		// TODO: How do we get source information in here?
		if (details.SkipReason is not null)
			testCase = new XunitTestCase(
				details.ResolvedTestMethod,
				details.TestCaseDisplayName,
				details.UniqueID,
				details.Explicit,
				details.SkipReason,
				traits,
				timeout: details.Timeout
			);
		else
			testCase = new XunitDelayEnumeratedTheoryTestCase(
				details.ResolvedTestMethod,
				details.TestCaseDisplayName,
				details.UniqueID,
				details.Explicit,
				traits,
				timeout: details.Timeout
			);

		return new(new[] { testCase });
	}

	/// <summary>
	/// Discover test cases from a test method.
	/// </summary>
	/// <remarks>
	/// This method performs the following steps:<br/>
	/// - If the theory attribute is marked with Skip, or pre-enumeration is off, or any of the test data is non serializable, returns the result of <see cref="CreateTestCasesForTheory"/>;<br/>
	/// - If there is no theory data, returns a single test case of <see cref="ExecutionErrorTestCase"/> with the error in it;<br/>
	/// - Otherwise, it returns one test case per data row, created by calling <see cref="CreateTestCasesForDataRow"/>.
	/// </remarks>
	/// <param name="discoveryOptions">The discovery options to be used.</param>
	/// <param name="testMethod">The test method the test cases belong to.</param>
	/// <param name="factAttribute">The theory attribute attached to the test method.</param>
	/// <returns>Returns zero or more test cases represented by the test method.</returns>
	public virtual async ValueTask<IReadOnlyCollection<IXunitTestCase>> Discover(
		_ITestFrameworkDiscoveryOptions discoveryOptions,
		_ITestMethod testMethod,
		_IAttributeInfo factAttribute)
	{
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(factAttribute);

		// Special case Skip, because we want a single Skip (not one per data item); plus, a skipped theory may
		// not actually have any data (which is quasi-legal, since it's skipped).
		var theoryAttributeSkipReason = factAttribute.GetNamedArgument<string>(nameof(FactAttribute.Skip));
		if (theoryAttributeSkipReason is not null)
			return await CreateTestCasesForTheory(discoveryOptions, testMethod, factAttribute);

		var preEnumerate =
			discoveryOptions.PreEnumerateTheoriesOrDefault()
			&& !factAttribute.GetNamedArgument<bool>(nameof(TheoryAttribute.DisableDiscoveryEnumeration));

		if (preEnumerate)
		{
			DisposalTracker disposalTracker = new();
			try
			{
				var dataAttributes = testMethod.Method.GetCustomAttributes(typeof(DataAttribute));
				var results = new List<IXunitTestCase>();

				foreach (var dataAttribute in dataAttributes)
				{
					var discovererAttribute = dataAttribute.GetCustomAttributes(typeof(DataDiscovererAttribute)).First();
					IDataDiscoverer? discoverer;
					try
					{
						discoverer = ExtensibilityPointFactory.GetDataDiscoverer(discovererAttribute);
					}
					catch (InvalidCastException)
					{
						var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, factAttribute);

						if (dataAttribute is _IReflectionAttributeInfo reflectionAttribute)
							results.Add(
								new ExecutionErrorTestCase(
									details.ResolvedTestMethod,
									details.TestCaseDisplayName,
									details.UniqueID,
									string.Format(
										CultureInfo.CurrentCulture,
										"Data discoverer specified for {0} on {1}.{2} does not implement IDataDiscoverer.",
										reflectionAttribute.Attribute.GetType(),
										testMethod.TestClass.Class.Name,
										testMethod.Method.Name
									)
								)
							);
						else
							results.Add(
								new ExecutionErrorTestCase(
									testMethod,
									details.TestCaseDisplayName,
									details.UniqueID,
									string.Format(
										CultureInfo.CurrentCulture,
										"A data discoverer specified on {0}.{1} does not implement IDataDiscoverer.",
										testMethod.TestClass.Class.Name,
										testMethod.Method.Name
									)
								)
							);

						continue;
					}

					if (discoverer is null)
					{
						var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, factAttribute);

						if (dataAttribute is _IReflectionAttributeInfo reflectionAttribute)
							results.Add(
								new ExecutionErrorTestCase(
									details.ResolvedTestMethod,
									details.TestCaseDisplayName,
									details.UniqueID,
									string.Format(
										CultureInfo.CurrentCulture,
										"Data discoverer specified for {0} on {1}.{2} does not exist.",
										reflectionAttribute.Attribute.GetType(),
										testMethod.TestClass.Class.Name,
										testMethod.Method.Name
									)
								)
							);
						else
							results.Add(
								new ExecutionErrorTestCase(
									testMethod,
									details.TestCaseDisplayName,
									details.UniqueID,
									string.Format(
										CultureInfo.CurrentCulture,
										"A data discoverer specified on {0}.{1} does not exist.",
										testMethod.TestClass.Class.Name,
										testMethod.Method.Name
									)
								)
							);

						continue;
					}

					if (!discoverer.SupportsDiscoveryEnumeration(dataAttribute, testMethod.Method))
						return await CreateTestCasesForTheory(discoveryOptions, testMethod, factAttribute);

					var data = await discoverer.GetData(dataAttribute, testMethod.Method, disposalTracker);
					if (disposalTracker.TrackedObjects.Count > 0)
						return await CreateTestCasesForTheory(discoveryOptions, testMethod, factAttribute);

					if (data is null)
					{
						var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, factAttribute);

						results.Add(
							new ExecutionErrorTestCase(
								details.ResolvedTestMethod,
								details.TestCaseDisplayName,
								details.UniqueID,
								string.Format(
									CultureInfo.CurrentCulture,
									"Test data returned null for {0}.{1}. Make sure it is statically initialized before this test method is called.",
									testMethod.TestClass.Class.Name,
									testMethod.Method.Name
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
						var resolvedData = dataRow.GetData();
						if (testMethod.Method is _IReflectionMethodInfo reflectionMethodInfo)
							resolvedData = reflectionMethodInfo.MethodInfo.ResolveMethodArguments(resolvedData);

						if (!resolvedData.All(d => SerializationHelper.IsSerializable(d)))
						{
							var typeNames =
								resolvedData
									.Select(x => x?.GetType().FullName)
									.WhereNotNull()
									.Select(x => string.Format(CultureInfo.CurrentCulture, "'{0}'", x))
									.ToList();

							TestContext.Current?.SendDiagnosticMessage(
								"Non-serializable data (one or more of: {0}) found for '{1}.{2}'; falling back to single test case.",
								string.Join(", ", typeNames),
								testMethod.TestClass.Class.Name,
								testMethod.Method.Name
							);

							return await CreateTestCasesForTheory(discoveryOptions, testMethod, factAttribute);
						}

						try
						{
							results.AddRange(await CreateTestCasesForDataRow(discoveryOptions, testMethod, factAttribute, dataRow, resolvedData));
						}
						catch (Exception ex)
						{
							TestContext.Current?.SendDiagnosticMessage(
								"Error creating theory test case for for '{0}.{1}'; falling back to single test case. Exception message: '{2}'",
								testMethod.TestClass.Class.Name,
								testMethod.Method.Name,
								ex.Message
							);

							return await CreateTestCasesForTheory(discoveryOptions, testMethod, factAttribute);
						}
					}
				}

				if (results.Count == 0)
				{
					var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, factAttribute);

					results.Add(
						new ExecutionErrorTestCase(
							details.ResolvedTestMethod,
							details.TestCaseDisplayName,
							details.UniqueID,
							string.Format(
								CultureInfo.CurrentCulture,
								"No data found for {0}.{1}",
								testMethod.TestClass.Class.Name,
								testMethod.Method.Name
							)
						)
					);
				}

				return results;
			}
			catch (Exception ex)    // If something goes wrong, fall through to return just the XunitTestCase
			{
				TestContext.Current?.SendDiagnosticMessage(
					"Exception thrown during theory discovery on '{0}.{1}'; falling back to single test case.{2}{3}",
					testMethod.TestClass.Class.Name,
					testMethod.Method.Name,
					Environment.NewLine,
					ex
				);
			}
			finally
			{
				await disposalTracker.DisposeAsync();
			}
		}

		return await CreateTestCasesForTheory(discoveryOptions, testMethod, factAttribute);
	}
}
