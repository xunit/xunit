using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk
{
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
		/// <param name="displayName">The optional display name for the test</param>
		/// <param name="traits">The traits associated with the test case.</param>
		/// <param name="dataRow">The row of data for this test case.</param>
		/// <returns>The test cases</returns>
		protected virtual ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForDataRow(
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestMethod testMethod,
			_IAttributeInfo theoryAttribute,
			string? displayName,
			Dictionary<string, List<string>>? traits,
			object?[] dataRow)
		{
			var testCase = new XunitPreEnumeratedTheoryTestCase(
				discoveryOptions.MethodDisplayOrDefault(),
				discoveryOptions.MethodDisplayOptionsOrDefault(),
				testMethod,
				dataRow,
				traits: traits,
				displayName: displayName
			);

			return new(new[] { testCase });
		}

		/// <summary>
		/// Creates test cases for a single row of skipped data. By default, returns a single instance of <see cref="XunitSkippedDataRowTestCase"/>
		/// with the data row inside of it.
		/// </summary>
		/// <remarks>If this method is overridden, the implementation will have to override <see cref="TestMethodTestCase.SkipReason"/> otherwise
		/// the default behavior will look at the <see cref="TheoryAttribute"/> and the test case will not be skipped.</remarks>
		/// <param name="discoveryOptions">The discovery options to be used.</param>
		/// <param name="testMethod">The test method the test cases belong to.</param>
		/// <param name="theoryAttribute">The theory attribute attached to the test method.</param>
		/// <param name="displayName">The optional display name for the test</param>
		/// <param name="traits">The traits associated with the test case.</param>
		/// <param name="dataRow">The row of data for this test case.</param>
		/// <param name="skipReason">The reason this test case is to be skipped</param>
		/// <returns>The test cases</returns>
		protected virtual ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForSkippedDataRow(
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestMethod testMethod,
			_IAttributeInfo theoryAttribute,
			string? displayName,
			Dictionary<string, List<string>>? traits,
			object?[] dataRow,
			string skipReason)
		{
			var testCase = new XunitSkippedDataRowTestCase(
				discoveryOptions.MethodDisplayOrDefault(),
				discoveryOptions.MethodDisplayOptionsOrDefault(),
				testMethod,
				dataRow,
				skipReason,
				traits,
				displayName: displayName
			);

			return new(new[] { testCase });
		}

		/// <summary>
		/// Creates test cases for a skipped theory. By default, returns a single instance of <see cref="XunitTestCase"/>
		/// (which inherently discovers the skip reason via the fact attribute).
		/// </summary>
		/// <param name="discoveryOptions">The discovery options to be used.</param>
		/// <param name="testMethod">The test method the test cases belong to.</param>
		/// <param name="theoryAttribute">The theory attribute attached to the test method.</param>
		/// <param name="skipReason">The skip reason that decorates <paramref name="theoryAttribute"/>.</param>
		/// <returns>The test cases</returns>
		protected virtual ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForSkippedTheory(
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestMethod testMethod,
			_IAttributeInfo theoryAttribute,
			string skipReason)
		{
			// TODO: Skip reason should be passed down into the test case
			var testCase = new XunitTestCase(
				discoveryOptions.MethodDisplayOrDefault(),
				discoveryOptions.MethodDisplayOptionsOrDefault(),
				testMethod
			);

			return new(new[] { testCase });
		}

		/// <summary>
		/// Creates test cases for the entire theory. This is used when one or more of the theory data items
		/// are not serializable, or if the user has requested to skip theory pre-enumeration. By default,
		/// returns a single instance of <see cref="XunitDelayEnumeratedTheoryTestCase"/>, which performs the data discovery
		/// at runtime.
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
			var testCase = new XunitDelayEnumeratedTheoryTestCase(
				discoveryOptions.MethodDisplayOrDefault(),
				discoveryOptions.MethodDisplayOptionsOrDefault(),
				testMethod
			);

			return new(new[] { testCase });
		}

		/// <summary>
		/// Discover test cases from a test method.
		/// </summary>
		/// <remarks>
		/// This method performs the following steps:
		/// - If the theory attribute is marked with Skip, returns the single test case from <see cref="CreateTestCasesForSkippedTheory"/>;
		/// - If pre-enumeration is off, or any of the test data is non serializable, returns the single test case from <see cref="CreateTestCasesForTheory"/>;
		/// - If there is no theory data, returns a single test case of <see cref="ExecutionErrorTestCase"/> with the error in it;
		/// - Otherwise, it returns one test case per data row, created by calling <see cref="CreateTestCasesForDataRow"/> or <see cref="CreateTestCasesForSkippedDataRow"/> if the data attribute has a skip reason.
		/// </remarks>
		/// <param name="discoveryOptions">The discovery options to be used.</param>
		/// <param name="testMethod">The test method the test cases belong to.</param>
		/// <param name="theoryAttribute">The theory attribute attached to the test method.</param>
		/// <returns>Returns zero or more test cases represented by the test method.</returns>
		public virtual async ValueTask<IReadOnlyCollection<IXunitTestCase>> Discover(
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestMethod testMethod,
			_IAttributeInfo theoryAttribute)
		{
			Guard.ArgumentNotNull(discoveryOptions);
			Guard.ArgumentNotNull(testMethod);
			Guard.ArgumentNotNull(theoryAttribute);

			// Special case Skip, because we want a single Skip (not one per data item); plus, a skipped test may
			// not actually have any data (which is quasi-legal, since it's skipped).
			var skipReason = theoryAttribute.GetNamedArgument<string>("Skip");
			if (skipReason != null)
				return await CreateTestCasesForSkippedTheory(discoveryOptions, testMethod, theoryAttribute, skipReason);

			var preEnumerate =
				discoveryOptions.PreEnumerateTheoriesOrDefault()
				&& !theoryAttribute.GetNamedArgument<bool>(nameof(TheoryAttribute.DisableDiscoveryEnumeration));

			if (preEnumerate)
			{
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
							if (dataAttribute is _IReflectionAttributeInfo reflectionAttribute)
								results.Add(
									new ExecutionErrorTestCase(
										discoveryOptions.MethodDisplayOrDefault(),
										discoveryOptions.MethodDisplayOptionsOrDefault(),
										testMethod,
										$"Data discoverer specified for {reflectionAttribute.Attribute.GetType()} on {testMethod.TestClass.Class.Name}.{testMethod.Method.Name} does not implement IDataDiscoverer."
									)
								);
							else
								results.Add(
									new ExecutionErrorTestCase(
										discoveryOptions.MethodDisplayOrDefault(),
										discoveryOptions.MethodDisplayOptionsOrDefault(),
										testMethod,
										$"A data discoverer specified on {testMethod.TestClass.Class.Name}.{testMethod.Method.Name} does not implement IDataDiscoverer."
									)
								);

							continue;
						}

						if (discoverer == null)
						{
							if (dataAttribute is _IReflectionAttributeInfo reflectionAttribute)
								results.Add(
									new ExecutionErrorTestCase(
										discoveryOptions.MethodDisplayOrDefault(),
										discoveryOptions.MethodDisplayOptionsOrDefault(),
										testMethod,
										$"Data discoverer specified for {reflectionAttribute.Attribute.GetType()} on {testMethod.TestClass.Class.Name}.{testMethod.Method.Name} does not exist."
									)
								);
							else
								results.Add(
									new ExecutionErrorTestCase(
										discoveryOptions.MethodDisplayOrDefault(),
										discoveryOptions.MethodDisplayOptionsOrDefault(),
										testMethod,
										$"A data discoverer specified on {testMethod.TestClass.Class.Name}.{testMethod.Method.Name} does not exist."
									)
								);

							continue;
						}

						skipReason = dataAttribute.GetNamedArgument<string>("Skip");

						if (!discoverer.SupportsDiscoveryEnumeration(dataAttribute, testMethod.Method))
							return await CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute);

						var data = await discoverer.GetData(dataAttribute, testMethod.Method);
						if (data == null)
						{
							results.Add(
								new ExecutionErrorTestCase(
									discoveryOptions.MethodDisplayOrDefault(),
									discoveryOptions.MethodDisplayOptionsOrDefault(),
									testMethod,
									$"Test data returned null for {testMethod.TestClass.Class.Name}.{testMethod.Method.Name}. Make sure it is statically initialized before this test method is called."
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
							var dataRowSkipReason = skipReason ?? dataRow.Skip;
							if (testMethod.Method is _IReflectionMethodInfo reflectionMethodInfo)
								resolvedData = reflectionMethodInfo.MethodInfo.ResolveMethodArguments(resolvedData);

							if (!resolvedData.All(d => SerializationHelper.IsSerializable(d)))
							{
								var typeNames =
									resolvedData
										.Select(x => x?.GetType().FullName)
										.WhereNotNull()
										.Select(x => $"'{x}'")
										.ToList();

								TestContext.Current?.SendDiagnosticMessage(
									"Non-serializable data (one or more of: {0}) found for '{1}.{2}'; falling back to single test case.",
									string.Join(", ", typeNames),
									testMethod.TestClass.Class.Name,
									testMethod.Method.Name
								);

								return await CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute);
							}

							try
							{
								var testCases =
									dataRowSkipReason != null
										? CreateTestCasesForSkippedDataRow(discoveryOptions, testMethod, theoryAttribute, dataRow.TestDisplayName, dataRow.Traits, resolvedData, dataRowSkipReason)
										: CreateTestCasesForDataRow(discoveryOptions, testMethod, theoryAttribute, dataRow.TestDisplayName, dataRow.Traits, resolvedData);

								results.AddRange(await testCases);
							}
							catch (Exception ex)
							{
								TestContext.Current?.SendDiagnosticMessage(
									"Error creating theory test case for for '{0}.{1}'; falling back to single test case. Exception message: '{2}'",
									testMethod.TestClass.Class.Name,
									testMethod.Method.Name,
									ex.Message
								);

								return await CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute);
							}
						}
					}

					if (results.Count == 0)
						results.Add(
							new ExecutionErrorTestCase(
								discoveryOptions.MethodDisplayOrDefault(),
								discoveryOptions.MethodDisplayOptionsOrDefault(),
								testMethod,
								$"No data found for {testMethod.TestClass.Class.Name}.{testMethod.Method.Name}"
							)
						);

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
			}

			return await CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute);
		}
	}
}
