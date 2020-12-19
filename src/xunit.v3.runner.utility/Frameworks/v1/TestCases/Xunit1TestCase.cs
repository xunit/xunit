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
	/// An implementation of <see cref="_ITestCase"/> (and parents) that adapts xUnit.net v1's XML-based APIs
	/// into xUnit.net v3's object-based APIs.
	/// </summary>
	public class Xunit1TestCase : _ITestMethod, _ITestCase, IXunitSerializable
	{
		static readonly Dictionary<string, List<string>> EmptyTraits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

		Xunit1ReflectionWrapper? reflectionWrapper;
		Xunit1TestClass? testClass;

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
		/// <param name="testClass">The test class this test case belongs to.</param>
		/// <param name="methodName">The method under test.</param>
		/// <param name="displayName">The display name of the unit test.</param>
		/// <param name="traits">The traits of the unit test.</param>
		/// <param name="skipReason">The skip reason, if the test is skipped.</param>
		public Xunit1TestCase(
			Xunit1TestClass testClass,
			string methodName,
			string? displayName,
			Dictionary<string, List<string>>? traits = null,
			string? skipReason = null)
		{
			Guard.ArgumentNotNullOrEmpty(nameof(methodName), methodName);

			this.testClass = Guard.ArgumentNotNull(nameof(testClass), testClass);

			var typeName = testClass.Class.Name;
			reflectionWrapper = new Xunit1ReflectionWrapper(testClass.TestCollection.TestAssembly.Assembly.AssemblyPath, typeName, methodName);

			DisplayName = displayName ?? $"{typeName}.{methodName}";
			Traits = traits ?? EmptyTraits;
			SkipReason = skipReason;
		}

		/// <inheritdoc/>
		public string DisplayName { get; set; }

		Xunit1ReflectionWrapper ReflectionWrapper => reflectionWrapper ?? throw new InvalidOperationException($"Attempted to get {nameof(ReflectionWrapper)} on an uninitialized '{GetType().FullName}' object");

		/// <inheritdoc/>
		public string? SkipReason { get; set; }

		/// <inheritdoc/>
		public _ISourceInformation? SourceInformation { get; set; }

		/// <inheritdoc/>
		public _ITestMethod TestMethod => this;

		/// <inheritdoc/>
		public object?[]? TestMethodArguments { get; set; }

		/// <inheritdoc/>
		public Dictionary<string, List<string>> Traits { get; set; }

		/// <inheritdoc/>
		public void Deserialize(IXunitSerializationInfo data)
		{
			Guard.ArgumentNotNull(nameof(data), data);

			var testAssembly = new Xunit1TestAssembly(
				data.GetValue<string>("AssemblyFileName"),
				data.GetValue<string>("ConfigFileName")
			);
			var testCollection = new Xunit1TestCollection(testAssembly);
			testClass = new Xunit1TestClass(testCollection, data.GetValue<string>("TypeName"));

			reflectionWrapper = new Xunit1ReflectionWrapper(
				testAssembly.Assembly.AssemblyPath,
				data.GetValue<string>("TypeName"),
				data.GetValue<string>("MethodName")
			);

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

			data.AddValue("AssemblyFileName", testClass!.TestCollection.TestAssembly.Assembly.AssemblyPath);
			data.AddValue("ConfigFileName", testClass!.TestCollection.TestAssembly.ConfigFileName);
			data.AddValue("MethodName", ReflectionWrapper.MethodName);
			data.AddValue("TypeName", testClass!.Class.Name);
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

		string _ITestCase.UniqueID => ReflectionWrapper.UniqueID;

		IMethodInfo _ITestMethod.Method => ReflectionWrapper;
		_ITestClass _ITestMethod.TestClass => testClass!;
	}
}

#endif
