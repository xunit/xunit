using System;
using Xunit;
using Xunit.Sdk;

#if NETFRAMEWORK
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
#endif

public class TypeAssertsTests
{
	public class IsAssignableFrom_Generic
	{
		[Fact]
		public void NullObject()
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
		public void SameType()
		{
			var ex = new InvalidCastException();

			Assert.IsAssignableFrom<InvalidCastException>(ex);
		}

		[Fact]
		public void BaseType()
		{
			var ex = new InvalidCastException();

			Assert.IsAssignableFrom<Exception>(ex);
		}

		[Fact]
		public void Interface()
		{
			var ex = new DisposableClass();

			Assert.IsAssignableFrom<IDisposable>(ex);
		}

		[Fact]
		public void ReturnsCastObject()
		{
			var ex = new InvalidCastException();

			var result = Assert.IsAssignableFrom<InvalidCastException>(ex);

			Assert.Same(ex, result);
		}

		[Fact]
		public void IncompatibleType()
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
	public class IsAssignableFrom_NonGeneric
	{
		[Fact]
		public void NullObject()
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
		public void SameType()
		{
			var ex = new InvalidCastException();

			Assert.IsAssignableFrom(typeof(InvalidCastException), ex);
		}

		[Fact]
		public void BaseType()
		{
			var ex = new InvalidCastException();

			Assert.IsAssignableFrom(typeof(Exception), ex);
		}

		[Fact]
		public void Interface()
		{
			var ex = new DisposableClass();

			Assert.IsAssignableFrom(typeof(IDisposable), ex);
		}

		[Fact]
		public void ReturnsCastObject()
		{
			var ex = new InvalidCastException();

			var result = Assert.IsAssignableFrom<InvalidCastException>(ex);

			Assert.Same(ex, result);
		}

		[Fact]
		public void IncompatibleType()
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

	public class IsNotAssignableFrom_Generic
	{
		[Fact]
		public void NullObject()
		{
			Assert.IsNotAssignableFrom<object>(null);
		}

		[Fact]
		public void SameType()
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
		public void BaseType()
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
		public void Interface()
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
		public void IncompatibleType()
		{
			Assert.IsNotAssignableFrom<InvalidCastException>(new InvalidOperationException());
		}
	}

	public class IsNotAssignableFrom_NonGeneric
	{
		[Fact]
		public void NullObject()
		{
			Assert.IsNotAssignableFrom(typeof(object), null);
		}

		[Fact]
		public void SameType()
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
		public void BaseType()
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
		public void Interface()
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
		public void IncompatibleType()
		{
			Assert.IsNotAssignableFrom(typeof(InvalidCastException), new InvalidOperationException());
		}
	}

	public class IsNotType_Generic
	{
		[Fact]
		public void UnmatchedType()
		{
			var ex = new InvalidCastException();

			Assert.IsNotType<Exception>(ex);
		}

		[Fact]
		public void MatchedType()
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
		public void NullObject()
		{
			Assert.IsNotType<object>(null);
		}
	}

#pragma warning disable xUnit2007 // Do not use typeof expression to check the type
	public class IsNotType_NonGeneric
	{
		[Fact]
		public void UnmatchedType()
		{
			var ex = new InvalidCastException();

			Assert.IsNotType(typeof(Exception), ex);
		}

		[Fact]
		public void MatchedType()
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
		public void NullObject()
		{
			Assert.IsNotType(typeof(object), null);
		}
	}
#pragma warning restore xUnit2007 // Do not use typeof expression to check the type

	public class IsType_Generic : TypeAssertsTests
	{
		[Fact]
		public void MatchingType()
		{
			var ex = new InvalidCastException();

			Assert.IsType<InvalidCastException>(ex);
		}

		[Fact]
		public void ReturnsCastObject()
		{
			var ex = new InvalidCastException();

			var result = Assert.IsType<InvalidCastException>(ex);

			Assert.Same(ex, result);
		}

		[Fact]
		public void UnmatchedType()
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
		public async Task UnmatchedTypesWithIdenticalNamesShowAssemblies()
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
#endif

		[Fact]
		public void NullObject()
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

#pragma warning disable xUnit2007 // Do not use typeof expression to check the type
	public class IsType_NonGeneric : TypeAssertsTests
	{
		[Fact]
		public void MatchingType()
		{
			var ex = new InvalidCastException();

			Assert.IsType(typeof(InvalidCastException), ex);
		}

		[Fact]
		public void UnmatchedTypeThrows()
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
		public async Task UnmatchedTypesWithIdenticalNamesShowAssemblies()
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
#endif

		[Fact]
		public void NullObjectThrows()
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
			new[] { "mscorlib.dll" };

		public static async Task<CSharpDynamicAssembly> Create(string code)
		{
			var assembly = new CSharpDynamicAssembly();
			await assembly.Compile(new[] { code });
			return assembly;
		}
	}
#endif
}
