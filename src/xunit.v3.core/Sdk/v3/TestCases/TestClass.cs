using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// The default implementation of <see cref="_ITestClass"/>.
	/// </summary>
	[Serializable]
	[DebuggerDisplay(@"\{ class = {Class.Name} \}")]
	public class TestClass : _ITestClass, ISerializable
	{
		_ITypeInfo @class;
		_ITestCollection testCollection;
		string uniqueID;

		/// <summary>
		/// Used for de-serialization.
		/// </summary>
		protected TestClass(
			SerializationInfo info,
			StreamingContext context)
		{
			testCollection = Guard.NotNull("Could not retrieve TestCollection from serialization", info.GetValue<_ITestCollection>("TestCollection"));
			uniqueID = Guard.NotNull("Could not retrieve UniqueID from serialization", info.GetValue<string>("UniqueID"));

			var assemblyName = Guard.NotNull("Could not retrieve ClassAssemblyName from serialization", info.GetValue<string>("ClassAssemblyName"));
			var typeName = Guard.NotNull("Could not retrieve ClassTypeName from serialization", info.GetValue<string>("ClassTypeName"));

			var type = SerializationHelper.GetType(assemblyName, typeName);
			if (type == null)
				throw new InvalidOperationException($"Failed to deserialize type '{typeName}' in assembly '{assemblyName}'");

			@class = Reflector.Wrap(type);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TestClass"/> class.
		/// </summary>
		/// <param name="testCollection">The test collection the class belongs to</param>
		/// <param name="class">The test class</param>
		/// <param name="uniqueID">The unique ID for the test class (only used to override default behavior in testing scenarios)</param>
		public TestClass(
			_ITestCollection testCollection,
			_ITypeInfo @class,
			string? uniqueID = null)
		{
			this.@class = Guard.ArgumentNotNull(@class);
			this.testCollection = Guard.ArgumentNotNull(testCollection);
			this.uniqueID = uniqueID ?? UniqueIDGenerator.ForTestClass(TestCollection.UniqueID, Class.Name);
		}

		/// <inheritdoc/>
		public _ITypeInfo Class
		{
			get => @class;
			set => @class = Guard.ArgumentNotNull(value, nameof(Class));
		}

		/// <inheritdoc/>
		public _ITestCollection TestCollection
		{
			get => testCollection;
			set => testCollection = Guard.ArgumentNotNull(value, nameof(TestCollection));
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
			info.AddValue("TestCollection", TestCollection);
			info.AddValue("ClassAssemblyName", Class.Assembly.Name);
			info.AddValue("ClassTypeName", Class.Name);
			info.AddValue("UniqueID", UniqueID);
		}
	}
}
