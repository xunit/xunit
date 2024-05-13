---
title: xUnit2021
description: Async assertions should be awaited
category: Assertions
severity: Error
---

## Cause

If you use an async assertion, you must await the result.

## Reason for rule

Calling an async assertion without awaiting the result will result in false positives, because the test framework cannot
follow the result of the async operation (and any errors will be thrown away).

## How to fix violations

To fix a violation of this rule, await the result of the async assertion.

## Examples

### Violates

```csharp
using System;
using Xunit;

public class xUnit2021
{
    [Fact]
    public void TestMethod()
    {
        Assert.ThrowsAsync<DivideByZeroException>(async () => throw new DivideByZeroException());
    }
}
```

### Does not violate

```csharp
using System;
using Xunit;

public class xUnit2021
{
    [Fact]
    public async Task TestMethod()
    {
        await Assert.ThrowsAsync<DivideByZeroException>(async () => throw new DivideByZeroException());
    }
}
```
