using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v1
{
	/// <summary>
	/// Implementation of <see cref="_ITestClass"/> and <see cref="ITypeInfo"/> for
	/// xUnit.net v1 test classes.
	/// </summary>
	public class Xunit1TestClass : _ITestClass, ITypeInfo
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
		public ITypeInfo Class => this;

		/// <inheritdoc/>
		public Xunit1TestCollection TestCollection { get; }

		/// <inheritdoc/>
		public string UniqueID { get; }

		_ITestCollection _ITestClass.TestCollection => TestCollection;

		// ITypeInfo explicit implementation

		IAssemblyInfo ITypeInfo.Assembly => TestCollection.TestAssembly;

		ITypeInfo? ITypeInfo.BaseType => null;

		IEnumerable<ITypeInfo> ITypeInfo.Interfaces => Enumerable.Empty<ITypeInfo>();

		bool ITypeInfo.IsAbstract => false;

		bool ITypeInfo.IsGenericParameter => false;

		bool ITypeInfo.IsGenericType => false;

		bool ITypeInfo.IsSealed => false;

		bool ITypeInfo.IsValueType => false;

		string ITypeInfo.Name => typeName;

		IEnumerable<IAttributeInfo> ITypeInfo.GetCustomAttributes(string? assemblyQualifiedAttributeTypeName) => Enumerable.Empty<IAttributeInfo>();

		IEnumerable<ITypeInfo> ITypeInfo.GetGenericArguments() => Enumerable.Empty<ITypeInfo>();

		IMethodInfo ITypeInfo.GetMethod(string? methodName, bool includePrivateMethods) => null!;

		IEnumerable<IMethodInfo> ITypeInfo.GetMethods(bool includePrivateMethods) => Enumerable.Empty<IMethodInfo>();
	}
}
