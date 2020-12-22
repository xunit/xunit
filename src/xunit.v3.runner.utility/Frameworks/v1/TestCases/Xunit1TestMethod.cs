using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v1
{
	/// <summary>
	/// Implementation of <see cref="_ITestMethod"/> and <see cref="IMethodInfo"/> for
	/// xUnit.net v1 test methods.
	/// </summary>
	public class Xunit1TestMethod : _ITestMethod, IMethodInfo
	{
		static readonly ITypeInfo VoidType = Reflector.Wrap(typeof(void));

		readonly string methodName;

		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit1TestMethod"/> class.
		/// </summary>
		/// <param name="testClass">The test class that this test method belongs to.</param>
		/// <param name="methodName">The test method name.</param>
		public Xunit1TestMethod(
			Xunit1TestClass testClass,
			string methodName)
		{
			this.methodName = Guard.ArgumentNotNull(nameof(methodName), methodName);

			TestClass = Guard.ArgumentNotNull(nameof(testClass), testClass);
			UniqueID = UniqueIDGenerator.ForTestMethod(TestClass.UniqueID, methodName);
		}

		/// <inheritdoc/>
		public IMethodInfo Method => this;

		/// <inheritdoc/>
		public Xunit1TestClass TestClass { get; }

		/// <inheritdoc/>
		public string UniqueID { get; }

		_ITestClass _ITestMethod.TestClass => TestClass;

		// IMethodInfo explicit implementation

		bool IMethodInfo.IsAbstract => false;

		bool IMethodInfo.IsGenericMethodDefinition => false;

		bool IMethodInfo.IsPublic => true;

		bool IMethodInfo.IsStatic => false;

		string? IMethodInfo.Name => methodName;

		ITypeInfo IMethodInfo.ReturnType => VoidType;

		ITypeInfo IMethodInfo.Type => TestClass;

		IEnumerable<IAttributeInfo> IMethodInfo.GetCustomAttributes(string? assemblyQualifiedAttributeTypeName) => Enumerable.Empty<IAttributeInfo>();

		IEnumerable<ITypeInfo> IMethodInfo.GetGenericArguments() => Enumerable.Empty<ITypeInfo>();

		IEnumerable<IParameterInfo> IMethodInfo.GetParameters() => Enumerable.Empty<IParameterInfo>();

		IMethodInfo IMethodInfo.MakeGenericMethod(params ITypeInfo[] typeArguments) => throw new NotImplementedException();
	}
}
