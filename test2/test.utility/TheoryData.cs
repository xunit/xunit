using System.Collections;
using System.Collections.Generic;

public abstract class TheoryData : IEnumerable<object[]>
{
    private readonly List<object[]> data = new List<object[]>();

    protected void AddRow(params object[] values)
    {
        data.Add(values);
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        return data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class TheoryData<T> : TheoryData
{
    public void Add(T p)
    {
        AddRow(p);
    }
}

public class TheoryData<T1, T2> : TheoryData
{
    public void Add(T1 p1, T2 p2)
    {
        AddRow(p1, p2);
    }
}

public class TheoryData<T1, T2, T3> : TheoryData
{
    public void Add(T1 p1, T2 p2, T3 p3)
    {
        AddRow(p1, p2, p3);
    }
}

public class TheoryData<T1, T2, T3, T4> : TheoryData
{
    public void Add(T1 p1, T2 p2, T3 p3, T4 p4)
    {
        AddRow(p1, p2, p3, p4);
    }
}

public class TheoryData<T1, T2, T3, T4, T5> : TheoryData
{
    public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
    {
        AddRow(p1, p2, p3, p4, p5);
    }
}