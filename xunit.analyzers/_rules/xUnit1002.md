---
title: xUnit1002
description: Test methods cannot have multiple Fact or Theory attributes
category: Usage
severity: Error
---

## Cause

A test method has multiple Fact or Theory attributes.

## Reason for rule

A test method only needs one Fact or Theory attribute.

## How to fix violations

To fix a violation of this rule, remove all but one of the Fact or Theory attributes.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit1002
{
    [Fact, Theory]
    public void TestMethod()
    { }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit1002
{
    [Fact]
    public void TestMethod()
    { }
}
```

```csharp
using Xunit;

public class xUnit1002
{
    [Theory]
    public void TestMethod()
    { }
}
```
