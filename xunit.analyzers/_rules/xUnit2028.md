---
title: xUnit2028
description: Do not use Assert.Empty or Assert.NotEmpty with problematic types
category: Assertions
severity: Warning
---

## Cause

Some problematic types ([`ArraySegment<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.arraysegment-1) and
[`StringValues`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.primitives.stringvalues)) should
not used with `Assert.Empty` or `Assert.NotEmpty`.

## Reason for rule

The reason `ArraySegment<T>` is problematic is because it has an implementation of `GetEnumerator` that can throw
exceptions, and the assertion library uses enumerator to determine when a collection is empty (or not).

The reason `StringValues` is problematic is because it contains an implicit cast to `string`, which the compiler
favors over its implementation of `IEnumerable`, and when the collection is empty, the string that it returns
is `null` (which is not legal for `Assert.Empty`). Further, you can't use `Assert.NotNull` because there is no
`string` overload, so you're not checking the implicit string for null-ness.

## How to fix violations

To fix a violation of this rule, it is recommended that you examine the item count via the `Count` property
with `Assert.Equal` or `Assert.NotEqual`. Doing these operations with these known problematic types will
not trigger [xUnit2013](xUnit2013) (unlike other collections which do).

## Examples

### Violates

```csharp
using System;
using Xunit;

public class xUnit2028
{
    [Fact]
    public void TestMethod()
    {
        var arraySegment = new ArraySegment<int>();

        Assert.Empty(arraySegment);
    }
}
```

```csharp
using Microsoft.Extensions.Primitives;
using Xunit;

public class xUnit2028
{
    [Fact]
    public void TestMethod()
    {
        var stringValues = StringValues.Empty;

        Assert.Empty(stringValues);
    }
}
```

### Does not violate

```csharp
using System;
using Xunit;

public class xUnit2028
{
    [Fact]
    public void TestMethod()
    {
        var arraySegment = new ArraySegment<int>();

        Assert.Equal(0, arraySegment.Count);
    }
}
```

```csharp
using Microsoft.Extensions.Primitives;
using Xunit;

public class xUnit2028
{
    [Fact]
    public void TestMethod()
    {
        var stringValues = StringValues.Empty;

        Assert.Equal(0, stringValues.Count);
    }
}
```
