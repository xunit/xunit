using System;
using Xunit;

public class StackTests
{
    [Fact]
    public void NoElementsShouldBeEmpty()
    {
        Stack<string> stack = new Stack<string>();

        Assert.True(stack.IsEmpty);
    }

    [Fact]
    public void PushAnElementShouldNotBeEmpty()
    {
        Stack<string> stack = new Stack<string>();

        stack.Push("first element");

        Assert.False(stack.IsEmpty);
    }

    [Fact]
    public void PushNullShouldNotBeEmpty()
    {
        Stack<string> stack = new Stack<string>();

        stack.Push(null);

        Assert.False(stack.IsEmpty);
    }

    [Fact]
    public void PushThenPopShouldLeaveStackEmpty()
    {
        Stack<string> stack = new Stack<string>();

        stack.Push("first element");
        stack.Pop();

        Assert.True(stack.IsEmpty);
    }

    [Fact]
    public void PopShouldReturnPushedElement()
    {
        Stack<string> stack = new Stack<string>();
        string expected = "first element";
        stack.Push(expected);

        string result = stack.Pop();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void PopShouldReturnNull()
    {
        Stack<string> stack = new Stack<string>();
        stack.Push(null);

        string result = stack.Pop();

        Assert.Null(result);
    }

    [Fact]
    public void TopShouldReturnNull()
    {
        Stack<string> stack = new Stack<string>();
        stack.Push(null);

        Assert.Null(stack.Top);
    }

    [Fact]
    public void MultiplePopsShouldreturnElementsInCorrectOrder()
    {
        string firstElement = "firstElement";
        string secondElement = "secondElement";
        string thirdElement = "thirdElement";

        Stack<string> stack = new Stack<string>();
        stack.Push(firstElement);
        stack.Push(secondElement);
        stack.Push(thirdElement);

        Assert.Equal(thirdElement, stack.Pop());
        Assert.Equal(secondElement, stack.Pop());
        Assert.Equal(firstElement, stack.Pop());
    }

    [Fact]
    public void PopEmptyStack()
    {
        Stack<string> stack = new Stack<string>();

        Assert.Throws<InvalidOperationException>(
            delegate
            {
                stack.Pop();
            });
    }

    [Fact]
    public void TopShouldNotChangeTheStateOfTheStack()
    {
        Stack<string> stack = new Stack<string>();
        stack.Push("42");

        string element = stack.Top;

        Assert.False(stack.IsEmpty);
    }

    [Fact]
    public void TopShouldReturnTopmostElement()
    {
        Stack<string> stack = new Stack<string>();
        stack.Push("42");

        Assert.Equal("42", stack.Top);
    }

    [Fact]
    public void MultipleTopCallsShouldReturnTopmostElement()
    {
        string firstElement = "firstElement";
        string secondElement = "secondElement";
        string thirdElement = "thirdElement";

        Stack<string> stack = new Stack<string>();
        stack.Push(firstElement);
        stack.Push(secondElement);
        stack.Push(thirdElement);

        for (int index = 0; index < 10; index++)
            Assert.Equal(thirdElement, stack.Top);
    }

    [Fact]
    public void TopEmptyStack()
    {
        Stack<string> stack = new Stack<string>();

        Assert.Throws<InvalidOperationException>(
            delegate
            {
                string element = stack.Top;
            });
    }

    [Fact]
    public void StackShouldWorkWithGenericTypes()
    {
        Stack<int> stack = new Stack<int>();

        stack.Push(42);

        Assert.Equal(42, stack.Pop());
    }
}