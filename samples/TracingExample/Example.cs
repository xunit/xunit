using System;
using Xunit;

public class Example
{
    [Fact, TracingSplicer]
    public void TestThis()
    {
        Console.WriteLine("I'm inside the test!");
    }
}