---
title: xUnit1037
description: There are fewer theory data type arguments than required by the parameters of the test method
category: Usage
severity: Error
v2: true
v3: true
---

## Cause

When you use `TheoryData` or `TheoryDataRow` with `[MemberData]` or `[ClassData]`, the number of generic types
must match the number of parameters in the test method. In this case, you have provided too few types.

## Reason for rule

You must provide the correct number of arguments to the test method to run the test.

## How to fix violations

To fix a violation of this rule, either add more type parameters to match the method signature, or remove
parameters from the test method.

## Examples

### Violates

#### Using `TheoryData<>` (for v2 and v3)

```csharp
using Xunit;

public class xUnit1037
{
    public static TheoryData<int> PropertyData =>
        new() { 1, 2, 3 };

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(int _1, string _2) { }
}
```

#### Using `TheoryDataRow<>` (for v3 only)

```csharp
using Xunit;

public class xUnit1037
{
    public static IEnumerable<TheoryDataRow<int>> PropertyData =>
        [new(1), new(2), new(3)];

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(int _1, string _2) { }
}
```

```csharp
using System.Collections;
using System.Collections.Generic;
using Xunit;

public class ClassRowData : IEnumerable<TheoryDataRow<int>>
{
    public IEnumerator<TheoryDataRow<int>> GetEnumerator()
    {
        yield return new(1);
        yield return new(2);
        yield return new(3);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class xUnit1037
{
    [Theory]
    [ClassData(typeof(ClassRowData))]
    public void TestMethod(int _1, string _2) { }
}
```

### Does not violate

#### Using `TheoryData<>` (for v2 and v3)

```csharp
using Xunit;

public class xUnit1037
{
    public static TheoryData<int, string> PropertyData =>
        new() { { 1, "Hello" }, { 2, "World" } };

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(int _1, string _2) { }
}
```

```csharp
using Xunit;

public class xUnit1037
{
    public static TheoryData<int> PropertyData =>
        new() { 1, 2, 3 };

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(int _) { }
}
```

#### Using `TheoryDataRow<>` (for v3 only)

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1037
{
    public static IEnumerable<TheoryDataRow<int, string>> PropertyData =>
        [new(1, "Hello"), new(2, "World")];

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(int _1, string _2) { }
}
```

```csharp
using Xunit;

public class xUnit1037
{
    public static IEnumerable<TheoryDataRow<int>> PropertyData =>
        [new(1), new(2), new(3)];

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(int _) { }
}
```

```csharp
using System.Collections;
using System.Collections.Generic;
using Xunit;

public class ClassRowData : IEnumerable<TheoryDataRow<int, string>>
{
    public IEnumerator<TheoryDataRow<int, string>> GetEnumerator()
    {
        yield return new(1, "Hello");
        yield return new(2, "World");
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class xUnit1037
{
    [Theory]
    [ClassData(typeof(ClassRowData))]
    public void TestMethod(int _1, string _2) { }
}
```

```csharp
using System.Collections;
using System.Collections.Generic;
using Xunit;

public class ClassRowData : IEnumerable<TheoryDataRow<int>>
{
    public IEnumerator<TheoryDataRow<int>> GetEnumerator()
    {
        yield return new(1);
        yield return new(2);
        yield return new(3);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class xUnit1037
{
    [Theory]
    [ClassData(typeof(ClassRowData))]
    public void TestMethod(int _1) { }
}
```
