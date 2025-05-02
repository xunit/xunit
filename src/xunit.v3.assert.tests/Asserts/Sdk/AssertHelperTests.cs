using System;
using System.Linq.Expressions;
using Xunit;
using Xunit.Internal;

public class AssertHelperTests
{
	[Fact]
	public void Properties()
	{
		var result = AssertHelper.ParseMemberExpressions(
			(Expression<Func<Child, object?>>)(c => c.Property),
			(Expression<Func<Parent, object?>>)(c => c.ParentProperty.Property),
			(Expression<Func<Grandparent, object?>>)(c => c.GrandparentProperty!.ParentProperty.Property)
		);

		Assert.Collection(
			result,
			child => Assert.Equal(nameof(Child.Property), Assert.Single(child)),
			parent => Assert.Collection(
				parent,
				name => Assert.Equal(nameof(Parent.ParentProperty), name),
				name => Assert.Equal(nameof(Child.Property), name)
			),
			grandparent => Assert.Collection(
				grandparent,
				name => Assert.Equal(nameof(Grandparent.GrandparentProperty), name),
				name => Assert.Equal(nameof(Parent.ParentProperty), name),
				name => Assert.Equal(nameof(Child.Property), name)
			)
		);
	}

	[Fact]
	public void Fields()
	{
		var result = AssertHelper.ParseMemberExpressions(
			(Expression<Func<Child, object?>>)(c => c.Field),
			(Expression<Func<Parent, object?>>)(c => c.ParentField.Field),
			(Expression<Func<Grandparent, object?>>)(c => c.GrandparentField!.ParentField.Field)
		);

		Assert.Collection(
			result,
			child => Assert.Equal(nameof(Child.Field), Assert.Single(child)),
			parent => Assert.Collection(
				parent,
				name => Assert.Equal(nameof(Parent.ParentField), name),
				name => Assert.Equal(nameof(Child.Field), name)
			),
			grandparent => Assert.Collection(
				grandparent,
				name => Assert.Equal(nameof(Grandparent.GrandparentField), name),
				name => Assert.Equal(nameof(Parent.ParentField), name),
				name => Assert.Equal(nameof(Child.Field), name)
			)
		);
	}

	[Fact]
	public void Mixed()
	{
		var result = AssertHelper.ParseMemberExpressions(
			(Expression<Func<Parent, object?>>)(c => c.ParentProperty.Field),
			(Expression<Func<Grandparent, object?>>)(c => c.GrandparentProperty!.ParentField.Property)
		);

		Assert.Collection(
			result,
			parent => Assert.Collection(
				parent,
				name => Assert.Equal(nameof(Parent.ParentProperty), name),
				name => Assert.Equal(nameof(Child.Field), name)
			),
			grandparent => Assert.Collection(
				grandparent,
				name => Assert.Equal(nameof(Grandparent.GrandparentProperty), name),
				name => Assert.Equal(nameof(Parent.ParentField), name),
				name => Assert.Equal(nameof(Child.Property), name)
			)
		);
	}

	[Fact]
	public void MethodNotSupported()
	{
		var expression = (Expression<Func<Child, object?>>)(c => c.Method());

		var ex = Record.Exception(() => AssertHelper.ParseMemberExpressions(expression));

		var argEx = Assert.IsType<ArgumentException>(ex);
		Assert.Equal("expressions", argEx.ParamName);
		Assert.StartsWith($"Expression '{expression}' is not supported. Only property or field expressions from the lambda parameter are supported.", ex.Message);
	}

	[Fact]
	public void ChildMethodNotSupported()
	{
		var expression = (Expression<Func<Parent, object?>>)(p => p.ParentProperty.Method());

		var ex = Record.Exception(() => AssertHelper.ParseMemberExpressions(expression));

		var argEx = Assert.IsType<ArgumentException>(ex);
		Assert.Equal("expressions", argEx.ParamName);
		Assert.StartsWith($"Expression '{expression}' is not supported. Only property or field expressions from the lambda parameter are supported.", ex.Message);
	}

	[Fact]
	public void IndexerNotSupported()
	{
		var expression = (Expression<Func<Child, object?>>)(c => c[0]);

		var ex = Record.Exception(() => AssertHelper.ParseMemberExpressions(expression));

		var argEx = Assert.IsType<ArgumentException>(ex);
		Assert.Equal("expressions", argEx.ParamName);
		Assert.StartsWith($"Expression '{expression}' is not supported. Only property or field expressions from the lambda parameter are supported.", ex.Message);
	}

	[Fact]
	public void ChildIndexerNotSupported()
	{
		var expression = (Expression<Func<Parent, object?>>)(p => p.ParentField[0]);

		var ex = Record.Exception(() => AssertHelper.ParseMemberExpressions(expression));

		var argEx = Assert.IsType<ArgumentException>(ex);
		Assert.Equal("expressions", argEx.ParamName);
		Assert.StartsWith($"Expression '{expression}' is not supported. Only property or field expressions from the lambda parameter are supported.", ex.Message);
	}

	static readonly string foo = "Hello world";

	[Fact]
	public void ExpressionMustOriginateFromParameter()
	{
		var expression = (Expression<Func<Child, object?>>)(c => foo.Length);

		var ex = Record.Exception(() => AssertHelper.ParseMemberExpressions(expression));

		var argEx = Assert.IsType<ArgumentException>(ex);
		Assert.Equal("expressions", argEx.ParamName);
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
