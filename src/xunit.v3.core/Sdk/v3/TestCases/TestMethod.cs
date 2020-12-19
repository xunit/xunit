using System;
using System.ComponentModel;
using System.Diagnostics;
using Xunit.Abstractions;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// The default implementation of <see cref="_ITestMethod"/>.
	/// </summary>
	[DebuggerDisplay(@"\{ class = {TestClass.Class.Name}, method = {Method.Name} \}")]
	public class TestMethod : _ITestMethod, IXunitSerializable
	{
		IMethodInfo? method;
		_ITestClass? testClass;
		string? uniqueID;

		/// <summary/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
		public TestMethod()
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="TestMethod"/> class.
		/// </summary>
		/// <param name="testClass">The test class</param>
		/// <param name="method">The test method</param>
		public TestMethod(
			_ITestClass testClass,
			IMethodInfo method)
		{
			this.testClass = Guard.ArgumentNotNull(nameof(testClass), testClass);
			this.method = Guard.ArgumentNotNull(nameof(method), method);

			uniqueID = UniqueIDGenerator.ForTestMethod(testClass.UniqueID, this.method.Name);
		}

		/// <inheritdoc/>
		public IMethodInfo Method
		{
			get => method ?? throw new InvalidOperationException($"Attempted to get Method on an uninitialized '{GetType().FullName}' object");
			set => method = Guard.ArgumentNotNull(nameof(Method), value);
		}

		/// <inheritdoc/>
		public _ITestClass TestClass
		{
			get => testClass ?? throw new InvalidOperationException($"Attempted to get TestClass on an uninitialized '{GetType().FullName}' object");
			set => testClass = Guard.ArgumentNotNull(nameof(TestClass), value);
		}

		/// <inheritdoc/>
		public string UniqueID
		{
			get => uniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(UniqueID)} on an uninitialized '{GetType().FullName}' object");
			set => uniqueID = Guard.ArgumentNotNull(nameof(UniqueID), value);
		}

		/// <inheritdoc/>
		public void Serialize(IXunitSerializationInfo info)
		{
			Guard.ArgumentNotNull(nameof(info), info);

			info.AddValue("MethodName", Method.Name);
			info.AddValue("TestClass", TestClass);
			info.AddValue("UniqueID", UniqueID);
		}

		/// <inheritdoc/>
		public void Deserialize(IXunitSerializationInfo info)
		{
			Guard.ArgumentNotNull(nameof(info), info);

			testClass = info.GetValue<_ITestClass>("TestClass");
			uniqueID = info.GetValue<string>("UniqueID");

			var methodName = info.GetValue<string>("MethodName");
			method = TestClass.Class.GetMethod(methodName, true);
		}
	}
}
