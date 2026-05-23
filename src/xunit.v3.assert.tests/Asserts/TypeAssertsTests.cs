#pragma warning disable CA2263  // Prefer generic overload when type is known

using Xunit;
using Xunit.Sdk;

#if NETFRAMEWORK
using System.Reflection;
using System.Xml;
#endif

public static class TypeAssertsTests
{

#pragma warning disable xUnit2032 // Type assertions based on 'assignable from' are confusingly named

	public static class IsAssignableFrom_Generic
	{
		[Fact]
		public static void NullObject()
		{
			var result = Record.Exception(() => Assert.IsAssignableFrom<object>(null));

			Assert.IsType<IsAssignableFromException>(result);
			Assert.Equal(
				"Assert.IsAssignableFrom() Failure: Value is null" + Environment.NewLine +
				"Expected: typeof(object)" + Environment.NewLine +
				"Actual:   null",
				result.Message
			);
		}

		[Fact]
		public static void SameType()
		{
			var ex = new InvalidCastException();

			Assert.IsAssignableFrom<InvalidCastException>(ex);
		}

		[Fact]
		public static void BaseType()
		{
			var ex = new InvalidCastException();

			Assert.IsAssignableFrom<Exception>(ex);
		}

		[Fact]
		public static void Interface()
		{
			var ex = new DisposableClass();

			Assert.IsAssignableFrom<IDisposable>(ex);
		}

		[Fact]
		public static void ReturnsCastObject()
		{
			var ex = new InvalidCastException();

			var result = Assert.IsAssignableFrom<InvalidCastException>(ex);

			Assert.Same(ex, result);
		}

		[Fact]
		public static void IncompatibleType()
		{
			var result =
				Record.Exception(
					() => Assert.IsAssignableFrom<InvalidCastException>(new InvalidOperationException())
				);

			Assert.IsType<IsAssignableFromException>(result);
			Assert.Equal(
				"Assert.IsAssignableFrom() Failure: Value is an incompatible type" + Environment.NewLine +
				"Expected: typeof(System.InvalidCastException)" + Environment.NewLine +
				"Actual:   typeof(System.InvalidOperationException)",
				result.Message
			);
		}
	}

#pragma warning disable xUnit2007 // Do not use typeof expression to check the type

	public static class IsAssignableFrom_NonGeneric
	{
		[Fact]
		public static void NullObject()
		{
			var result = Record.Exception(() => Assert.IsAssignableFrom(typeof(object), null));

			Assert.IsType<IsAssignableFromException>(result);
			Assert.Equal(
				"Assert.IsAssignableFrom() Failure: Value is null" + Environment.NewLine +
				"Expected: typeof(object)" + Environment.NewLine +
				"Actual:   null",
				result.Message
			);
		}

		[Fact]
		public static void SameType()
		{
			var ex = new InvalidCastException();

			Assert.IsAssignableFrom(typeof(InvalidCastException), ex);
		}

		[Fact]
		public static void BaseType()
		{
			var ex = new InvalidCastException();

			Assert.IsAssignableFrom(typeof(Exception), ex);
		}

		[Fact]
		public static void Interface()
		{
			var ex = new DisposableClass();

			Assert.IsAssignableFrom(typeof(IDisposable), ex);
		}

		[Fact]
		public static void ReturnsCastObject()
		{
			var ex = new InvalidCastException();

			var result = Assert.IsAssignableFrom<InvalidCastException>(ex);

			Assert.Same(ex, result);
		}

		[Fact]
		public static void IncompatibleType()
		{
			var result =
				Record.Exception(
					() => Assert.IsAssignableFrom(typeof(InvalidCastException), new InvalidOperationException())
				);

			Assert.IsType<IsAssignableFromException>(result);
			Assert.Equal(
				"Assert.IsAssignableFrom() Failure: Value is an incompatible type" + Environment.NewLine +
				"Expected: typeof(System.InvalidCastException)" + Environment.NewLine +
				"Actual:   typeof(System.InvalidOperationException)",
				result.Message
			);
		}
	}

#pragma warning restore xUnit2007 // Do not use typeof expression to check the type

	public static class IsNotAssignableFrom_Generic
	{
		[Fact]
		public static void NullObject()
		{
			Assert.IsNotAssignableFrom<object>(null);
		}

		[Fact]
		public static void SameType()
		{
			var ex = new InvalidCastException();

			var result = Record.Exception(() => Assert.IsNotAssignableFrom<InvalidCastException>(ex));

			Assert.IsType<IsNotAssignableFromException>(result);
			Assert.Equal(
				"Assert.IsNotAssignableFrom() Failure: Value is a compatible type" + Environment.NewLine +
				"Expected: typeof(System.InvalidCastException)" + Environment.NewLine +
				"Actual:   typeof(System.InvalidCastException)",
				result.Message
			);
		}

