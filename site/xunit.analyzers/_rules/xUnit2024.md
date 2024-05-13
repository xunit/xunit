---
title: xUnit2024
description: Do not use boolean asserts for simple equality tests
category: Assertions
severity: Info
---

## Cause

You should not use boolean assertions (like `Assert.True` or `Assert.False`) with simple equality comparisons
against literal values like `null`, numeric constants, or enum values.

## Reason for rule

The error message provided when the assertion fails is less useful, since it merely indicates that you expected
a value to be `true` or `false`. Using a better assertion (like `Assert.Equal`) will provide a better user
experience, because it will show you the actual values in question when the comparison fails.

## How to fix violations

To fix a violation of this rule, replace the boolean assertion with one more appropriate (including `Assert.Equal`,
`Assert.NotEqual`, `Assert.Null`, or `Assert.NotNull`);

## Examples

### Violates

```csharp
using Xunit;

public class xUnit2024
{
    [Fact]
    public void TestMethod()
    {
        var x = 42;

        Assert.True(x == 2112);
    }
}
```

```csharp
using Xunit;

public class xUnit2024
{
    [Fact]
    public void TestMethod()
    {
        var x = new object();

        Assert.True(x != null);
    }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit2024
{
    [Fact]
    public void TestMethod()
    {
        var x = 42;

        Assert.Equal(2112, x);
    }
}
```

```csharp
using Xunit;

public class xUnit2024
{
    [Fact]
    public void TestMethod()
    {
        var x = new object();

        Assert.NotNull(x);
    }
}
```
