---
title: xUnit1048
description: Avoid using 'async void' for test methods as it is deprecated in xUnit.net v3
category: Usage
severity: Warning
v2: true
v3: false
---

## Cause

A violation of this rule occurs when an async test method returns `void`.

## Reason for rule

Support for `async void` test methods is being removed in xUnit.net v3. To ease upgrading from
v2 to v3, convert the test method to return `Task` instead of `void`. This rule will only trigger
for v2 projects.

## How to fix violations

To fix a violation of this rule, change the test method return type to `Task`.

## Examples

### Violates

```csharp
using Xunit;

public class TestClass
{
    [Fact]
    public async void TestMethod()
    {
        // ...
    }
}
```

### Does not violate

```csharp
using System.Threading.Tasks;
using Xunit;

public class TestClass
{
    [Fact]
    public async Task TestMethod()
    {
        // ...
    }
}
```
