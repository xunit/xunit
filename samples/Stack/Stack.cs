using System;
using System.Collections.Generic;
using System.Linq;

public class Stack<T>
{
    List<T> elements = new List<T>();

    public int Count
    {
        get { return elements.Count; }
    }

    public bool Contains(T element)
    {
        return elements.Contains(element);
    }

    public T Peek()
    {
        if (Count == 0)
            throw new InvalidOperationException("empty stack");

        return elements.Last();
    }

    public T Pop()
    {
        T element = Peek();
        elements.RemoveAt(Count - 1);
        return element;
    }

    public void Push(T element)
    {
        elements.Add(element);
    }
}