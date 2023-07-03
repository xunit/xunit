---
title: xUnit1029
description: Local functions cannot be test functions
category: Usage
severity: Error
---

## Cause

A test method must be directly inside a class. Local functions (that is, functions defined inside other code blocks)
are not supported.

## Reason for rule

xUnit.net does not look for local functions to find tests, as they are not supported.

## How to fix violations

To fix a violation of this rule, move the test function to class-level.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit1029
{
    private void NonTestMethod()
    {
        [Fact]
        void TestMethod()
        {
            // ...
        }
    }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit1029
{
    [Fact]
    void TestMethod()
    {
        // ...
    }

    private void NonTestMethod()
    {
        // ...
    }
}
```
