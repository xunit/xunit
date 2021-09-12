---
title: xUnit2012
description: Do not use Enumerable.Any() to check if a value exists in a collection
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when `Enumerable.Any` is used to check if a value matching a predicate exists in a collection.

## Reason for rule

There are specialized assertions for checking for elements in collections.

## How to fix violations

Replace `Assert.True` with `Assert.Contains` and/or `Assert.False` with `Assert.DoesNotContain`.

## Examples

### Violates

```csharp
IEnumerable<string> result = GetItems();

Assert.True(result.Any(x => x == "foo"));
```

### Does not violate

```csharp
IEnumerable<string> result = GetItems();

Assert.Contains(result, x => x == "foo");
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2012 // Do not use Enumerable.Any() to check if a value exists in a collection
#pragma warning restore xUnit2012 // Do not use Enumerable.Any() to check if a value exists in a collection
```
