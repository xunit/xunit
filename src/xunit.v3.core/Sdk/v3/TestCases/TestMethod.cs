using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// The default implementation of <see cref="_ITestMethod"/>.
	/// </summary>
	[Serializable]
	[DebuggerDisplay(@"\{ class = {TestClass.Class.Name}, method = {Method.Name} \}")]
	public class TestMethod : _ITestMethod, ISerializable
	{
		_IMethodInfo method;
		_ITestClass testClass;
		string uniqueID;

		/// <summary>
		/// Used for de-serialization.
		/// </summary>
		protected TestMethod(
			SerializationInfo info,
			StreamingContext context)
		{
			testClass = Guard.NotNull("Could not retrieve TestClass from serialization", info.GetValue<_ITestClass>("TestClass"));
			uniqueID = Guard.NotNull("Could not retrieve UniqueID from serialization", info.GetValue<string>("UniqueID"));

			var methodName = Guard.NotNull("Could not retrieve MethodName from serialization", info.GetValue<string>("MethodName"));
			method = Guard.NotNull($"Could not find test method {methodName} on test class {testClass.Class.Name}", TestClass.Class.GetMethod(methodName, true));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TestMethod"/> class.
		/// </summary>
		/// <param name="testClass">The test class</param>
		/// <param name="method">The test method</param>
		/// <param name="uniqueID">The unique ID for the test method (only used to override default behavior in testing scenarios)</param>
		public TestMethod(
			_ITestClass testClass,
			_IMethodInfo method,
			string? uniqueID = null)
		{
			this.testClass = Guard.ArgumentNotNull(testClass);
			this.method = Guard.ArgumentNotNull(method);

			this.uniqueID = uniqueID ?? UniqueIDGenerator.ForTestMethod(testClass.UniqueID, this.method.Name);
		}

		/// <inheritdoc/>
		public _IMethodInfo Method
		{
			get => method;
			set => method = Guard.ArgumentNotNull(value, nameof(Method));
		}

		/// <inheritdoc/>
		public _ITestClass TestClass
		{
			get => testClass;
			set => testClass = Guard.ArgumentNotNull(value, nameof(TestClass));
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
			info.AddValue("MethodName", Method.Name);
			info.AddValue("TestClass", TestClass);
			info.AddValue("UniqueID", UniqueID);
		}
	}
}
