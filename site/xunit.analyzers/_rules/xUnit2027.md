---
title: xUnit2027
description: Comparison of sets to linear containers have undefined results
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when calling an equality assertion with a set and a linear container.

## Reason for rule

Sets do not have a predefined order when enumerating their contents, so comparing them against a linear container
(like an array or list) can cause false negatives/positives, depending on how the set data is enumerated. The most
common tipping point causing unpredictably tends to be when containers grow larger in size.

For more information, see [Equality with hash sets vs. linear containers](/docs/hash-sets-vs-linear-containers).

## How to fix violations

Create an order to the set data. A common way to do this is to use `OrderBy` from LINQ.

## Examples

### Violates

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit2027
{
    [Fact]
    public void TestMethod()
    {
        var expected = new List<int> { 42, 2112 };
        var actual = new HashSet<int> { 42, 2112 };

        Assert.Equal(expected, actual);
    }
}
```

### Does not violate

```csharp
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class xUnit2027
{
    [Fact]
    public void TestMethod()
    {
        var expected = new List<int> { 42, 2112 };
        var actual = new HashSet<int> { 42, 2112 };

        Assert.Equal(expected, actual.OrderBy(x => x));
    }
}
```