		[Fact]
		public static void BaseType()
		{
			var ex = new InvalidCastException();

			var result = Record.Exception(() => Assert.IsNotAssignableFrom<Exception>(ex));

			Assert.IsType<IsNotAssignableFromException>(result);
			Assert.Equal(
				"Assert.IsNotAssignableFrom() Failure: Value is a compatible type" + Environment.NewLine +
				"Expected: typeof(System.Exception)" + Environment.NewLine +
				"Actual:   typeof(System.InvalidCastException)",
				result.Message
			);
		}

		[Fact]
		public static void Interface()
		{
			var ex = new DisposableClass();

			var result = Record.Exception(() => Assert.IsNotAssignableFrom<IDisposable>(ex));

			Assert.IsType<IsNotAssignableFromException>(result);
			Assert.Equal(
				"Assert.IsNotAssignableFrom() Failure: Value is a compatible type" + Environment.NewLine +
				"Expected: typeof(System.IDisposable)" + Environment.NewLine +
				"Actual:   typeof(TypeAssertsTests+DisposableClass)",
				result.Message
			);
		}

		[Fact]
		public static void IncompatibleType()
		{
			Assert.IsNotAssignableFrom<InvalidCastException>(new InvalidOperationException());
		}
	}

	public static class IsNotAssignableFrom_NonGeneric
	{
		[Fact]
		public static void NullObject()
		{
			Assert.IsNotAssignableFrom(typeof(object), null);
		}

		[Fact]
		public static void SameType()
		{
			var ex = new InvalidCastException();

			var result = Record.Exception(() => Assert.IsNotAssignableFrom(typeof(InvalidCastException), ex));

			Assert.IsType<IsNotAssignableFromException>(result);
			Assert.Equal(
				"Assert.IsNotAssignableFrom() Failure: Value is a compatible type" + Environment.NewLine +
				"Expected: typeof(System.InvalidCastException)" + Environment.NewLine +
				"Actual:   typeof(System.InvalidCastException)",
				result.Message
			);
		}

		[Fact]
		public static void BaseType()
		{
			var ex = new InvalidCastException();

			var result = Record.Exception(() => Assert.IsNotAssignableFrom(typeof(Exception), ex));

			Assert.IsType<IsNotAssignableFromException>(result);
			Assert.Equal(
				"Assert.IsNotAssignableFrom() Failure: Value is a compatible type" + Environment.NewLine +
				"Expected: typeof(System.Exception)" + Environment.NewLine +
				"Actual:   typeof(System.InvalidCastException)",
				result.Message
			);
		}

		[Fact]
		public static void Interface()
		{
			var ex = new DisposableClass();

			var result = Record.Exception(() => Assert.IsNotAssignableFrom(typeof(IDisposable), ex));

			Assert.IsType<IsNotAssignableFromException>(result);
			Assert.Equal(
				"Assert.IsNotAssignableFrom() Failure: Value is a compatible type" + Environment.NewLine +
				"Expected: typeof(System.IDisposable)" + Environment.NewLine +
				"Actual:   typeof(TypeAssertsTests+DisposableClass)",
				result.Message
			);
		}

		[Fact]
		public static void IncompatibleType()
		{
			Assert.IsNotAssignableFrom(typeof(InvalidCastException), new InvalidOperationException());
		}
	}

#pragma warning restore xUnit2032 // Type assertions based on 'assignable from' are confusingly named

	public static class IsNotType_Generic
	{
		[Fact]
		public static void UnmatchedType()
		{
			var ex = new InvalidCastException();

			Assert.IsNotType<Exception>(ex);
		}

		[Fact]
		public static void MatchedType()
		{
			var result = Record.Exception(() => Assert.IsNotType<InvalidCastException>(new InvalidCastException()));

			Assert.IsType<IsNotTypeException>(result);
			Assert.Equal(
				"Assert.IsNotType() Failure: Value is the exact type" + Environment.NewLine +
				"Expected: typeof(System.InvalidCastException)" + Environment.NewLine +
				"Actual:   typeof(System.InvalidCastException)",
				result.Message
			);
		}

		[Fact]
		public static void NullObject()
		{
			Assert.IsNotType<object>(null);
		}
	}

	public static class IsNotType_Generic_InexactMatch
	{
		[Fact]
		public static void NullObject()
		{
			Assert.IsNotType<object>(null, exactMatch: false);
		}

