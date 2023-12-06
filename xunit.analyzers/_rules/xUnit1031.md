---
title: xUnit1031
description: Do not use blocking task operations in test method
category: Usage
severity: Warning
---

## Cause

Developers should not call blocking operations on `Task` and `ValueTask` inside a unit tests.

## Reason for rule

Calling blocking operations on async types can cause deadlocks, as unit tests run on their own special pool of threads
that are limited by the user. Calling a blocking operation means any other async work must take place on a different
thread, which could exhaust the thread pool and end up causing a deadlock when there are no free threads to process
the work on. Even in cases where deadlocks don't occur, you are reducing performance by consuming an extra thread to
process work.

This only affects test methods marked with `[Fact]` or `[Theory]`. It does not apply to any third party test methods
or test any non-test methods.

## How to fix violations

To fix a violation of this rule, mark your unit test as `async` and use `await`.

## Examples

### Violates

```csharp
using System.Threading.Tasks;
using Xunit;

public class xUnit1031
{
    [Fact]
    public void TestMethod()
    {
        Task.Delay(1).Wait();
    }
}
```

### Does not violate

```csharp
using System.Threading.Tasks;
using Xunit;

public class xUnit1031
{
    [Fact]
    public async Task TestMethod()
    {
        await Task.Delay(1);
    }
}
```

## Disabled parallelization

If you have disabled parallelization for the entire test assembly, you can disable this warning as it no longer
applies to your project.

You can disable it via a global attribute:

```csharp
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("xUnit", "xUnit1031", Justification = "Parallelization is disabled")]
```

Or you can disable it by adding the following to a project-level `.editorconfig` file:

```ini
[*.cs]
dotnet_diagnostic.xUnit1031.severity = none
```
