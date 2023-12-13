---
title: xUnit1030
description: Do not call ConfigureAwait in test method
category: Usage
severity: Warning
---

## Cause

Developers who configure awaiting may cause parallelization issues, like running too many tests in parallel. There
are two ways they could do this: by calling `ConfigureAwait(false)` or by calling the `ConfigureAwait()` overload that
accepts `ConfigureAwaitOptions` without including `ConfigureAwaitOptions.ContinueOnCapturedContext`.

## Reason for rule

Calling `ConfigureAwait` (with `false`, or without `ConfigureAwaitOptions.ContinueOnCapturedContext`) will cause any code
after the awaited task to run on a thread pool thread, which can grow without limit. xUnit.net uses its own special thread
pool to limit the number of tests which can actively run in parallel.

This only affects test methods marked with `[Fact]` or `[Theory]`. It does not apply to any third party test methods
or test any non-test methods.

[CA2007](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2007) forces users to
write `ConfigureAwait` regardless of the situation, so if you've enabled this rule, you may write `.ConfigureAwait(true)`
and this rule will not trigger. However, this comes with at least two side effects. First, the call is not free;
it always allocates at least one (or more) unnecessary objects, even when the net result should be the same, in
addition to spending the time to execute all the involve code. Second, the call is not always transparent; when
used in the context of `await using` of a newly constructed object, the result type from `ConfigureAwait` overwrites
the type of your object, so you will be unable to use the newly constructed object in any way due to the incompatible
type that is returned. _**It is for reasons like these that we strongly recommend you disable CA2007 in your unit test
projects**_, especially when feeling any of the friction involved with this.

## How to fix violations

To fix a violation of this rule, remove the call to `ConfigureAwait`, use a `true` value, or use a `ConfigureAwaitOptions`
value that includes `ConfigureAwaitOptions.ContinueOnCapturedContext`.

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

#### .NET 8 or later

```csharp
using System.Threading.Tasks;
using Xunit;

public class xUnit1030
{
    [Fact]
    public async Task TestMethod()
    {
        await Task.Delay(1).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

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

```csharp
using System.Threading.Tasks;
using Xunit;

public class xUnit1030
{
    [Fact]
    public async Task TestMethod()
    {
        await Task.Delay(1).ConfigureAwait(true);

        // ...code running on xUnit.net parallel execution thread pool thread...
    }
}
```

#### .NET 8 or later

```csharp
using System.Threading.Tasks;
using Xunit;

public class xUnit1030
{
    [Fact]
    public async Task TestMethod()
    {
        await Task.Delay(1).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ContinueOnCapturedContext);

        // ...code running on xUnit.net parallel execution thread pool thread...
    }
}
```
