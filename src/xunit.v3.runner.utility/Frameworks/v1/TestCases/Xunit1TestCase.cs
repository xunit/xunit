#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v1
{
	/// <summary>
	/// Implementation of <see cref="_ITestCase"/> for xUnit.net v1 test cases.
	/// </summary>
	public class Xunit1TestCase : _ITestCase, IXunitSerializable
	{
		static readonly Dictionary<string, List<string>> EmptyTraits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

		Xunit1TestMethod? testMethod;

		/// <summary/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
		public Xunit1TestCase()
		{
			DisplayName = "<unset>";
			Traits = new Dictionary<string, List<string>>();
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

		/// <inheritdoc/>
		public void Deserialize(IXunitSerializationInfo data)
		{
			Guard.ArgumentNotNull(nameof(data), data);

			var testAssembly = new Xunit1TestAssembly(
				data.GetValue<string>("AssemblyFileName"),
				data.GetValue<string>("ConfigFileName")
			);
			var testCollection = new Xunit1TestCollection(testAssembly);
			var testClass = new Xunit1TestClass(testCollection, data.GetValue<string>("TypeName"));
			testMethod = new Xunit1TestMethod(testClass, data.GetValue<string>("MethodName"));

			DisplayName = data.GetValue<string>("DisplayName");
			SkipReason = data.GetValue<string>("SkipReason");
			SourceInformation = data.GetValue<_SourceInformation>("SourceInformation");

			Traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			var keys = data.GetValue<string[]>("Traits.Keys");
			foreach (var key in keys)
				Traits.Add(key, data.GetValue<string[]>($"Traits[{key}]").ToList());
		}

		/// <inheritdoc/>
		public void Serialize(IXunitSerializationInfo data)
		{
			Guard.ArgumentNotNull(nameof(data), data);

			data.AddValue("AssemblyFileName", TestMethod.TestClass.TestCollection.TestAssembly.Assembly.AssemblyPath);
			data.AddValue("ConfigFileName", TestMethod.TestClass.TestCollection.TestAssembly.ConfigFileName);
			data.AddValue("MethodName", TestMethod.Method.Name);
			data.AddValue("TypeName", TestMethod.TestClass.Class.Name);
			data.AddValue("DisplayName", DisplayName);
			data.AddValue("SkipReason", SkipReason);
			data.AddValue("SourceInformation", SourceInformation);

			if (Traits == null)
			{
				data.AddValue("Traits.Keys", new string[0]);
			}
			else
			{
				data.AddValue("Traits.Keys", Traits.Keys.ToArray());
				foreach (var key in Traits.Keys)
					data.AddValue($"Traits[{key}]", Traits[key].ToArray());
			}
		}
	}
}

#endif
