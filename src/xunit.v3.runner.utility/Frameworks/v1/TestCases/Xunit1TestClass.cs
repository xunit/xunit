using System.Collections.Generic;
using System.Linq;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v1
{
	/// <summary>
	/// Implementation of <see cref="_ITestClass"/> and <see cref="_ITypeInfo"/> for
	/// xUnit.net v1 test classes.
	/// </summary>
	public class Xunit1TestClass : _ITestClass, _ITypeInfo
	{
		readonly string typeName;

		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit1TestClass"/> class.
		/// </summary>
		/// <param name="testCollection">The test collection this test class belongs to.</param>
		/// <param name="typeName">The type that defines this test class.</param>
		public Xunit1TestClass(
			Xunit1TestCollection testCollection,
			string typeName)
		{
			this.typeName = Guard.ArgumentNotNull(nameof(typeName), typeName);

			TestCollection = Guard.ArgumentNotNull(nameof(testCollection), testCollection);
			UniqueID = UniqueIDGenerator.ForTestClass(TestCollection.UniqueID, Class.Name);
		}

		/// <inheritdoc/>
		public _ITypeInfo Class => this;

		/// <inheritdoc/>
		public Xunit1TestCollection TestCollection { get; }

		/// <inheritdoc/>
		public string UniqueID { get; }

		_ITestCollection _ITestClass.TestCollection => TestCollection;

		// _ITypeInfo explicit implementation

		_IAssemblyInfo _ITypeInfo.Assembly => TestCollection.TestAssembly;

		_ITypeInfo? _ITypeInfo.BaseType => null;

		IEnumerable<_ITypeInfo> _ITypeInfo.Interfaces => Enumerable.Empty<_ITypeInfo>();

		bool _ITypeInfo.IsAbstract => false;

		bool _ITypeInfo.IsGenericParameter => false;

		bool _ITypeInfo.IsGenericType => false;

		bool _ITypeInfo.IsSealed => false;

		bool _ITypeInfo.IsValueType => false;

		string _ITypeInfo.Name => typeName;

		IEnumerable<_IAttributeInfo> _ITypeInfo.GetCustomAttributes(string? assemblyQualifiedAttributeTypeName) => Enumerable.Empty<_IAttributeInfo>();

		IEnumerable<_ITypeInfo> _ITypeInfo.GetGenericArguments() => Enumerable.Empty<_ITypeInfo>();

		_IMethodInfo? _ITypeInfo.GetMethod(string? methodName, bool includePrivateMethods) => null;

		IEnumerable<_IMethodInfo> _ITypeInfo.GetMethods(bool includePrivateMethods) => Enumerable.Empty<_IMethodInfo>();
	}
}
