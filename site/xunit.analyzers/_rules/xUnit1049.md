---
title: xUnit1049
description: Do not use 'async void' for test methods as it is no longer supported
category: Usage
severity: Error
v2: false
v3: true
---

## Cause

A violation of this rule occurs when an async test method returns `void`.

## Reason for rule

Support for `async void` test methods has been removed in xUnit.net v3. This rule will only trigger
for v3 projects.

## How to fix violations

To fix a violation of this rule, change the test method return type to `Task` or `ValueTask`.

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

```csharp
using System.Threading.Tasks;
using Xunit;

public class TestClass
{
    [Fact]
    public async ValueTask TestMethod()
    {
        // ...
    }
}
```
