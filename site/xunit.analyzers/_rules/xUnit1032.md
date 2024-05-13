---
title: xUnit1032
description: Test classes cannot be nested within a generic class
category: Usage
severity: Error
---

## Cause

Test classes cannot be nested within a generic class.

## Reason for rule

Generic types must be instantiated with types provided, which xUnit.net cannot provide. Test classes embedded within
generic types, therefore, cannot be instantiated or executed.

## How to fix violations

To fix a violation of this rule, move the test class outside the scope of the generic type.

## Examples

### Violates

```csharp
using Xunit;

public class GenericClass<T>
{
    public class TestClass
    {
        [Fact]
        public void TestMethod() { }
    }
}
```

### Does not violate

```csharp
using Xunit;

public class GenericClass<T>
{
}

public class TestClass
{
    [Fact]
    public void TestMethod() { }
}
```
