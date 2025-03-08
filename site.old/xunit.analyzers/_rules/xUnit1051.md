---
title: xUnit1051
description: Calls to methods which accept CancellationToken should use TestContext.Current.CancellationToken
category: Usage
severity: Warning
v2: false
v3: true
---

## Cause

A violation of this rule occurs when a method that accepts `CancellationToken` is not passed a cancellation token.

## Reason for rule

To provide for orderly cancellation (especially for tests which have timed out), developers should pass a cancellation
token to any method which accepts one. The cancellation token should be `TestContext.Current.CancellationToken` or a
linked token source that includes it.

## How to fix violations

To fix a violation of this rule, pass `TestContext.Current.CancellationToken`.

## Examples

### Violates

```csharp
using System.Threading.Tasks;
using Xunit;

public class xUnit1051
{
    [Fact]
    public async ValueTask TestMethod()
    {
        await Task.Delay(1);
    }
}
```

### Does not violate

```csharp
using System.Threading.Tasks;
using Xunit;

public class xUnit1051
{
    [Fact]
    public async ValueTask TestMethod()
    {
        await Task.Delay(1, TestContext.Current.CancellationToken);
    }
}
```