		[Fact]
		public static void SameType()
		{
			var ex = new InvalidCastException();

			var result = Record.Exception(() => Assert.IsNotType<InvalidCastException>(ex, exactMatch: false));

			Assert.IsType<IsNotTypeException>(result);
			Assert.Equal(
				"Assert.IsNotType() Failure: Value is a compatible type" + Environment.NewLine +
				"Expected: typeof(System.InvalidCastException)" + Environment.NewLine +
				"Actual:   typeof(System.InvalidCastException)",
				result.Message
			);
		}

		[Fact]
		public static void BaseType()
		{
			var ex = new InvalidCastException();

			var result = Record.Exception(() => Assert.IsNotType<Exception>(ex, exactMatch: false));

			Assert.IsType<IsNotTypeException>(result);
			Assert.Equal(
				"Assert.IsNotType() Failure: Value is a compatible type" + Environment.NewLine +
				"Expected: typeof(System.Exception)" + Environment.NewLine +
				"Actual:   typeof(System.InvalidCastException)",
				result.Message
			);
		}

		[Fact]
		public static void Interface()
		{
			var ex = new DisposableClass();

			var result = Record.Exception(() => Assert.IsNotType<IDisposable>(ex, exactMatch: false));

			Assert.IsType<IsNotTypeException>(result);
			Assert.Equal(
				"Assert.IsNotType() Failure: Value is a compatible type" + Environment.NewLine +
				"Expected: typeof(System.IDisposable)" + Environment.NewLine +
				"Actual:   typeof(TypeAssertsTests+DisposableClass)",
				result.Message
			);
		}

		[Fact]
		public static void IncompatibleType()
		{
			Assert.IsNotType<InvalidCastException>(new InvalidOperationException(), exactMatch: false);
		}
	}

#pragma warning disable xUnit2007 // Do not use typeof expression to check the type

	public static class IsNotType_NonGeneric
	{
		[Fact]
		public static void UnmatchedType()
		{
			var ex = new InvalidCastException();

			Assert.IsNotType(typeof(Exception), ex);
		}

		[Fact]
		public static void MatchedType()
		{
			var result = Record.Exception(() => Assert.IsNotType(typeof(InvalidCastException), new InvalidCastException()));

			Assert.IsType<IsNotTypeException>(result);
			Assert.Equal(
				"Assert.IsNotType() Failure: Value is the exact type" + Environment.NewLine +
				"Expected: typeof(System.InvalidCastException)" + Environment.NewLine +
				"Actual:   typeof(System.InvalidCastException)",
				result.Message
			);
		}

		[Fact]
		public static void NullObject()
		{
			Assert.IsNotType(typeof(object), null);
		}
	}

	public static class IsNotType_NonGeneric_InexactMatch
	{
		[Fact]
		public static void NullObject()
		{
			Assert.IsNotType(typeof(object), null, exactMatch: false);
		}

		[Fact]
		public static void SameType()
		{
			var ex = new InvalidCastException();

			var result = Record.Exception(() => Assert.IsNotType(typeof(InvalidCastException), ex, exactMatch: false));

			Assert.IsType<IsNotTypeException>(result);
			Assert.Equal(
				"Assert.IsNotType() Failure: Value is a compatible type" + Environment.NewLine +
				"Expected: typeof(System.InvalidCastException)" + Environment.NewLine +
				"Actual:   typeof(System.InvalidCastException)",
				result.Message
			);
		}

		[Fact]
		public static void BaseType()
		{
			var ex = new InvalidCastException();

			var result = Record.Exception(() => Assert.IsNotType(typeof(Exception), ex, exactMatch: false));

			Assert.IsType<IsNotTypeException>(result);
			Assert.Equal(
				"Assert.IsNotType() Failure: Value is a compatible type" + Environment.NewLine +
				"Expected: typeof(System.Exception)" + Environment.NewLine +
				"Actual:   typeof(System.InvalidCastException)",
				result.Message
			);
		}

		[Fact]
		public static void Interface()
		{
			var ex = new DisposableClass();

			var result = Record.Exception(() => Assert.IsNotType(typeof(IDisposable), ex, exactMatch: false));

			Assert.IsType<IsNotTypeException>(result);
			Assert.Equal(
				"Assert.IsNotType() Failure: Value is a compatible type" + Environment.NewLine +
				"Expected: typeof(System.IDisposable)" + Environment.NewLine +
				"Actual:   typeof(TypeAssertsTests+DisposableClass)",
				result.Message
			);
		}

