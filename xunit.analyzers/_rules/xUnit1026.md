---
title: xUnit1026
description: Theory methods should use all of their parameters
category: Usage
severity: Warning
---

## Cause

This rule is triggered by having an unused parameter on your `[Theory]`.

## Reason for rule

Unused parameters are typically an indication of some kind of coding error (typically a parameter that was previously used and is no longer required, or a parameter that was overlooked in the test implementation).

<div class="note">As of <code>xunit.analyzers</code> version <code>1.2</code>, this rule will no longer trigger with parameters that are named as discards (meaning, starting with <code>_</code> and optionally 1 or more numbers). For more information on discard parameter names, see <a href="https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0060">IDE0060</a>.</div>

## How to fix violations

To fix a violation of this rule, you may:

* Remove the unused parameter (and all associated data)
* Rename the parameter to a discard name
* Use the unused parameter

## Examples

### Violates

```csharp
using Xunit;

static class MyGreetingService
{
    public static string Greet(string name) =>
        $"Hello, {name}!";
}

public class xUnit1026
{
    [Theory]
    [InlineData("Joe", 42)]
    public void ValidateGreeting(string name, int age)
    {
        var result = MyGreetingService.Greet(name);

        Assert.Equal("Hello, Joe!", result);
    }
}
```

### Does not violate

```csharp
using Xunit;

static class MyGreetingService
{
    public static string Greet(string name) =>
        $"Hello, {name}!";
}

public class xUnit1026
{
    [Theory]
    [InlineData("Joe", 42)]
    public void ValidateGreeting(string name)
    {
        var result = MyGreetingService.Greet(name);

        Assert.Equal("Hello, Joe!", result);
    }
}
```

```csharp
using Xunit;

static class MyGreetingService
{
    public static string Greet(string name) =>
        $"Hello, {name}!";
}

public class xUnit1026
{
    [Theory]
    [InlineData("Joe", 42)]
    public void ValidateGreeting(string name, int _)
    {
        var result = MyGreetingService.Greet(name);

        Assert.Equal("Hello, Joe!", result);
    }
}
```

```csharp
using Xunit;

static class MyGreetingService
{
    public static string Greet(string name, int age) =>
        $"Hello, {name}! How do you like being {age} years old?";
}

public class xUnit1026
{
    [Theory]
    [InlineData("Joe", 42)]
    public void ValidateGreeting(string name, int age)
    {
        var result = MyGreetingService.Greet(name, age);

        Assert.Equal("Hello, Joe! How do you like being 42 years old?", result);
    }
}
```
