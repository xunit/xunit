---
title: xUnit2017
description: Do not use Contains() to check if a value exists in a collection
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when `Enumerable.Contains()` is used to check if a value exists in a collection.

## Reason for rule

There are specialized assertions for checking for elements in collections.

## How to fix violations

Use `Assert.Contains` or `Assert.DoesNotContain` instead.

## Examples

### Violates

```csharp
Assert.True(collection.Contains("foo"));
```

```csharp
Assert.False(collection.Contains("bar"));
```


### Does not violate

```csharp
Assert.Contains("foo", result);
```

```csharp
Assert.DoesNotContain("bar", result);
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2017 // Do not use Contains() to check if a value exists in a collection
#pragma warning restore xUnit2017 // Do not use Contains() to check if a value exists in a collection
```
