using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// A base class implementation of <see cref="_ITestCase"/> which is based on test cases being
	/// related directly to test methods.
	/// </summary>
	[Serializable]
	public abstract class TestMethodTestCase : _ITestCase, ISerializable, IAsyncDisposable
	{
		string displayName;
		readonly DisposalTracker disposalTracker = new();
		readonly DisplayNameFormatter formatter;
		string uniqueID;

		/// <summary>
		/// Used for de-serialization.
		/// </summary>
		protected TestMethodTestCase(
			SerializationInfo info,
			StreamingContext context)
		{
			DefaultMethodDisplay = info.GetValue<TestMethodDisplay>("DefaultMethodDisplay");
			DefaultMethodDisplayOptions = info.GetValue<TestMethodDisplayOptions>("DefaultMethodDisplayOptions");
			displayName = Guard.NotNull("Could not retrieve DisplayName from serialization", info.GetValue<string>("DisplayName"));
			SkipReason = info.GetValue<string>("SkipReason");
			TestMethod = Guard.NotNull("Could not retrieve TestMethod from serialization", info.GetValue<_ITestMethod>("TestMethod"));
			TestMethodArguments = info.GetValue<object[]>("TestMethodArguments");
			Traits = SerializationHelper.DeserializeTraits(info).ToReadWrite(StringComparer.OrdinalIgnoreCase);
			uniqueID = Guard.NotNull("Could not retrieve UniqueID from serialization", info.GetValue<string>("UniqueID"));

			formatter = new DisplayNameFormatter(DefaultMethodDisplay, DefaultMethodDisplayOptions);

			var initResults = Initialize(BaseDisplayName, TestMethod, TestMethodArguments);

			InitializationException = initResults.initException;
			Method = initResults.method;
			MethodGenericTypes = initResults.methodGenericTypes;

			disposalTracker.AddRange(TestMethodArguments);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TestMethodTestCase"/> class.
		/// </summary>
		/// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
		/// <param name="defaultMethodDisplayOptions">Default method display options to use (when not customized).</param>
		/// <param name="testMethod">The test method this test case belongs to.</param>
		/// <param name="testMethodArguments">The optional arguments for the test method.</param>
		/// <param name="skipReason">The optional reason for skipping the test.</param>
		/// <param name="traits">The optional traits list.</param>
		/// <param name="uniqueID">The optional unique ID for the test case; if not provided, will be calculated.</param>
		/// <param name="displayName">The optional display name for the test</param>
		protected TestMethodTestCase(
			TestMethodDisplay defaultMethodDisplay,
			TestMethodDisplayOptions defaultMethodDisplayOptions,
			_ITestMethod testMethod,
			object?[]? testMethodArguments = null,
			string? skipReason = null,
			Dictionary<string, List<string>>? traits = null,
			string? uniqueID = null,
			string? displayName = null)
		{
			DefaultMethodDisplay = defaultMethodDisplay;
			DefaultMethodDisplayOptions = defaultMethodDisplayOptions;
			SkipReason = skipReason;
			TestMethod = Guard.ArgumentNotNull(testMethod);
			TestMethodArguments = testMethodArguments;

			if (traits != null)
				Traits = new Dictionary<string, List<string>>(traits, StringComparer.OrdinalIgnoreCase);
			else
				Traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

			formatter = new DisplayNameFormatter(defaultMethodDisplay, defaultMethodDisplayOptions);

			var baseDisplayName = displayName ?? BaseDisplayName;
			var initResults = Initialize(baseDisplayName, testMethod, TestMethodArguments);

			this.displayName = initResults.displayName;
			InitializationException = initResults.initException;
			Method = initResults.method;
			MethodGenericTypes = initResults.methodGenericTypes;
			this.uniqueID = uniqueID ?? UniqueIDGenerator.ForTestCase(TestMethod.UniqueID, MethodGenericTypes, TestMethodArguments);

			disposalTracker.AddRange(TestMethodArguments);
		}

		static (string displayName, Exception? initException, _IMethodInfo method, _ITypeInfo[]? methodGenericTypes) Initialize(
			string baseDisplayName,
			_ITestMethod testMethod,
			object?[]? testMethodArguments)
		{
			string? displayName = null;
			Exception? initException = null;
			_ITypeInfo[]? methodGenericTypes = null;

			var method = testMethod.Method;

			if (testMethodArguments != null)
			{
				if (method is _IReflectionMethodInfo reflectionMethod)
				{
					try
					{
						testMethodArguments = reflectionMethod.MethodInfo.ResolveMethodArguments(testMethodArguments);
					}
					catch (Exception ex)
					{
						initException = ex;
						testMethodArguments = null;
						displayName = $"{baseDisplayName}(???)";
					}
				}
			}

			if (testMethodArguments != null && method.IsGenericMethodDefinition)
			{
				methodGenericTypes = method.ResolveGenericTypes(testMethodArguments);
				method = method.MakeGenericMethod(methodGenericTypes);
			}

			if (displayName == null)
				displayName = method.GetDisplayNameWithArguments(baseDisplayName, testMethodArguments, methodGenericTypes);

			return (displayName, initException, method, methodGenericTypes);
		}

		/// <summary>
		/// Returns the base display name for a test; the actual value depends on <see cref="DefaultMethodDisplay"/>.
		/// "TestClassName.MethodName" for <see cref="TestMethodDisplay.ClassAndMethod"/>, or "MethodName"
		/// for <see cref="TestMethodDisplay.Method"/>.
		/// </summary>
		protected string BaseDisplayName
		{
			get
			{
				if (DefaultMethodDisplay == TestMethodDisplay.ClassAndMethod)
					return formatter.Format($"{TestMethod.TestClass.Class.Name}.{TestMethod.Method.Name}");

				return formatter.Format(TestMethod.Method.Name);
			}
		}

		/// <summary>
		/// Returns the default method display to use (when not customized).
		/// </summary>
		protected internal TestMethodDisplay DefaultMethodDisplay { get; }

		/// <summary>
		/// Returns the default method display options to use (when not customized).
		/// </summary>
		protected internal TestMethodDisplayOptions DefaultMethodDisplayOptions { get; }

		/// <inheritdoc/>
		public string TestCaseDisplayName
		{
			get => displayName;
			protected set => displayName = Guard.ArgumentNotNull(value, nameof(TestCaseDisplayName));
		}

		/// <summary>
		/// Gets or sets the exception that happened during initialization. When this is set, then
		/// the test execution should fail with this exception.
		/// </summary>
		public Exception? InitializationException { get; }

		/// <inheritdoc/>
		public _IMethodInfo Method { get; }

		/// <summary>
		/// Gets the generic types that were used to close the generic test method, if
		/// applicable; <c>null</c>, if the test method was not an open generic.
		/// </summary>
		protected _ITypeInfo[]? MethodGenericTypes { get; }

		/// <inheritdoc/>
		public string? SkipReason { get; protected set; }

		/// <inheritdoc/>
		public string? SourceFilePath { get; set; }

		/// <inheritdoc/>
		public int? SourceLineNumber { get; set; }

		/// <inheritdoc/>
		public _ITestCollection TestCollection =>
			TestMethod.TestClass.TestCollection;

		string? _ITestCaseMetadata.TestClassName =>
			TestMethod.TestClass.Class.SimpleName;

		string? _ITestCaseMetadata.TestClassNamespace =>
			TestMethod.TestClass.Class.Namespace;

		string? _ITestCaseMetadata.TestClassNameWithNamespace =>
			TestMethod.TestClass.Class.Name;

		/// <inheritdoc/>
		public _ITestMethod TestMethod { get; }

		string _ITestCaseMetadata.TestMethodName =>
			TestMethod.Method.Name;

		/// <inheritdoc/>
		public object?[]? TestMethodArguments { get; }

		/// <inheritdoc/>
		public Dictionary<string, List<string>> Traits { get; }

		IReadOnlyDictionary<string, IReadOnlyList<string>> _ITestCaseMetadata.Traits =>
			Traits.ToReadOnly();

		/// <inheritdoc/>
		public virtual string UniqueID
		{
			get => uniqueID;
			protected set => uniqueID = Guard.ArgumentNotNull(value, nameof(UniqueID));
		}

		/// <inheritdoc/>
		public virtual ValueTask DisposeAsync() =>
			disposalTracker.DisposeAsync();

		/// <inheritdoc/>
		public virtual void GetObjectData(
			SerializationInfo info,
			StreamingContext context)
		{
			info.AddValue("DefaultMethodDisplay", DefaultMethodDisplay);
			info.AddValue("DefaultMethodDisplayOptions", DefaultMethodDisplayOptions);
			info.AddValue("DisplayName", TestCaseDisplayName);
			info.AddValue("SkipReason", SkipReason);
			info.AddValue("TestMethod", TestMethod);
			info.AddValue("TestMethodArguments", TestMethodArguments);
			info.AddValue("UniqueID", UniqueID);

			SerializationHelper.SerializeTraits(info, Traits);
		}
	}
}
