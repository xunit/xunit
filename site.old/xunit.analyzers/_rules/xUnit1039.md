---
title: xUnit1039
description: The type argument to theory data is not compatible with the type of the corresponding test method parameter
category: Usage
severity: Error
v2: true
v3: true
---

## Cause

The type argument given to `TheoryData` or `TheoryDataRow` is not compatible with the matching parameter in the test method.

## Reason for rule

When the data types aren't compatible, then the test will fail at runtime with a type mismatch, instead of
running the test.

## How to fix violations

To fix a violation of this rule, make the types in the parameter and theory data compatible.

## Examples

### Violates

#### Using `TheoryData<>` (for v2 and v3)

```csharp
using Xunit;

public class xUnit1039
{
    public static TheoryData<int> PropertyData =>
        new() { 1, 2, 3 };

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(string _) { }
}
```

#### Using `TheoryDataRow<>` (for v3 only)

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1039
{
    public static IEnumerable<TheoryDataRow<int>> PropertyData =>
        [new(1), new(2), new(3)];

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(string _) { }
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

public class xUnit1039
{
    [Theory]
    [ClassData(typeof(ClassRowData))]
    public void TestMethod(string _) { }
}
```

### Does not violate

#### Using `TheoryData<>` (for v2 and v3)

```csharp
using Xunit;

public class xUnit1039
{
    public static TheoryData<int> PropertyData =>
        new() { 1, 2, 3 };

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(int _) { }
}
```

```csharp
using Xunit;

public class xUnit1039
{
    public static TheoryData<string> PropertyData =>
        new() { "1", "2", "3" };

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(string _) { }
}
```

#### Using `TheoryDataRow<>` (for v3 only)

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1039
{
    public static IEnumerable<TheoryDataRow<int>> PropertyData =>
        [new(1), new(2), new(3)];

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(int _) { }
}
```

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1039
{
    public static IEnumerable<TheoryDataRow<string>> PropertyData =>
        [new("1"), new("2"), new("3")];

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(string _) { }
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

public class xUnit1039
{
    [Theory]
    [ClassData(typeof(ClassRowData))]
    public void TestMethod(int _) { }
}
```

```csharp
using System.Collections;
using System.Collections.Generic;
using Xunit;

public class ClassRowData : IEnumerable<TheoryDataRow<string>>
{
    public IEnumerator<TheoryDataRow<string>> GetEnumerator()
    {
        yield return new("1");
        yield return new("2");
        yield return new("3");
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class xUnit1039
{
    [Theory]
    [ClassData(typeof(ClassRowData))]
    public void TestMethod(string _) { }
}
```
