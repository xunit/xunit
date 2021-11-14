using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// The default implementation of <see cref="_ITestCollection"/>.
	/// </summary>
	[Serializable]
	[DebuggerDisplay(@"\{ id = {UniqueID}, display = {DisplayName} \}")]
	public class TestCollection : _ITestCollection, ISerializable
	{
		string displayName;
		_ITestAssembly testAssembly;
		string uniqueID;

		/// <summary>
		/// Used for de-serialization.
		/// </summary>
		protected TestCollection(
			SerializationInfo info,
			StreamingContext context)
		{
			displayName = Guard.NotNull("Could not retrieve DisplayName from serialization", info.GetValue<string>("DisplayName"));
			testAssembly = Guard.NotNull("Could not retrieve TestAssembly from serialization", info.GetValue<_ITestAssembly>("TestAssembly"));
			uniqueID = Guard.NotNull("Could not retrieve UniqueID from serialization", info.GetValue<string>("UniqueID"));

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

			this.displayName = Guard.ArgumentNotNull(displayName);
			this.testAssembly = Guard.ArgumentNotNull(testAssembly);
			this.uniqueID = uniqueID ?? UniqueIDGenerator.ForTestCollection(testAssembly.UniqueID, this.displayName, CollectionDefinition?.Name);
		}

		/// <inheritdoc/>
		public _ITypeInfo? CollectionDefinition { get; set; }

		/// <inheritdoc/>
		public string DisplayName
		{
			get => displayName;
			set => displayName = Guard.ArgumentNotNull(value, nameof(DisplayName));
		}

		/// <inheritdoc/>
		public _ITestAssembly TestAssembly
		{
			get => testAssembly;
			set => testAssembly = Guard.ArgumentNotNull(value, nameof(TestAssembly));
		}

		/// <inheritdoc/>
		public string UniqueID
		{
			get => uniqueID;
			set => uniqueID = Guard.ArgumentNotNull(value, nameof(UniqueID));
		}

		/// <inheritdoc/>
		public virtual void GetObjectData(
			SerializationInfo info,
			StreamingContext context)
		{
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
	}
}
