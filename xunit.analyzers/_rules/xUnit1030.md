---
title: xUnit1030
description: Do not call ConfigureAwait in test method
category: Usage
severity: Warning
---

## Cause

Developers should not call ConfigureAwait against tasks in a test method, as this may cause parallelization issues
like running too many tests in parallel.

## Reason for rule

Calling `ConfigureAwait` (with `false`, specifically) will cause any code after the awaited task to run on a thread
pool thread, which can grow without limit. xUnit.net uses its own special thread pool to limit the number of tests
which can actively run in parallel. Any usage of `ConfigureAwait` in a unit test is suspect code, so all invocations
are marked by the analyzer, regardless of the value that's passed.

This only affects test methods marked with `[Fact]` or `[Theory]`. It does not apply to any third party test methods
or test any non-test methods.

## How to fix violations

To fix a violation of this rule, remove the call to `ConfigureAwait`.

## Examples

### Violates

```csharp
using System.Threading.Tasks;
using Xunit;

public class xUnit1030
{
    [Fact]
    public async Task TestMethod()
    {
        await Task.Delay(1).ConfigureAwait(false);

        // ...code running on thread pool thread...
    }
}
```

### Does not violate

```csharp
using System.Threading.Tasks;
using Xunit;

public class xUnit1030
{
    [Fact]
    public async Task TestMethod()
    {
        await Task.Delay(1);

        // ...code running on xUnit.net parallel execution thread pool thread...
    }
}
```
