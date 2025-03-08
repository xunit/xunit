---
title: xUnit3002
description: Classes which are JSON serializable should not be tested for their concrete type
category: Extensibility
severity: Warning
v2: false
v3: true
---

## Cause

A violation of this rule occurs when a developer is dependent on the concrete type of a JSON serializable object
(usually a message sink message implementation).

## Reason for rule

Types which are JSON serializable are encouraged to use interfaces to represent them, because they are typically
backed by different concrete types depending on whether they exist on the execution side or the runner side.

The message sink messages in the v3 Core Framework follow this pattern: there are concrete versions that live in
`xunit.v3.core` (in the `Xunit.v3` namespace) that support serialization only, and versions that live in
`xunit.v3.runner.common` (in the `Xunit.Runner.Common` namespace) that support both serialization and deserialization.
They implement a common interface that lives in `xunit.v3.common` (in the `Xunit.Sdk` namespace) that should be
used instead.

For example, you should reference `IDiscoveryComplete` rather than either `DiscoveryComplete` implementation in all
cases other than creating new instances of the message.

## How to fix violations

To fix a violation of this rule, reference the class by its primary interface instead of its concrete type.

## Examples

### Violates

```csharp
using System;
using Xunit.Sdk;
using Xunit.v3;

public class Xunit3002 : IMessageSink
{
    public bool OnMessage(IMessageSinkMessage message)
    {
        if (message is DiscoveryComplete)
            Console.WriteLine("Discovery is finished!");

        return true;
    }
}
```

### Does not violate

```csharp
using System;
using Xunit.Sdk;

public class Xunit3002 : IMessageSink
{
    public bool OnMessage(IMessageSinkMessage message)
    {
        if (message is IDiscoveryComplete)
            Console.WriteLine("Discovery is finished!");

        return true;
    }
}
```