		[Fact]
		public static void IncompatibleType()
		{
			Assert.IsNotType(typeof(InvalidCastException), new InvalidOperationException(), exactMatch: false);
		}
	}

#pragma warning restore xUnit2007 // Do not use typeof expression to check the type

	public static class IsType_Generic
	{
		[Fact]
		public static void MatchingType()
		{
			var ex = new InvalidCastException();

			Assert.IsType<InvalidCastException>(ex);
		}

		[Fact]
		public static void ReturnsCastObject()
		{
			var ex = new InvalidCastException();

			var result = Assert.IsType<InvalidCastException>(ex);

			Assert.Same(ex, result);
		}

		[Fact]
		public static void UnmatchedType()
		{
			var result = Record.Exception(() => Assert.IsType<InvalidCastException>(new InvalidOperationException()));

			Assert.IsType<IsTypeException>(result);
			Assert.Equal(
				"Assert.IsType() Failure: Value is not the exact type" + Environment.NewLine +
				"Expected: typeof(System.InvalidCastException)" + Environment.NewLine +
				"Actual:   typeof(System.InvalidOperationException)",
				result.Message
			);
		}

#if NETFRAMEWORK

		[Fact]
		public static async Task UnmatchedTypesWithIdenticalNamesShowAssemblies()
		{
			var dynamicAssembly = await CSharpDynamicAssembly.Create("namespace System.Xml { public class XmlException: Exception { } }");
			var assembly = Assembly.LoadFile(dynamicAssembly.FileName);
			var dynamicXmlExceptionType = assembly.GetType("System.Xml.XmlException");
			Assert.NotNull(dynamicXmlExceptionType);
			var ex = Activator.CreateInstance(dynamicXmlExceptionType);

			var result = Record.Exception(() => Assert.IsType<XmlException>(ex));

			Assert.IsType<IsTypeException>(result);
			Assert.Equal(
				"Assert.IsType() Failure: Value is not the exact type" + Environment.NewLine +
				"Expected: typeof(System.Xml.XmlException) (from " + typeof(XmlException).Assembly.FullName + ")" + Environment.NewLine +
				"Actual:   typeof(System.Xml.XmlException) (from " + assembly.FullName + ")",
				result.Message
			);
		}

#endif  // NETFRAMEWORK

		[Fact]
		public static void NullObject()
		{
			var result = Record.Exception(() => Assert.IsType<object>(null));

			Assert.IsType<IsTypeException>(result);
			Assert.Equal(
				"Assert.IsType() Failure: Value is null" + Environment.NewLine +
				"Expected: typeof(object)" + Environment.NewLine +
				"Actual:   null",
				result.Message
			);
		}
	}

	public static class IsType_Generic_InexactMatch
	{
		[Fact]
		public static void NullObject()
		{
			var result = Record.Exception(() => Assert.IsType<object>(null, exactMatch: false));

			Assert.IsType<IsTypeException>(result);
			Assert.Equal(
				"Assert.IsType() Failure: Value is null" + Environment.NewLine +
				"Expected: typeof(object)" + Environment.NewLine +
				"Actual:   null",
				result.Message
			);
		}

		[Fact]
		public static void SameType()
		{
			var ex = new InvalidCastException();

			Assert.IsType<InvalidCastException>(ex, exactMatch: false);
		}

		[Fact]
		public static void BaseType()
		{
			var ex = new InvalidCastException();

			Assert.IsType<Exception>(ex, exactMatch: false);
		}

		[Fact]
		public static void Interface()
		{
			var ex = new DisposableClass();

			Assert.IsType<IDisposable>(ex, exactMatch: false);
		}

		[Fact]
		public static void ReturnsCastObject()
		{
			var ex = new InvalidCastException();

			var result = Assert.IsType<InvalidCastException>(ex, exactMatch: false);

			Assert.Same(ex, result);
		}

		[Fact]
		public static void IncompatibleType()
		{
			var result =
				Record.Exception(
					() => Assert.IsType<InvalidCastException>(new InvalidOperationException(), exactMatch: false)
				);

			Assert.IsType<IsTypeException>(result);
			Assert.Equal(
				"Assert.IsType() Failure: Value is an incompatible type" + Environment.NewLine +
				"Expected: typeof(System.InvalidCastException)" + Environment.NewLine +
				"Actual:   typeof(System.InvalidOperationException)",
				result.Message
			);
		}
	}

#pragma warning disable xUnit2007 // Do not use typeof expression to check the type

	public static class IsType_NonGeneric
	{
		[Fact]
		public static void MatchingType()
		{
			var ex = new InvalidCastException();

			Assert.IsType(typeof(InvalidCastException), ex);
		}

