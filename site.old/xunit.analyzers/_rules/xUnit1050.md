---
title: xUnit1050
description: The class referenced by the ClassData attribute returns untyped data rows
category: Usage
severity: Info
v2: false
v3: true
---

## Cause

xUnit.net v3 offers a strongly-typed `TheoryDataRow<>` class which can be used instead of `object[]`
when returning member data. (Note that `ITheoryDataRow` and the non-generic `TheoryDataRow` are also
untyped, and will cause this rule to trigger.)

## Reason for rule

Using generic `TheoryDataRow<>` provides a type-safe way to provide member data for theories. The compiler
can indicate when you have provided incompatible data, and several other analyzers will help ensure your
`TheoryDataRow<>` type usage matches your test method parameters.

## How to fix violations

To fix a violation of this rule, replace `IEnumerable<object[]>`, `IEnumerable<ITheoryDataRow>`, or
`IEnumerable<TheoryDataRow>` with `IEnumerable<TheoryDataRow<>>`. This also applies to `IAsyncEnumerable`.

## Examples

### Violates

```csharp
using System.Collections;
using System.Collections.Generic;
using Xunit;

public class EnumerableData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return [1];
        yield return [2];
        yield return [3];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class xUnit1050
{
    [Theory]
    [ClassData(typeof(EnumerableData))]
    public void TestMethod(int _) { }
}
```

```csharp
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class AsyncEnumerableData : IAsyncEnumerable<object[]>
{
    public async IAsyncEnumerator<object[]> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        yield return [1];
        yield return [2];
        yield return [3];
    }
}

public class xUnit1050
{
    [Theory]
    [ClassData(typeof(AsyncEnumerableData))]
    public void TestMethod(int _) { }
}
```

### Does not violate

```csharp
using System.Collections;
using System.Collections.Generic;
using Xunit;

public class EnumerableData : IEnumerable<TheoryDataRow<int>>
{
    public IEnumerator<TheoryDataRow<int>> GetEnumerator()
    {
        yield return new(1);
        yield return new(2);
        yield return new(3);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class xUnit1050
{
    [Theory]
    [ClassData(typeof(EnumerableData))]
    public void TestMethod(int _) { }
}
```

```csharp
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class AsyncEnumerableData : IAsyncEnumerable<TheoryDataRow<int>>
{
    public async IAsyncEnumerator<TheoryDataRow<int>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        yield return new(1);
        yield return new(2);
        yield return new(3);
    }
}

public class xUnit1050
{
    [Theory]
    [ClassData(typeof(AsyncEnumerableData))]
    public void TestMethod(int _) { }
}
```
