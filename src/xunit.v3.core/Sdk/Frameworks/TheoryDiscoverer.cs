using System;
using System.Collections.Generic;
using System.Linq;
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
		/// Initializes a new instance of the <see cref="TheoryDiscoverer"/> class.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		public TheoryDiscoverer(_IMessageSink diagnosticMessageSink)
		{
			DiagnosticMessageSink = Guard.ArgumentNotNull(nameof(diagnosticMessageSink), diagnosticMessageSink);
		}

		/// <summary>
		/// Gets the message sink to be used to send diagnostic messages.
		/// </summary>
		protected _IMessageSink DiagnosticMessageSink { get; }

		/// <summary>
		/// Creates test cases for a single row of data. By default, returns a single instance of <see cref="XunitTestCase"/>
		/// with the data row inside of it.
		/// </summary>
		/// <param name="discoveryOptions">The discovery options to be used.</param>
		/// <param name="testMethod">The test method the test cases belong to.</param>
		/// <param name="theoryAttribute">The theory attribute attached to the test method.</param>
		/// <param name="dataRow">The row of data for this test case.</param>
		/// <returns>The test cases</returns>
		protected virtual IReadOnlyCollection<IXunitTestCase> CreateTestCasesForDataRow(
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestMethod testMethod,
			_IAttributeInfo theoryAttribute,
			object?[] dataRow)
		{
			var testCase = new XunitPreEnumeratedTheoryTestCase(
				DiagnosticMessageSink,
				discoveryOptions.MethodDisplayOrDefault(),
				discoveryOptions.MethodDisplayOptionsOrDefault(),
				testMethod,
				dataRow
			);

			return new[] { testCase };
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
		protected virtual IReadOnlyCollection<IXunitTestCase> CreateTestCasesForSkip(
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestMethod testMethod,
			_IAttributeInfo theoryAttribute,
			string skipReason)
		{
			// TODO: Skip reason should be passed down into the test case
			var testCase = new XunitTestCase(
				DiagnosticMessageSink,
				discoveryOptions.MethodDisplayOrDefault(),
				discoveryOptions.MethodDisplayOptionsOrDefault(),
				testMethod
			);

			return new[] { testCase };
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
		protected virtual IReadOnlyCollection<IXunitTestCase> CreateTestCasesForTheory(
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestMethod testMethod,
			_IAttributeInfo theoryAttribute)
		{
			var testCase = new XunitDelayEnumeratedTheoryTestCase(
				DiagnosticMessageSink,
				discoveryOptions.MethodDisplayOrDefault(),
				discoveryOptions.MethodDisplayOptionsOrDefault(),
				testMethod
			);

			return new[] { testCase };
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
		/// <param name="dataRow">The row of data for this test case.</param>
		/// <param name="skipReason">The reason this test case is to be skipped</param>
		/// <returns>The test cases</returns>
		protected virtual IReadOnlyCollection<IXunitTestCase> CreateTestCasesForSkippedDataRow(
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestMethod testMethod,
			_IAttributeInfo theoryAttribute,
			object?[] dataRow,
			string skipReason)
		{
			var testCase = new XunitSkippedDataRowTestCase(
				DiagnosticMessageSink,
				discoveryOptions.MethodDisplayOrDefault(),
				discoveryOptions.MethodDisplayOptionsOrDefault(),
				testMethod,
				dataRow
,
				skipReason);

			return new[] { testCase };
		}

		/// <summary>
		/// Discover test cases from a test method.
		/// </summary>
		/// <remarks>
		/// This method performs the following steps:
		/// - If the theory attribute is marked with Skip, returns the single test case from <see cref="CreateTestCasesForSkip"/>;
		/// - If pre-enumeration is off, or any of the test data is non serializable, returns the single test case from <see cref="CreateTestCasesForTheory"/>;
		/// - If there is no theory data, returns a single test case of <see cref="ExecutionErrorTestCase"/> with the error in it;
		/// - Otherwise, it returns one test case per data row, created by calling <see cref="CreateTestCasesForDataRow"/> or <see cref="CreateTestCasesForSkippedDataRow"/> if the data attribute has a skip reason.
		/// </remarks>
		/// <param name="discoveryOptions">The discovery options to be used.</param>
		/// <param name="testMethod">The test method the test cases belong to.</param>
		/// <param name="theoryAttribute">The theory attribute attached to the test method.</param>
		/// <returns>Returns zero or more test cases represented by the test method.</returns>
		public virtual IReadOnlyCollection<IXunitTestCase> Discover(
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestMethod testMethod,
			_IAttributeInfo theoryAttribute)
		{
			Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);
			Guard.ArgumentNotNull(nameof(testMethod), testMethod);
			Guard.ArgumentNotNull(nameof(theoryAttribute), theoryAttribute);

			// Special case Skip, because we want a single Skip (not one per data item); plus, a skipped test may
			// not actually have any data (which is quasi-legal, since it's skipped).
			var skipReason = theoryAttribute.GetNamedArgument<string>("Skip");
			if (skipReason != null)
				return CreateTestCasesForSkip(discoveryOptions, testMethod, theoryAttribute, skipReason);

			if (discoveryOptions.PreEnumerateTheoriesOrDefault())
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
							discoverer = ExtensibilityPointFactory.GetDataDiscoverer(DiagnosticMessageSink, discovererAttribute);
						}
						catch (InvalidCastException)
						{
							if (dataAttribute is _IReflectionAttributeInfo reflectionAttribute)
								results.Add(
									new ExecutionErrorTestCase(
										DiagnosticMessageSink,
										discoveryOptions.MethodDisplayOrDefault(),
										discoveryOptions.MethodDisplayOptionsOrDefault(),
										testMethod,
										$"Data discoverer specified for {reflectionAttribute.Attribute.GetType()} on {testMethod.TestClass.Class.Name}.{testMethod.Method.Name} does not implement IDataDiscoverer."
									)
								);
							else
								results.Add(
									new ExecutionErrorTestCase(
										DiagnosticMessageSink,
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
										DiagnosticMessageSink,
										discoveryOptions.MethodDisplayOrDefault(),
										discoveryOptions.MethodDisplayOptionsOrDefault(),
										testMethod,
										$"Data discoverer specified for {reflectionAttribute.Attribute.GetType()} on {testMethod.TestClass.Class.Name}.{testMethod.Method.Name} does not exist."
									)
								);
							else
								results.Add(
									new ExecutionErrorTestCase(
										DiagnosticMessageSink,
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
							return CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute);

						var data = discoverer.GetData(dataAttribute, testMethod.Method);
						if (data == null)
						{
							results.Add(
								new ExecutionErrorTestCase(
									DiagnosticMessageSink,
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
							var resolvedData = dataRow;
							if (testMethod.Method is _IReflectionMethodInfo reflectionMethodInfo)
								resolvedData = reflectionMethodInfo.MethodInfo.ResolveMethodArguments(dataRow);

							if (!resolvedData.All(d => SerializationHelper.IsSerializable(d)))
							{
								var typeNames =
									resolvedData
										.Select(x => x?.GetType().FullName)
										.WhereNotNull()
										.Select(x => $"'{x}'")
										.ToList();

								DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Non-serializable data (one or more of: {string.Join(", ", typeNames)}) found for '{testMethod.TestClass.Class.Name}.{testMethod.Method.Name}'; falling back to single test case." });
								return CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute);
							}

							try
							{
								var testCases =
									skipReason != null
										? CreateTestCasesForSkippedDataRow(discoveryOptions, testMethod, theoryAttribute, resolvedData, skipReason)
										: CreateTestCasesForDataRow(discoveryOptions, testMethod, theoryAttribute, resolvedData);

								results.AddRange(testCases);
							}
							catch (Exception ex)
							{
								DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Error creating theory test case for for '{testMethod.TestClass.Class.Name}.{testMethod.Method.Name}'; falling back to single test case. Exception message: '{ex.Message}'" });
								return CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute);
							}
						}
					}

					if (results.Count == 0)
						results.Add(
							new ExecutionErrorTestCase(
								DiagnosticMessageSink,
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
					DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Exception thrown during theory discovery on '{testMethod.TestClass.Class.Name}.{testMethod.Method.Name}'; falling back to single test case.{Environment.NewLine}{ex}" });
				}
			}

			return CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute);
		}
	}
}
