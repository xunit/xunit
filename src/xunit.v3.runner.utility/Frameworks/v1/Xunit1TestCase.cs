using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.v2;

namespace Xunit
{
	/// <summary>
	/// An implementation of <see cref="ITestCase"/> that adapts xUnit.net v1's XML-based APIs
	/// into xUnit.net v2's object-based APIs.
	/// </summary>
	public class Xunit1TestCase : ITestAssembly, ITestCollection, ITestClass, ITestMethod, ITestCase, IXunitSerializable
	{
		static readonly Dictionary<string, List<string>> EmptyTraits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

		Xunit1ReflectionWrapper? reflectionWrapper;

		/// <summary/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
		public Xunit1TestCase()
		{ }

		/// <summary>
		/// Initializes a new instance  of the <see cref="Xunit1TestCase"/> class.
		/// </summary>
		/// <param name="assemblyFileName">The assembly under test.</param>
		/// <param name="configFileName">The configuration file name.</param>
		/// <param name="typeName">The type under test.</param>
		/// <param name="methodName">The method under test.</param>
		/// <param name="displayName">The display name of the unit test.</param>
		/// <param name="traits">The traits of the unit test.</param>
		/// <param name="skipReason">The skip reason, if the test is skipped.</param>
		public Xunit1TestCase(
			string assemblyFileName,
			string? configFileName,
			string typeName,
			string methodName,
			string? displayName,
			Dictionary<string, List<string>>? traits = null,
			string? skipReason = null)
		{
			Guard.ArgumentNotNullOrEmpty(nameof(assemblyFileName), assemblyFileName);
			Guard.ArgumentNotNullOrEmpty(nameof(typeName), typeName);
			Guard.ArgumentNotNullOrEmpty(nameof(methodName), methodName);

			reflectionWrapper = new Xunit1ReflectionWrapper(assemblyFileName, typeName, methodName);

			ConfigFileName = configFileName;
			DisplayName = displayName;
			Traits = traits ?? EmptyTraits;
			SkipReason = skipReason;
		}

		/// <inheritdoc/>
		public string? ConfigFileName { get; set; }

		/// <inheritdoc/>
		public string? DisplayName { get; set; }

		Xunit1ReflectionWrapper ReflectionWrapper => Guard.NotNull($"Attempted to get ReflectionWrapper on an uninitialized '{GetType().FullName}' object", reflectionWrapper);

		/// <inheritdoc/>
		public string? SkipReason { get; set; }

		/// <inheritdoc/>
		public ISourceInformation? SourceInformation { get; set; }

		/// <inheritdoc/>
		public ITestMethod TestMethod => this;

		/// <inheritdoc/>
		public object[]? TestMethodArguments { get; set; }

		/// <inheritdoc/>
		public Dictionary<string, List<string>>? Traits { get; set; }

		/// <inheritdoc/>
		public string UniqueID => ReflectionWrapper.UniqueID;

		/// <inheritdoc/>
		public void Dispose()
		{ }

		/// <inheritdoc/>
		public void Deserialize(IXunitSerializationInfo data)
		{
			Guard.ArgumentNotNull(nameof(data), data);

			reflectionWrapper = new Xunit1ReflectionWrapper(
				data.GetValue<string>("AssemblyFileName"),
				data.GetValue<string>("TypeName"),
				data.GetValue<string>("MethodName")
			);

			ConfigFileName = data.GetValue<string>("ConfigFileName");
			DisplayName = data.GetValue<string>("DisplayName");
			SkipReason = data.GetValue<string>("SkipReason");
			SourceInformation = data.GetValue<SourceInformation>("SourceInformation");

			Traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			var keys = data.GetValue<string[]>("Traits.Keys");
			foreach (var key in keys)
				Traits.Add(key, data.GetValue<string[]>($"Traits[{key}]").ToList());
		}

		/// <inheritdoc/>
		public void Serialize(IXunitSerializationInfo data)
		{
			Guard.ArgumentNotNull(nameof(data), data);

			data.AddValue("AssemblyFileName", ReflectionWrapper.AssemblyFileName);
			data.AddValue("ConfigFileName", ConfigFileName);
			data.AddValue("MethodName", ReflectionWrapper.MethodName);
			data.AddValue("TypeName", ReflectionWrapper.TypeName);
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

		/// <inheritdoc/>
		IAssemblyInfo ITestAssembly.Assembly => ReflectionWrapper;

		/// <inheritdoc/>
		ITypeInfo? ITestCollection.CollectionDefinition => null;

		/// <inheritdoc/>
		string ITestCollection.DisplayName => $"xUnit.net v1 Tests for {ReflectionWrapper.AssemblyFileName}";

		/// <inheritdoc/>
		ITestAssembly ITestCollection.TestAssembly => this;

		/// <inheritdoc/>
		Guid ITestCollection.UniqueID => Guid.Empty;

		/// <inheritdoc/>
		ITypeInfo ITestClass.Class => ReflectionWrapper;

		/// <inheritdoc/>
		ITestCollection ITestClass.TestCollection => this;

		/// <inheritdoc/>
		IMethodInfo ITestMethod.Method => ReflectionWrapper;

		/// <inheritdoc/>
		ITestClass ITestMethod.TestClass => this;
	}
}
