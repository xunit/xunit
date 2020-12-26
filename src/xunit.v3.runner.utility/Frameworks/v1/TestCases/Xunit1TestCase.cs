#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v1
{
	/// <summary>
	/// Implementation of <see cref="_ITestCase"/> for xUnit.net v1 test cases.
	/// </summary>
	[Serializable]
	public class Xunit1TestCase : _ITestCase, ISerializable
	{
		static readonly Dictionary<string, List<string>> EmptyTraits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

		Xunit1TestMethod? testMethod;

		/// <summary>
		/// Used for de-serialization.
		/// </summary>
		protected Xunit1TestCase(
			SerializationInfo info,
			StreamingContext context)
		{
			var testAssembly = new Xunit1TestAssembly(
				Guard.NotNull("Could not retrieve AssemblyFileName from serialization", info.GetValue<string>("AssemblyFileName")),
				info.GetValue<string>("ConfigFileName")
			);
			var testCollection = new Xunit1TestCollection(testAssembly);
			var testClass = new Xunit1TestClass(testCollection, Guard.NotNull("Could not retrieve TypeName from serialization", info.GetValue<string>("TypeName")));
			testMethod = new Xunit1TestMethod(testClass, Guard.NotNull("Could not retrieve MethodName from serialization", info.GetValue<string>("MethodName")));

			DisplayName = Guard.NotNull("Could not retrieve DisplayName from serialization", info.GetValue<string>("DisplayName"));
			SkipReason = info.GetValue<string>("SkipReason");
			SourceInformation = info.GetValue<_SourceInformation>("SourceInformation");
			Traits = SerializationHelper.DeserializeTraits(info);
		}

		/// <summary>
		/// Initializes a new instance  of the <see cref="Xunit1TestCase"/> class.
		/// </summary>
		/// <param name="testMethod">The test method this test case belongs to.</param>
		/// <param name="displayName">The display name of the unit test.</param>
		/// <param name="traits">The traits of the unit test.</param>
		/// <param name="skipReason">The skip reason, if the test is skipped.</param>
		public Xunit1TestCase(
			Xunit1TestMethod testMethod,
			string? displayName,
			Dictionary<string, List<string>>? traits = null,
			string? skipReason = null)
		{
			this.testMethod = Guard.ArgumentNotNull(nameof(testMethod), testMethod);

			var typeName = testMethod.TestClass.Class.Name;
			var methodName = testMethod.Method.Name;

			DisplayName = displayName ?? $"{typeName}.{methodName}";
			Traits = traits ?? EmptyTraits;
			SkipReason = skipReason;
		}

		/// <inheritdoc/>
		public string DisplayName { get; set; }

		/// <inheritdoc/>
		public string? SkipReason { get; set; }

		/// <inheritdoc/>
		public _ISourceInformation? SourceInformation { get; set; }

		/// <inheritdoc/>
		public _ITestMethod TestMethod => testMethod ?? throw new InvalidOperationException($"Attempted to get {nameof(TestMethod)} on an uninitialized '{GetType().FullName}' object");

		/// <inheritdoc/>
		public object?[]? TestMethodArguments { get; set; }

		/// <inheritdoc/>
		public Dictionary<string, List<string>> Traits { get; set; }

		/// <inheritdoc/>
		// TODO: Should get updated to UniqueIDGenerator once it can generate test case unique IDs
		public string UniqueID => $"{TestMethod.TestClass.Class.Name}.{TestMethod.Method.Name} ({TestMethod.TestClass.TestCollection.TestAssembly.Assembly.AssemblyPath})";

		void ISerializable.GetObjectData(
			SerializationInfo info,
			StreamingContext context)
		{
			info.AddValue("AssemblyFileName", TestMethod.TestClass.TestCollection.TestAssembly.Assembly.AssemblyPath);
			info.AddValue("ConfigFileName", TestMethod.TestClass.TestCollection.TestAssembly.ConfigFileName);
			info.AddValue("MethodName", TestMethod.Method.Name);
			info.AddValue("TypeName", TestMethod.TestClass.Class.Name);
			info.AddValue("DisplayName", DisplayName);
			info.AddValue("SkipReason", SkipReason);
			info.AddValue("SourceInformation", SourceInformation);
			SerializationHelper.SerializeTraits(info, Traits);
		}
	}
}

#endif
