---
title: xUnit1004
description: Test methods should not be skipped
category: Usage
severity: Info
v2: true
v3: true
---

## Cause

Tests should not be skipped long term. This analyzer highlights skipped tests for better visibility.

## Reason for rule

Skipping tests should be considered a temporary mechanism, and as such, tests which are skipped are highlighted so they're easier to see and to allow the developer to fix the issue.

For v3 test projects, using `SkipWhen` or `SkipUnless` suppresses this analyzer, since that's used to conditionally skip tests based on the testing environment.

## How to fix violations

To fix a violation of this rule, you will typically employ one of two strategies:

* If the test is temporarily skipped because it's broken or flaky, then repair the test and un-skip it.
* If the test is deemed unreliable or unnecessary, the best strategy may be to just delete the test.

## Examples

### Violates

#### For v2 and v3

```csharp
using Xunit;

public class xUnit1004
{
    [Fact(Skip = "This is a flaky test")]
    public void TestMethod() { }
}
```

### Does not violate

#### For v2 and v3

```csharp
using Xunit;

public class xUnit1004
{
    [Fact]
    public void TestMethod() { }
}
```

#### For v3 only

```csharp
using System;
using Xunit;

public class xUnit1004
{
    [Fact(Skip = "This test requires Windows",
          SkipUnless = nameof(OperatingSystem.IsWindows),
          SkipType = typeof(OperatingSystem))]
    public void TestMethod() { }
}
```
