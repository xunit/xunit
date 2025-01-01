using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents a test case which runs multiple tests for theory data, either because theory
/// data pre-enumeration was disabled or because the data was not serializable.
/// </summary>
public class XunitDelayEnumeratedTheoryTestCase : XunitTestCase, IXunitDelayEnumeratedTestCase
{
	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public XunitDelayEnumeratedTheoryTestCase()
	{ }

	/// <summary>
	/// Gets a flag which indicates whether a theory without data is skipped rather than failed.
	/// </summary>
	public bool SkipTestWithoutData { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitDelayEnumeratedTheoryTestCase"/> class.
	/// </summary>
	/// <param name="testMethod">The test method this test case belongs to.</param>
	/// <param name="testCaseDisplayName">The display name for the test case.</param>
	/// <param name="uniqueID">The optional unique ID for the test case; if not provided, will be calculated.</param>
	/// <param name="explicit">Indicates whether the test case was marked as explicit.</param>
	/// <param name="skipTestWithoutData">Set to <c>true</c> to skip if the test has no data, rather than fail.</param>
	/// <param name="skipExceptions">The value obtained from <see cref="IFactAttribute.SkipExceptions"/>.</param>
	/// <param name="skipReason">The value from <see cref="IFactAttribute.Skip"/></param>
	/// <param name="skipType">The value from <see cref="IFactAttribute.SkipType"/> </param>
	/// <param name="skipUnless">The value from <see cref="IFactAttribute.SkipUnless"/></param>
	/// <param name="skipWhen">The value from <see cref="IFactAttribute.SkipWhen"/></param>
	/// <param name="traits">The optional traits list.</param>
	/// <param name="sourceFilePath">The optional source file in where this test case originated.</param>
	/// <param name="sourceLineNumber">The optional source line number where this test case originated.</param>
	/// <param name="timeout">The optional timeout for the test case (in milliseconds).</param>
	public XunitDelayEnumeratedTheoryTestCase(
		IXunitTestMethod testMethod,
		string testCaseDisplayName,
		string uniqueID,
		bool @explicit,
		bool skipTestWithoutData,
		Type[]? skipExceptions = null,
		string? skipReason = null,
		Type? skipType = null,
		string? skipUnless = null,
		string? skipWhen = null,
		Dictionary<string, HashSet<string>>? traits = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		int? timeout = null) :
			base(
				testMethod,
				testCaseDisplayName,
				uniqueID,
				@explicit,
				skipExceptions,
				skipReason,
				skipType,
				skipUnless,
				skipWhen,
				traits,
				testMethodArguments: null,
				sourceFilePath,
				sourceLineNumber,
				timeout
			) =>
				SkipTestWithoutData = skipTestWithoutData;

	/// <summary>
	/// Enumerates the theory data and creates tests to be run.
	/// </summary>
	public override async ValueTask<IReadOnlyCollection<IXunitTest>> CreateTests()
	{
		var testIndex = 0;
		var result = new List<IXunitTest>();

		foreach (var dataAttribute in TestMethod.DataAttributes)
		{
			var data =
				await dataAttribute.GetData(TestMethod.Method, DisposalTracker)
					?? throw new TestPipelineException(
						string.Format(
							CultureInfo.CurrentCulture,
							"Test data returned null for {0}.{1}. Make sure it is statically initialized before this test method is called.",
							TestMethod.TestClass.TestClassName,
							TestMethod.MethodName
						)
					);

			foreach (var dataRow in data)
			{
				var dataRowData = dataRow.GetData();
				DisposalTracker.AddRange(dataRowData);

				var testMethod = TestMethod;
				var resolvedTypes = testMethod.ResolveGenericTypes(dataRowData);
				if (resolvedTypes is not null)
					testMethod = new XunitTestMethod(testMethod.TestClass, testMethod.MakeGenericMethod(resolvedTypes), dataRowData);

				var convertedDataRow = testMethod.ResolveMethodArguments(dataRowData);

				var parameterTypes = testMethod.Parameters.Select(p => p.ParameterType).ToArray();
				convertedDataRow = TypeHelper.ConvertArguments(convertedDataRow, parameterTypes);

				var baseDisplayName = dataRow.TestDisplayName ?? dataAttribute.TestDisplayName ?? TestCaseDisplayName;
				var theoryDisplayName = testMethod.GetDisplayName(baseDisplayName, convertedDataRow, resolvedTypes);
				var traits = TestIntrospectionHelper.GetTraits(testMethod, dataRow);
				var timeout = dataRow.Timeout ?? dataAttribute.Timeout ?? Timeout;
				var skipReason = dataRow.Skip ?? dataAttribute.Skip ?? SkipReason;
				var test = new XunitTest(
					this,
					testMethod,
					dataRow.Explicit,
					skipReason,
					theoryDisplayName,
					testIndex++,
					traits.ToReadOnly(),
					timeout,
					convertedDataRow
				);

				result.Add(test);
			}

			if (result.Count == 0)
			{
				var message = string.Format(CultureInfo.CurrentCulture, "No data found for {0}.{1}", TestMethod.TestClass.TestClassName, TestMethod.MethodName);
				throw new TestPipelineException(SkipTestWithoutData ? DynamicSkipToken.Value + message : message);
			}
		}

		return result;
	}

	/// <inheritdoc/>
	protected override void Deserialize(IXunitSerializationInfo info)
	{
		base.Deserialize(info);

		SkipTestWithoutData = info.GetValue<bool>("swd");
	}

	/// <inheritdoc/>
	protected override void Serialize(IXunitSerializationInfo info)
	{
		base.Serialize(info);

		info.AddValue("swd", SkipTestWithoutData);
	}
}
