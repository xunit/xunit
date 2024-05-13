---
title: xUnit2026
description: Comparison of sets must be done with IEqualityComparer
category: Assertions
severity: Warning
---

## Cause

This rule is triggered by the use of an equality assertion with two sets, using an item comparison function.

## Reason for rule

Unlike linear containers (arrays, lists, etc.), sets do not have a natural order to them. A simple item comparison function
is not sufficient, due to the way sets define item equality.

For more information, see [Equality with hash sets vs. linear containers](/docs/hash-sets-vs-linear-containers).

## How to fix violations

In order for set item comparisons to perform properly, they need both an implementation of `Equals` and an
implementation of `GetHashCode`. More importantly, any two values which return `true` for `Equals` must also
return the same value for `GetHashCode`. Because both of these functions need to be defined, and because
the implementation of `GetHashCode` cannot be inferred from the equality function, you are required to
provide both pieces of code. This is fundamental to how sets work.

If the item in the set is a custom class that you control, then you can override the implementation
of both `Equals` and `GetHashCode` on the custom class. These functions will be used by the set when you
don't have a custom item comparer. This is the recommended way to do this when you control the definition
of the item type.

If you don't control the definition of the item being placed into the set, then you need to implement a custom
[`IEqualityComparer<T>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.iequalitycomparer-1)
for the type in question. Once you have the custom comparer, there are two ways you can use it. The preferred
way is to pass it to the set during construction so that it will be used for all comparison operations. Doing
this means you don't have to pass the comparer to the assertion function.

If you don't control the creation of the set, the final way to make this work is to pass the custom item comparer
to the assertion function. Note that in this case, the assertion library must make new copies of the `expected`
and `actual` sets using the new comparer, and copy all the items into the new sets. This means the assertion will
be slower and use more memory (how much slower and how much more memory depends on the size of the sets in question,
among other variables).

**Important debugging note:** If your tests are failing when you think they should be passing, please double
check your `GetHashCode` implementation to ensure equal values have equal hash codes. In the samples below,
where our equality test is case-insensitive, we make sure to pass uppercase normalized values into the hash
code combiner function; if we just passed the original values, then `Brad` and `BRAD` would have different
hash code values, so the set would not consider them to be equal. Sets always consult hash codes _before_
equality functions to determine item equality.

## Examples

### Violates

```csharp
using System;
using System.Collections.Generic;
using Xunit;

public class Person(string firstName, string lastName)
{
    public string FirstName { get; } = firstName;
    public string LastName { get; } = lastName;
}

public class xUnit2026
{
    [Fact]
    public void TestMethod()
    {
        var set1 = new HashSet<Person> { new("Brad", "Wilson") };
        var set2 = new HashSet<Person> { new("BRAD", "WILSON") };

        Assert.Equal(
            set1, set2,
            (Person x, Person y) =>
                x.FirstName.Equals(y.FirstName, StringComparison.CurrentCultureIgnoreCase)
                && x.LastName.Equals(y.LastName, StringComparison.CurrentCultureIgnoreCase)
        );
    }
}
```

### Does not violate

#### Override `Equals` and `GetHashCode` on the item

```csharp
using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

public class Person(string firstName, string lastName)
{
    public string FirstName { get; } = firstName;
    public string LastName { get; } = lastName;

    public override bool Equals(object? obj)
    {
        if (obj is not Person other)
            return false;

        return FirstName.Equals(other.FirstName, StringComparison.CurrentCultureIgnoreCase)
            && LastName.Equals(other.LastName, StringComparison.CurrentCultureIgnoreCase);
    }

    public override int GetHashCode() =>
        HashCode.Combine(
            FirstName.ToUpper(CultureInfo.CurrentCulture),
            LastName.ToUpper(CultureInfo.CurrentCulture)
        );
}

public class xUnit2026
{
    [Fact]
    public void TestMethod()
    {
        var set1 = new HashSet<Person> { new("Brad", "Wilson") };
        var set2 = new HashSet<Person> { new("BRAD", "WILSON") };

        Assert.Equal(set1, set2);
    }
}
```

#### Pass the comparer to the set during construction

```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Xunit;

public class Person(string firstName, string lastName)
{
    public string FirstName { get; } = firstName;
    public string LastName { get; } = lastName;
}

public class PersonComparer : IEqualityComparer<Person>
{
    public bool Equals(Person? x, Person? y)
    {
        if (x is null)
            return y is null;
        if (y is null)
            return false;
        return x.FirstName.Equals(y.FirstName, StringComparison.CurrentCultureIgnoreCase)
            && x.LastName.Equals(y.LastName, StringComparison.CurrentCultureIgnoreCase);
    }

    public int GetHashCode([DisallowNull] Person obj) =>
        HashCode.Combine(
            obj.FirstName.ToUpper(CultureInfo.CurrentCulture),
            obj.LastName.ToUpper(CultureInfo.CurrentCulture)
        );
}

public class xUnit2026
{
    [Fact]
    public void TestMethod()
    {
        var comparer = new PersonComparer();
        var set1 = new HashSet<Person>(comparer) { new("Brad", "Wilson") };
        var set2 = new HashSet<Person>(comparer) { new("BRAD", "WILSON") };

        Assert.Equal(set1, set2);
    }
}
```

#### Pass the comparer to the assertion

```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Xunit;

public class Person(string firstName, string lastName)
{
    public string FirstName { get; } = firstName;
    public string LastName { get; } = lastName;
}

public class PersonComparer : IEqualityComparer<Person>
{
    public bool Equals(Person? x, Person? y)
    {
        if (x is null)
            return y is null;
        if (y is null)
            return false;
        return x.FirstName.Equals(y.FirstName, StringComparison.CurrentCultureIgnoreCase)
            && x.LastName.Equals(y.LastName, StringComparison.CurrentCultureIgnoreCase);
    }

    public int GetHashCode([DisallowNull] Person obj) =>
        HashCode.Combine(
            obj.FirstName.ToUpper(CultureInfo.CurrentCulture),
            obj.LastName.ToUpper(CultureInfo.CurrentCulture)
        );
}

public class xUnit2026
{
    [Fact]
    public void TestMethod()
    {
        var comparer = new PersonComparer();
        var set1 = new HashSet<Person> { new("Brad", "Wilson") };
        var set2 = new HashSet<Person> { new("BRAD", "WILSON") };

        Assert.Equal(set1, set2, comparer);
    }
}
```
