using System;
using SubSpec;
using Xunit;

public class StackSpecs
{
    [Specification]
    public void EmptyStackSpecifications()
    {
        Stack<string> stack = null;

        "Given a new stack".Context(() => stack = new Stack<string>());

        "the stack is empty".Assert(() => Assert.True(stack.IsEmpty));
        "calling Pop throws".Assert(() =>
            Assert.Throws<InvalidOperationException>(() => stack.Pop()));
        "calling Top throws".Assert(() =>
            Assert.Throws<InvalidOperationException>(() => { string unused = stack.Top; }));
    }

    [Specification]
    public void PushSpecifications()
    {
        Stack<string> stack = null;

        "Given a new stack".Context(() => stack = new Stack<string>());

        "with an element pushed into it".Do(() => stack.Push("first element"));

        "the stack is not empty".Assert(() => Assert.False(stack.IsEmpty));
    }

    [Specification]
    public void PushNullSpecifications()
    {
        Stack<string> stack = null;

        "Given a new stack".Context(() => stack = new Stack<string>());

        "with null pushed into it".Do(() => stack.Push(null));

        "the stack is not empty".Assert(() => Assert.False(stack.IsEmpty));
        "the popped value is null".Assert(() => Assert.Null(stack.Pop()));
        "Top returns null".Assert(() => Assert.Null(stack.Top));
    }

    [Specification]
    public void PushPopSpecification()
    {
        Stack<string> stack = null;
        const string expected = "first element";
        string actual = null;

        "Given a new stack".Context(() => stack = new Stack<string>());

        "with an element pushed then popped".Do(() =>
        {
            stack.Push(expected);
            actual = stack.Pop();
        });

        "the stack is empty again".Assert(() => Assert.True(stack.IsEmpty));
        "the value is popped".Assert(() => Assert.Equal(expected, actual));
    }

    [Specification]
    public void TopSpecifications()
    {
        Stack<string> stack = null;

        "Given a new stack".Context(() => stack = new Stack<string>());

        "with an element on it and Top called".Do(() =>
            {
                stack.Push("42");
                string unused = stack.Top;
            });

        "it does not modify the stack".Assert(() => Assert.False(stack.IsEmpty));
        "it returns the topmost element".Assert(() => Assert.Equal("42", stack.Top));
        "it returns the topmost element repeatedly".Assert(() =>
            {
                Assert.Equal("42", stack.Top);
                Assert.Equal("42", stack.Top);
                Assert.Equal("42", stack.Top);
            });

    }

    [Specification]
    public void OrderSpecifications()
    {
        const string firstElement = "firstElement";
        const string secondElement = "secondElement";
        const string thirdElement = "thirdElement";
        Stack<string> stack = null;

        "Given a new stack".Context(() => stack = new Stack<string>());

        "with three elements pushed onto it".Do(() =>
            {
                stack.Push(firstElement);
                stack.Push(secondElement);
                stack.Push(thirdElement);
            });

        "the elements pop off in reverse order".Assert(() =>
            {
                Assert.Equal(thirdElement, stack.Pop());
                Assert.Equal(secondElement, stack.Pop());
                Assert.Equal(firstElement, stack.Pop());
            });
    }
}
