---
title: xUnit1022
description: Theory methods cannot have a parameter array
category: Usage
severity: Error
---

## Cause

In versions of xUnit.net prior to 2.2.0, `[Theory]` methods did not support `params` arrays.

## How to fix violations

To fix a violation of this rule, upgrade to xUnit.net 2.2.0 or later.
