using System;
using SpecificationBaseStyle;
using Xunit;

public class When_you_have_a_new_stack : SpecificationBase
{
    Stack<string> stack;

    protected override void Because()
    {
        stack = new Stack<string>();
    }

    [Observation]
    public void should_be_empty()
    {
        Assert.True(stack.IsEmpty);
    }

    [Observation]
    public void should_not_allow_you_to_call_Pop()
    {
        Assert.Throws<InvalidOperationException>(() => stack.Pop());
    }

    [Observation]
    public void should_not_allow_you_to_call_Top()
    {
        Assert.Throws<InvalidOperationException>(() => { string unused = stack.Top; });
    }
}

public class When_you_push_an_item_onto_the_stack : SpecificationBase
{
    Stack<string> stack;

    protected override void EstablishContext()
    {
        stack = new Stack<string>();
    }

    protected override void Because()
    {
        stack.Push("first element");
    }

    [Observation]
    public void should_not_be_empty()
    {
        Assert.False(stack.IsEmpty);
    }
}

public class When_you_push_null_onto_the_stack : SpecificationBase
{
    Stack<string> stack;

    protected override void EstablishContext()
    {
        stack = new Stack<string>();
    }

    protected override void Because()
    {
        stack.Push(null);
    }

    [Observation]
    public void should_not_be_empty()
    {
        Assert.False(stack.IsEmpty);
    }

    [Observation]
    public void should_return_null_when_calling_Top()
    {
        Assert.Null(stack.Top);
    }

    [Observation]       // Order dependent: calling Pop before Top would cause a test failure!
    public void should_return_null_when_calling_Pop()
    {
        Assert.Null(stack.Pop());
    }
}

public class When_you_push_then_pop_a_value_from_the_stack : SpecificationBase
{
    Stack<string> stack;
    const string expected = "first element";
    string actual;

    protected override void EstablishContext()
    {
        stack = new Stack<string>();
    }

    protected override void Because()
    {
        stack.Push(expected);
        actual = stack.Pop();
    }

    [Observation]
    public void should_get_the_value_that_was_pushed()
    {
        Assert.Equal(expected, actual);
    }

    [Observation]
    public void should_be_empty_again()
    {
        Assert.True(stack.IsEmpty);
    }
}

public class When_you_push_an_item_on_the_stack_and_call_Top : SpecificationBase
{
    Stack<string> stack;
    const string expected = "first element";
    string actual;

    protected override void EstablishContext()
    {
        stack = new Stack<string>();
    }

    protected override void Because()
    {
        stack.Push(expected);
        actual = stack.Top;
    }

    [Observation]
    public void should_not_modify_the_stack()
    {
        Assert.False(stack.IsEmpty);
    }

    [Observation]
    public void should_return_the_last_item_pushed_onto_the_stack()
    {
        Assert.Equal(expected, actual);
    }

    [Observation]
    public void should_return_the_same_item_for_subsequent_Top_calls()
    {
        Assert.Equal(actual, stack.Top);
        Assert.Equal(actual, stack.Top);
        Assert.Equal(actual, stack.Top);
    }
}

public class When_you_push_several_items_onto_the_stack : SpecificationBase
{
    Stack<string> stack;
    const string firstElement = "firstElement";
    const string secondElement = "secondElement";
    const string thirdElement = "thirdElement";

    protected override void EstablishContext()
    {
        stack = new Stack<string>();
    }

    protected override void Because()
    {
        stack.Push(firstElement);
        stack.Push(secondElement);
        stack.Push(thirdElement);
    }

    [Observation]
    public void should_Pop_last_item_first()
    {
        Assert.Equal(thirdElement, stack.Pop());
    }

    [Observation]
    public void should_Pop_second_item_second()
    {
        Assert.Equal(secondElement, stack.Pop());
    }

    [Observation]
    public void should_Pop_first_item_last()
    {
        Assert.Equal(firstElement, stack.Pop());
    }
}