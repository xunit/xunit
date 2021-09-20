---
title: xUnit1027
description: Collection definition classes must be public
category: Usage
severity: Error
---

## Cause

A collection definition class is not public.

## Reason for rule

xUnit.net will not discover the collection definition class if the class is not public.

## How to fix violations

To fix a violation of this rule, make the collection definition class public.

## Examples

### Violates

```csharp
using Xunit;

[CollectionDefinition("CollectionName")]
class CollectionDefinitionClass
{ }
```

### Does not violate

```csharp
using Xunit;

[CollectionDefinition("CollectionName")]
public class CollectionDefinitionClass
{ }
```
