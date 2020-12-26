using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v1
{
	/// <summary>
	/// Implementation of <see cref="_ITestMethod"/> and <see cref="_IMethodInfo"/> for
	/// xUnit.net v1 test methods.
	/// </summary>
	public class Xunit3TestMethod : _ITestMethod, _IMethodInfo
	{
		static readonly _ITypeInfo VoidType = Reflector.Wrap(typeof(void));

		readonly string methodName;

		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit3TestMethod"/> class.
		/// </summary>
		/// <param name="testClass">The test class that this test method belongs to.</param>
		/// <param name="methodName">The test method name.</param>
		public Xunit3TestMethod(
			Xunit3TestClass testClass,
			string methodName)
		{
			this.methodName = Guard.ArgumentNotNull(nameof(methodName), methodName);

			TestClass = Guard.ArgumentNotNull(nameof(testClass), testClass);
			UniqueID = UniqueIDGenerator.ForTestMethod(TestClass.UniqueID, methodName);
		}

		/// <inheritdoc/>
		public _IMethodInfo Method => this;

		/// <inheritdoc/>
		public Xunit3TestClass TestClass { get; }

		/// <inheritdoc/>
		public string UniqueID { get; }

		_ITestClass _ITestMethod.TestClass => TestClass;

		// _IMethodInfo explicit implementation

		bool _IMethodInfo.IsAbstract => false;

		bool _IMethodInfo.IsGenericMethodDefinition => false;

		bool _IMethodInfo.IsPublic => true;

		bool _IMethodInfo.IsStatic => false;

		string _IMethodInfo.Name => methodName;

		_ITypeInfo _IMethodInfo.ReturnType => VoidType;

		_ITypeInfo _IMethodInfo.Type => TestClass;

		IEnumerable<_IAttributeInfo> _IMethodInfo.GetCustomAttributes(string? assemblyQualifiedAttributeTypeName) => Enumerable.Empty<_IAttributeInfo>();

		IEnumerable<_ITypeInfo> _IMethodInfo.GetGenericArguments() => Enumerable.Empty<_ITypeInfo>();

		IEnumerable<_IParameterInfo> _IMethodInfo.GetParameters() => Enumerable.Empty<_IParameterInfo>();

		_IMethodInfo _IMethodInfo.MakeGenericMethod(params _ITypeInfo[] typeArguments) => throw new NotImplementedException();
	}
}
