using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v1
{
	/// <summary>
	/// Contains the data required to serialize a test case for xUnit.net v1.
	/// </summary>
	[Serializable]
	public class Xunit1TestCase : ISerializable
	{
		string? assemblyUniqueID;
		string? testCollectionUniqueID;
		string? testCaseDisplayName;
		string? testCaseUniqueID;
		string? testClass;
		string? testClassUniqueID;
		string? testMethod;
		string? testMethodUniqueID;
		Dictionary<string, List<string>> traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit1TestCase"/> class.
		/// </summary>
		public Xunit1TestCase()
		{ }

		/// <summary>
		/// Deserialization constructor.
		/// </summary>
		protected Xunit1TestCase(
			SerializationInfo info,
			StreamingContext context)
		{
			AssemblyUniqueID = Guard.NotNull("Could not retrieve AssemblyUniqueID from serialization", info.GetString("AssemblyUniqueID"));
			SkipReason = info.GetString("SkipReason");
			SourceFilePath = info.GetString("SourceFilePath");
			TestCollectionUniqueID = Guard.NotNull("Could not retrieve TestCollectionUniqueID from serialization", info.GetString("TestCollectionUniqueID"));
			TestCaseDisplayName = Guard.NotNull("Could not retrieve TestCaseDisplayName from serialization", info.GetString("TestCaseDisplayName"));
			TestCaseUniqueID = Guard.NotNull("Could not retrieve TestCaseUniqueID from serialization", info.GetString("TestCaseUniqueID"));
			TestClass = Guard.NotNull("Could not retrieve TestClass from serialization", info.GetString("TestClass"));
			TestClassUniqueID = Guard.NotNull("Could not retrieve TestClassUniqueID from serialization", info.GetString("TestClassUniqueID"));
			TestMethod = Guard.NotNull("Could not retrieve TestMethod from serialization", info.GetString("TestMethod"));
			TestMethodUniqueID = Guard.NotNull("Could not retrieve TestMethodUniqueID from serialization", info.GetString("TestMethodUniqueID"));
			Traits = SerializationHelper.DeserializeTraits(info);

			var sourceLineNumberText = info.GetString("SourceLineNumber");
			if (sourceLineNumberText != null)
				SourceLineNumber = int.Parse(sourceLineNumberText);
		}

		/// <summary>
		/// Gets the unique ID for the test assembly.
		/// </summary>
		public string AssemblyUniqueID
		{
			get => assemblyUniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(AssemblyUniqueID)} on an uninitialized '{GetType().FullName}' object");
			set => assemblyUniqueID = Guard.ArgumentNotNull(nameof(AssemblyUniqueID), value);
		}

		/// <summary>
		/// Gets the reason this test is being skipped; will return <c>null</c> when
		/// the test is not skipped.
		/// </summary>
		public string? SkipReason { get; set; }

		/// <summary>
		/// Gets the source file path of the test method, if known.
		/// </summary>
		public string? SourceFilePath { get; set; }

		/// <summary>
		/// Gets the source line number of the test method, if known.
		/// </summary>
		public int? SourceLineNumber { get; set; }

		/// <summary>
		/// Gets the unique ID of the test collection.
		/// </summary>
		public string TestCollectionUniqueID
		{
			get => testCollectionUniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(TestCollectionUniqueID)} on an uninitialized '{GetType().FullName}' object");
			set => testCollectionUniqueID = Guard.ArgumentNotNull(nameof(TestCollectionUniqueID), value);
		}

		/// <summary>
		/// Gets the display name for the test case.
		/// </summary>
		public string TestCaseDisplayName
		{
			get => testCaseDisplayName ?? throw new InvalidOperationException($"Attempted to get {nameof(TestCaseDisplayName)} on an uninitialized '{GetType().FullName}' object");
			set => testCaseDisplayName = Guard.ArgumentNotNull(nameof(TestCaseDisplayName), value);
		}

		/// <summary>
		/// Gets the unique ID for the test case.
		/// </summary>
		public string TestCaseUniqueID
		{
			get => testCaseUniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(TestCaseUniqueID)} on an uninitialized '{GetType().FullName}' object");
			set => testCaseUniqueID = Guard.ArgumentNotNull(nameof(TestCaseUniqueID), value);
		}

		/// <summary>
		/// Gets the fully qualified type name of the test class.
		/// </summary>
		public string TestClass
		{
			get => testClass ?? throw new InvalidOperationException($"Attempted to get {nameof(TestClass)} on an uninitialized '{GetType().FullName}' object");
			set => testClass = Guard.ArgumentNotNull(nameof(TestClass), value);
		}

		/// <summary>
		/// Gets the unique ID for the test class.
		/// </summary>
		public string TestClassUniqueID
		{
			get => testClassUniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(TestClassUniqueID)} on an uninitialized '{GetType().FullName}' object");
			set => testClassUniqueID = Guard.ArgumentNotNull(nameof(TestClassUniqueID), value);
		}

		/// <summary>
		/// Gets the name of the test method.
		/// </summary>
		public string TestMethod
		{
			get => testMethod ?? throw new InvalidOperationException($"Attempted to get {nameof(TestMethod)} on an uninitialized '{GetType().FullName}' object");
			set => testMethod = Guard.ArgumentNotNull(nameof(TestMethod), value);
		}

		/// <summary>
		/// Gets the unique ID of the test method.
		/// </summary>
		public string TestMethodUniqueID
		{
			get => testMethodUniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(TestMethodUniqueID)} on an uninitialized '{GetType().FullName}' object");
			set => testMethodUniqueID = Guard.ArgumentNotNull(nameof(TestMethodUniqueID), value);
		}

		/// <summary>
		/// Gets the traits that are associated with this test case.
		/// </summary>
		public Dictionary<string, List<string>> Traits
		{
			get => traits;
			set
			{
				traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
				if (value != null)
					foreach (var kvp in value)
						traits[kvp.Key] = kvp.Value;
			}
		}

		void ISerializable.GetObjectData(
			SerializationInfo info,
			StreamingContext context)
		{
			info.AddValue("AssemblyUniqueID", AssemblyUniqueID);
			info.AddValue("SkipReason", SkipReason);
			info.AddValue("SourceFilePath", SourceFilePath);
			info.AddValue("SourceLineNumber", SourceLineNumber?.ToString());
			info.AddValue("TestCollectionUniqueID", TestCollectionUniqueID);
			info.AddValue("TestCaseDisplayName", TestCaseDisplayName);
			info.AddValue("TestCaseUniqueID", TestCaseUniqueID);
			info.AddValue("TestClass", TestClass);
			info.AddValue("TestClassUniqueID", TestClassUniqueID);
			info.AddValue("TestMethod", TestMethod);
			info.AddValue("TestMethodUniqueID", TestMethodUniqueID);
			SerializationHelper.SerializeTraits(info, Traits);
		}

		/// <summary>
		/// Converts the test case to <see cref="_TestCaseDiscovered"/>, with optional
		/// serialization of the test case.
		/// </summary>
		/// <param name="includeSerialization">A flag to indicate whether serialization is needed.</param>
		/// <returns>The converted test case</returns>
		public _TestCaseDiscovered ToTestCaseDiscovered(bool includeSerialization)
		{
			string? @namespace = null;
			string? @class = null;

			var namespaceIdx = TestClass.LastIndexOf('.');
			if (namespaceIdx < 0)
				@class = TestClass;
			else
			{
				@namespace = TestClass.Substring(0, namespaceIdx);
				@class = TestClass.Substring(namespaceIdx + 1);

				var innerClassIdx = @class.LastIndexOf('+');
				if (innerClassIdx >= 0)
					@class = @class.Substring(innerClassIdx + 1);
			}

			var result = new _TestCaseDiscovered
			{
				AssemblyUniqueID = AssemblyUniqueID,
				SkipReason = SkipReason,
				SourceFilePath = SourceFilePath,
				SourceLineNumber = SourceLineNumber,
				TestCaseDisplayName = TestCaseDisplayName,
				TestCaseUniqueID = TestCaseUniqueID,
				TestClass = @class,
				TestClassUniqueID = TestClassUniqueID,
				TestClassWithNamespace = TestClass,
				TestCollectionUniqueID = TestCollectionUniqueID,
				TestMethod = TestMethod,
				TestMethodUniqueID = TestMethodUniqueID,
				TestNamespace = @namespace,
				Traits = Traits
			};

			if (includeSerialization)
				result.Serialization = SerializationHelper.Serialize(this);

			return result;
		}
	}
}
