#if !XUNIT_AOT

using System;
using System.Linq.Expressions;
using Xunit;
using Xunit.Internal;

public class AssertHelperTests
{
	public class ParseExclusionExpressions_LambdaExpression
	{
		[Fact]
		public void NullExpression()
		{
			var expression = (Expression<Func<Child, object?>>)null!;

			var ex = Record.Exception(() => AssertHelper.ParseExclusionExpressions(expression));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("exclusionExpressions", argEx.ParamName);
			Assert.StartsWith("Null expression is not valid.", ex.Message);
		}

		[Fact]
		public void Properties()
		{
			var result = AssertHelper.ParseExclusionExpressions(
				(Expression<Func<Child, object?>>)(c => c.Property),
				(Expression<Func<Parent, object?>>)(c => c.ParentProperty.Property),
				(Expression<Func<Grandparent, object?>>)(c => c.GrandparentProperty!.ParentProperty.Property)
			);

			Assert.Collection(
				result,
				ex => Assert.Equal((string.Empty, "Property"), ex),
				ex => Assert.Equal(("ParentProperty", "Property"), ex),
				ex => Assert.Equal(("GrandparentProperty.ParentProperty", "Property"), ex)
			);
		}

		[Fact]
		public void Fields()
		{
			var result = AssertHelper.ParseExclusionExpressions(
				(Expression<Func<Child, object?>>)(c => c.Field),
				(Expression<Func<Parent, object?>>)(c => c.ParentField.Field),
				(Expression<Func<Grandparent, object?>>)(c => c.GrandparentField!.ParentField.Field)
			);

			Assert.Collection(
				result,
				ex => Assert.Equal((string.Empty, "Field"), ex),
				ex => Assert.Equal(("ParentField", "Field"), ex),
				ex => Assert.Equal(("GrandparentField.ParentField", "Field"), ex)
			);
		}

		[Fact]
		public void Mixed()
		{
			var result = AssertHelper.ParseExclusionExpressions(
				(Expression<Func<Parent, object?>>)(c => c.ParentProperty.Field),
				(Expression<Func<Grandparent, object?>>)(c => c.GrandparentProperty!.ParentField.Property)
			);

			Assert.Collection(
				result,
				ex => Assert.Equal(("ParentProperty", "Field"), ex),
				ex => Assert.Equal(("GrandparentProperty.ParentField", "Property"), ex)
			);
		}

		[Fact]
		public void MethodNotSupported()
		{
			var expression = (Expression<Func<Child, object?>>)(c => c.Method());

			var ex = Record.Exception(() => AssertHelper.ParseExclusionExpressions(expression));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("exclusionExpressions", argEx.ParamName);
			Assert.StartsWith($"Expression '{expression}' is not supported. Only property or field expressions from the lambda parameter are supported.", ex.Message);
		}

		[Fact]
		public void ChildMethodNotSupported()
		{
			var expression = (Expression<Func<Parent, object?>>)(p => p.ParentProperty.Method());

			var ex = Record.Exception(() => AssertHelper.ParseExclusionExpressions(expression));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("exclusionExpressions", argEx.ParamName);
			Assert.StartsWith($"Expression '{expression}' is not supported. Only property or field expressions from the lambda parameter are supported.", ex.Message);
		}

		[Fact]
		public void IndexerNotSupported()
		{
			var expression = (Expression<Func<Child, object?>>)(c => c[0]);

			var ex = Record.Exception(() => AssertHelper.ParseExclusionExpressions(expression));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("exclusionExpressions", argEx.ParamName);
			Assert.StartsWith($"Expression '{expression}' is not supported. Only property or field expressions from the lambda parameter are supported.", ex.Message);
		}

		[Fact]
		public void ChildIndexerNotSupported()
		{
			var expression = (Expression<Func<Parent, object?>>)(p => p.ParentField[0]);

			var ex = Record.Exception(() => AssertHelper.ParseExclusionExpressions(expression));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("exclusionExpressions", argEx.ParamName);
			Assert.StartsWith($"Expression '{expression}' is not supported. Only property or field expressions from the lambda parameter are supported.", ex.Message);
		}

		static readonly string foo = "Hello world";

		[Fact]
		public void ExpressionMustOriginateFromParameter()
		{
			var expression = (Expression<Func<Child, object?>>)(c => foo.Length);

			var ex = Record.Exception(() => AssertHelper.ParseExclusionExpressions(expression));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("exclusionExpressions", argEx.ParamName);
			Assert.StartsWith($"Expression '{expression}' is not supported. Only property or field expressions from the lambda parameter are supported.", ex.Message);
		}

		class Child
		{
			public int Field = 2112;

			public string this[int idx] => idx.ToString();

			public string? Property => ToString();

			public string? Method() => ToString();
		}

		class Parent
		{
			public Child ParentField = new();

			public Child ParentProperty { get; } = new();
		}

		class Grandparent
		{
			public Parent? GrandparentField = new();

			public Parent? GrandparentProperty { get; set; }
		}
	}

	public class ParseExclusionExpressions_String
	{
		[Fact]
		public void NullExpression()
		{
			var ex = Record.Exception(() => AssertHelper.ParseExclusionExpressions(default(string)!));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("exclusionExpressions", argEx.ParamName);
			Assert.StartsWith("Null/empty expressions are not valid.", ex.Message);
		}

		[Fact]
		public void EmptyExpression()
		{
			var ex = Record.Exception(() => AssertHelper.ParseExclusionExpressions(string.Empty));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("exclusionExpressions", argEx.ParamName);
			Assert.StartsWith("Null/empty expressions are not valid.", ex.Message);
		}

		[Fact]
		public void StartsWithDot()
		{
			var ex = Record.Exception(() => AssertHelper.ParseExclusionExpressions(".Foo"));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("exclusionExpressions", argEx.ParamName);
			Assert.StartsWith("Expression '.Foo' is not valid. Expressions may not start with a period.", ex.Message);
		}

		[Fact]
		public void EndsWithDot()
		{
			var ex = Record.Exception(() => AssertHelper.ParseExclusionExpressions("Foo."));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("exclusionExpressions", argEx.ParamName);
			Assert.StartsWith("Expression 'Foo.' is not valid. Expressions may not end with a period.", ex.Message);
		}

		[Fact]
		public void SuccessCases()
		{
			var result = AssertHelper.ParseExclusionExpressions(
				"Child",
				"Parent.Child",
				"Grandparent.Parent.Child"
			);

			Assert.Collection(
				result,
				ex => Assert.Equal((string.Empty, "Child"), ex),
				ex => Assert.Equal(("Parent", "Child"), ex),
				ex => Assert.Equal(("Grandparent.Parent", "Child"), ex)
			);
		}
	}
}

#endif  // !XUNIT_AOT