		[Fact]
		public static void UnmatchedTypeThrows()
		{
			var result = Record.Exception(() => Assert.IsType(typeof(InvalidCastException), new InvalidOperationException()));

			Assert.IsType<IsTypeException>(result);
			Assert.Equal(
				"Assert.IsType() Failure: Value is not the exact type" + Environment.NewLine +
				"Expected: typeof(System.InvalidCastException)" + Environment.NewLine +
				"Actual:   typeof(System.InvalidOperationException)",
				result.Message
			);
		}

#if NETFRAMEWORK

		[Fact]
		public  static async Task UnmatchedTypesWithIdenticalNamesShowAssemblies()
		{
			var dynamicAssembly = await CSharpDynamicAssembly.Create("namespace System.Xml { public class XmlException: Exception { } }");
			var assembly = Assembly.LoadFile(dynamicAssembly.FileName);
			var dynamicXmlExceptionType = assembly.GetType("System.Xml.XmlException");
			Assert.NotNull(dynamicXmlExceptionType);
			var ex = Activator.CreateInstance(dynamicXmlExceptionType);

			var result = Record.Exception(() => Assert.IsType(typeof(XmlException), ex));

			Assert.IsType<IsTypeException>(result);
			Assert.Equal(
				"Assert.IsType() Failure: Value is not the exact type" + Environment.NewLine +
				"Expected: typeof(System.Xml.XmlException) (from " + typeof(XmlException).Assembly.FullName + ")" + Environment.NewLine +
				"Actual:   typeof(System.Xml.XmlException) (from " + assembly.FullName + ")",
				result.Message
			);
		}

#endif  // NETFRAMEWORK

		[Fact]
		public static void NullObjectThrows()
		{
			var result = Record.Exception(() => Assert.IsType(typeof(object), null));

			Assert.IsType<IsTypeException>(result);
			Assert.Equal(
				"Assert.IsType() Failure: Value is null" + Environment.NewLine +
				"Expected: typeof(object)" + Environment.NewLine +
				"Actual:   null",
				result.Message
			);
		}
	}

	public static class IsType_NonGeneric_InexactMatch
	{
		[Fact]
		public static void NullObject()
		{
			var result = Record.Exception(() => Assert.IsType(typeof(object), null, exactMatch: false));

			Assert.IsType<IsTypeException>(result);
			Assert.Equal(
				"Assert.IsType() Failure: Value is null" + Environment.NewLine +
				"Expected: typeof(object)" + Environment.NewLine +
				"Actual:   null",
				result.Message
			);
		}

		[Fact]
		public static void SameType()
		{
			var ex = new InvalidCastException();

			Assert.IsType(typeof(InvalidCastException), ex, exactMatch: false);
		}

		[Fact]
		public static void BaseType()
		{
			var ex = new InvalidCastException();

			Assert.IsType(typeof(Exception), ex, exactMatch: false);
		}

		[Fact]
		public static void Interface()
		{
			var ex = new DisposableClass();

			Assert.IsType(typeof(IDisposable), ex, exactMatch: false);
		}

		[Fact]
		public static void ReturnsCastObject()
		{
			var ex = new InvalidCastException();

			var result = Assert.IsType<InvalidCastException>(ex, exactMatch: false);

			Assert.Same(ex, result);
		}

		[Fact]
		public static void IncompatibleType()
		{
			var result =
				Record.Exception(
					() => Assert.IsType(typeof(InvalidCastException), new InvalidOperationException(), exactMatch: false)
				);

			Assert.IsType<IsTypeException>(result);
			Assert.Equal(
				"Assert.IsType() Failure: Value is an incompatible type" + Environment.NewLine +
				"Expected: typeof(System.InvalidCastException)" + Environment.NewLine +
				"Actual:   typeof(System.InvalidOperationException)",
				result.Message
			);
		}
	}

#pragma warning restore xUnit2007 // Do not use typeof expression to check the type

	class DisposableClass : IDisposable
	{
		public void Dispose()
		{ }
	}

#if NETFRAMEWORK

	class CSharpDynamicAssembly : CSharpAcceptanceTestAssembly
	{
		public CSharpDynamicAssembly() :
			base(Path.GetTempPath())
		{ }

		protected override IEnumerable<string> GetStandardReferences() =>
			[];

		public static async Task<CSharpDynamicAssembly> Create(string code)
		{
			var assembly = new CSharpDynamicAssembly();
			await assembly.Compile([code]);
			return assembly;
		}
	}

#endif
}
