---
title: xUnit1004
description: Test methods should not be skipped
category: Usage
severity: Info
---

## Cause

Tests should not be skipped long term. This analyzer highlights skipped tests for better visibility.

## Reason for rule

Skipping tests should be considered a temporary mechanism, and as such, tests which are skipped are highlighted so they're easier to see and to allow the developer to fix the issue.

## How to fix violations

To fix a violation of this rule, you will typically employ one of two strategies:

* If the test is temporarily skipped because it's broken or flaky, then repair the test and un-skip it.
* If the test is deemed unreliable or unnecessary, the best strategy may be to just delete the test.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit1004
{
    [Fact(Skip = "This is a flaky test")]
    public void TestMethod() { }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit1004
{
    [Fact]
    public void TestMethod() { }
}
```
