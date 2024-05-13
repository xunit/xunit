---
title: xUnit1028
description: Test method must have valid return type
category: Usage
severity: Error
---

## Cause

A test method must have a valid return type.

For v2 tests, that includes `void` and `Task`. For v3 tests, you can also use `ValueTask`.

## Reason for rule

xUnit.net will not run a test method with the wrong return type.

## How to fix violations

To fix a violation of this rule, use one of the supported return types.

## Examples

### Violates

All versions:

```csharp
using Xunit;

public class xUnit1028
{
    [Fact]
    public int TestMethod()
    {
        return 42;
    }
}
```

v2 only:

```csharp
using System.Threading.Tasks;
using Xunit;

public class xUnit1028
{
    [Fact]
    public ValueTask TestMethod()
    {
        return default(ValueTask);
    }
}
```

### Does not violate

All versions:

```csharp
using Xunit;

public class xUnit1028
{
    [Fact]
    public void TestMethod()
    {
    }
}
```

```csharp
using System.Threading.Tasks;
using Xunit;

public class xUnit1028
{
    [Fact]
    public Task TestMethod()
    {
        return Task.CompletedTask;
    }
}
```

v3 or later:

```csharp
using System.Threading.Tasks;
using Xunit;

public class xUnit1028
{
    [Fact]
    public ValueTask TestMethod()
    {
        return default(ValueTask);
    }
}
```
