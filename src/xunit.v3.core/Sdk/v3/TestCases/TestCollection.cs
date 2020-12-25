using System;
using System.ComponentModel;
using System.Diagnostics;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.v2;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// The default implementation of <see cref="_ITestCollection"/>.
	/// </summary>
	[DebuggerDisplay(@"\{ id = {UniqueID}, display = {DisplayName} \}")]
	public class TestCollection : _ITestCollection, IXunitSerializable
	{
		string? displayName;
		_ITestAssembly? testAssembly;
		string? uniqueID;

		/// <summary/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
		public TestCollection()
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="TestCollection"/> class.
		/// </summary>
		/// <param name="testAssembly">The test assembly the collection belongs to</param>
		/// <param name="collectionDefinition">The optional type which contains the collection definition</param>
		/// <param name="displayName">The display name for the test collection</param>
		/// <param name="uniqueID">The unique ID for the test collection (only used to override default behavior in testing scenarios)</param>
		public TestCollection(
			_ITestAssembly testAssembly,
			_ITypeInfo? collectionDefinition,
			string displayName,
			string? uniqueID = null)
		{
			CollectionDefinition = collectionDefinition;
			this.displayName = Guard.ArgumentNotNull(nameof(displayName), displayName);
			this.testAssembly = Guard.ArgumentNotNull(nameof(testAssembly), testAssembly);
			this.uniqueID = uniqueID ?? UniqueIDGenerator.ForTestCollection(testAssembly.UniqueID, this.displayName, CollectionDefinition?.Name);
		}

		/// <inheritdoc/>
		public _ITypeInfo? CollectionDefinition { get; set; }

		/// <inheritdoc/>
		public string DisplayName
		{
			get => displayName ?? throw new InvalidOperationException($"Attempted to get {nameof(DisplayName)} on an uninitialized '{GetType().FullName}' object");
			set => displayName = Guard.ArgumentNotNull(nameof(DisplayName), value);
		}

		/// <inheritdoc/>
		public _ITestAssembly TestAssembly
		{
			get => testAssembly ?? throw new InvalidOperationException($"Attempted to get {nameof(TestAssembly)} on an uninitialized '{GetType().FullName}' object");
			set => testAssembly = Guard.ArgumentNotNull(nameof(TestAssembly), value);
		}

		/// <inheritdoc/>
		public string UniqueID
		{
			get => uniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(UniqueID)} on an uninitialized '{GetType().FullName}' object");
			set => uniqueID = Guard.ArgumentNotNull(nameof(UniqueID), value);
		}

		/// <inheritdoc/>
		public virtual void Serialize(IXunitSerializationInfo info)
		{
			Guard.ArgumentNotNull(nameof(info), info);

			info.AddValue("DisplayName", DisplayName);
			info.AddValue("TestAssembly", TestAssembly);
			info.AddValue("UniqueID", UniqueID);

			if (CollectionDefinition != null)
			{
				info.AddValue("DeclarationAssemblyName", CollectionDefinition.Assembly.Name);
				info.AddValue("DeclarationTypeName", CollectionDefinition.Name);
			}
			else
			{
				info.AddValue("DeclarationAssemblyName", null);
				info.AddValue("DeclarationTypeName", null);
			}
		}

		/// <inheritdoc/>
		public virtual void Deserialize(IXunitSerializationInfo info)
		{
			Guard.ArgumentNotNull(nameof(info), info);

			DisplayName = info.GetValue<string>("DisplayName");
			TestAssembly = info.GetValue<_ITestAssembly>("TestAssembly");
			UniqueID = info.GetValue<string>("UniqueID");

			var assemblyName = info.GetValue<string>("DeclarationAssemblyName");
			var typeName = info.GetValue<string>("DeclarationTypeName");

			if (!string.IsNullOrWhiteSpace(assemblyName) && !string.IsNullOrWhiteSpace(typeName))
			{
				var type = SerializationHelper.GetType(assemblyName, typeName);
				if (type == null)
					throw new InvalidOperationException($"Failed to deserialize type '{typeName}' in assembly '{assemblyName}'");

				CollectionDefinition = Reflector.Wrap(type);
			}
		}
	}
}
