---
title: xUnit1040
description: The type argument to theory data is nullable, while the type of the corresponding test method parameter is not
category: Usage
severity: Warning
v2: true
v3: true
---

## Cause

The `TheoryData` or `TheoryDataRow` type argument is marked as nullable, and the test method argument is marked as non-nullable.

## Reason for rule

Passing `null` data to a test method that isn't expecting it could cause runtime errors or unpredictable test results
(either false positives or false negatives).

## How to fix violations

To fix a violation of this rule, either make the theory data type non-nullable, or make the test method parameter nullable.

## Examples

### Violates

#### Using `TheoryData<>` (for v2 and v3)

```csharp
using Xunit;

public class xUnit1040
{
    public static TheoryData<string?> PropertyData =>
        new() { "Hello", "World", default(string) };

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(string _) { }
}
```

#### Using `TheoryDataRow<>` (for v3 only)

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1040
{
    public static IEnumerable<TheoryDataRow<string?>> PropertyData =>
        [new("Hello"), new("World"), new(null)];

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(string _) { }
}
```

```csharp
using System.Collections;
using System.Collections.Generic;
using Xunit;

public class ClassRowData : IEnumerable<TheoryDataRow<string?>>
{
    public IEnumerator<TheoryDataRow<string?>> GetEnumerator()
    {
        yield return new("Hello");
        yield return new("World");
        yield return new(null);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class xUnit1040
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

public class xUnit1040
{
    public static TheoryData<string> PropertyData =>
        new() { "Hello", "World" };

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(string _) { }
}
```

```csharp
using Xunit;

public class xUnit1040
{
    public static TheoryData<string?> PropertyData =>
        new() { "Hello", "World", default(string) };

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(string? _) { }
}
```

#### Using `TheoryDataRow<>` (for v3 only)

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1040
{
    public static IEnumerable<TheoryDataRow<string>> PropertyData =>
        [new("Hello"), new("World")];

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(string _) { }
}
```

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1040
{
    public static IEnumerable<TheoryDataRow<string?>> PropertyData =>
        [new("Hello"), new("World"), new(null)];

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(string? _) { }
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
        yield return new("Hello");
        yield return new("World");
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class xUnit1040
{
    [Theory]
    [ClassData(typeof(ClassRowData))]
    public void TestMethod(string _) { }
}
```

```csharp
using System.Collections;
using System.Collections.Generic;
using Xunit;

public class ClassRowData : IEnumerable<TheoryDataRow<string?>>
{
    public IEnumerator<TheoryDataRow<string?>> GetEnumerator()
    {
        yield return new("Hello");
        yield return new("World");
        yield return new(null);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class xUnit1040
{
    [Theory]
    [ClassData(typeof(ClassRowData))]
    public void TestMethod(string? _) { }
}
```
