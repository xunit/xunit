---
layout: default
title: Query Filter Language
breadcrumb: Documentation
---

# Query Filter Language

_Last updated: 2024 December 16_

New in v3 is support for an advanced query filter language. It is inspired by the [MSTest Graph Query Filter](https://github.com/microsoft/testfx/blob/main/docs/mstest-runner-graphqueryfiltering/graph-query-filtering.md), but with several differences in final implementation.

The intention of the query filter language is to allow a more powerful and flexible way to filter tests than the existing simple filters (which allow filtering based on namespace, fully qualified class names, fully qualified method names, and traits).

_**Note:** In order to better reason about how filtering works, you may use either query filters or simple filters, but you may not use both at the same time._


## Query filter language

Below is a list of components of the query language.

_**Note:** All string comparisons are done case insensitively. That means `/foo` and `/FOO` are the same query._

### Query is segmented

The query filter supported in xUnit.net v3 is structured with between one and four segments:

`/<assemblyFilter>/<namespaceFilter>/<classFilter>/<methodFilter>`

The four segments represent the four parts of a fully qualified test method in an assembly. For the purposes of illustration, let's assume a test method of `MyNamespace.MySubNamespace.MyClass+MySubClass.MyTestMethod` that lives in `C:\Dev\MyProjects\Tests\bin\Debug\net8.0\MyTests.dll`:

- `assemblyFilter` is matched against the test assembly name (i.e., `MyTests`)
- `namespaceFilter` is matched against the namespace of the test (i.e., `MyNamespace.MySubNamespace`)
- `classFilter` is matched against the name of the test class (i.e., `MyClass+MySubClass`)
- `methodFilter` is matched against the name of the test method (i.e., `MyTestMethod`)

> Example:
>
> `/MyTests/MyNamespace.MySubNamespace/MyClass+MySubClass/MyTestMethod` will match the example test method above.

### Use a wildcard in a segment to indicate a "match all"

For any segment, you can use `*` to represent a "match all".

In addition, any segment left off the query is considered to be an implicit "match all".

> Examples:
>
> - `/` is equivalent to `/*`, `/*/*`, `/*/*/*`, and `/*/*/*/*`.
> - `/MyAssembly` is equivalent to `/MyAssembly/*`, `/MyAssembly/*/*`, and `/MyAssembly/*/*/*`.
> - `/MyAssembly/MyNamespace` is equivalent to `/MyAssembly/MyNamespace/*` and `/MyAssembly/MyNamespace/*/*`.
> - etc.

_**Note:** Queries must always start with `/`, even if you are not specifying any segment values. Specifying more than four segments results in a parsing error for the query._

### Use a trait filter

To filter based on a trait, add an appropriate trait expression to the end of your query, in the form of `[name=value]` or `[name!=value]`.

> Examples:
>
> - `/[category=slow]` means "run all tests that have a trait named `category` with a value named `slow`.
> - `/[category!=slow]` means "run all tests that **do not** have a trait named `category` with a value named `slow`.

### Use wildcards to start/end a query expression

You may start and/or end a query expression with `*` to indicate a wildcard that matches 0 or more characters. This includes both segment query expressions as well as the name and/or value portion of a trait filter.

> Query segment examples:
>
> - `query` means "match exactly against `query`"
>   - &#x2705; `query` (matches)
>   - &#x274c; `1query` (does not match)
>   - &#x274c; `query2` (does not match)
>   - &#x274c; `1query2` (does not match)
> - `query*` means "match against anything that starts with `query`"
>   - &#x2705; `query` (matches)
>   - &#x274c; `1query` (does not match)
>   - &#x2705; `query2` (matches)
>   - &#x274c; `1query2` (does not match)
> - `*query` means "match against anything that ends with `query`"
>   - &#x2705; `query` (matches)
>   - &#x2705; `1query` (matches)
>   - &#x274c; `query2` (does not match)
>   - &#x274c; `1query2` (does not match)
> - `*query*` means "match against anything that contains `query`"
>   - &#x2705; `query` (matches)
>   - &#x2705; `1query` (matches)
>   - &#x2705; `query2` (matches)
>   - &#x2705; `1query2` (matches)
>
> Trait filter example:
>
> - `[*name*=*value*]` means "match against a trait whose name contains `name` and whose value contains `value`

_**Note:** The `*` wildcard is only allowed at the start or end, and not in the middle, of any query._

_**Note:** In v3, all simple queries have been updated to support this same start and/or end wildcards, including using wildcards for trait names and/or values. In v2, only the simple method query supported wildcards._

### Combine multiple queries in a single segment

Within a single segment (or within a trait query), you may combine multiple matching patterns by using parenthesis, separated by either `|` (for OR) or `&` (for AND). Parenthesis in this situation are _not optional_.

If your multipart query contains three or more pieces, they must either be all the same type (OR vs. AND), or must use extra parenthesis to indicate how the operators are to be grouped. A query like `(A)|(B)|(C)` is legal, but `(A)|(B)&(C)` is not and must be expressed either as `((A)|(B))&(C)` or `(A)|((B)&(C))`.

Multipart queries cannot span across segment boundaries.

> Examples:
>
> - `/*/*/*/(False)|(True)` means "run all tests whose test method is named either `False` or `True`".
> - `/[(name1=value1)&(name2=value2)]` means "run all tests with both traits `name1=value1` and `name2=value2`.
> - `/[((name1=value1a)|(name1=value1b))&(name2!=value2)]` means "run all tests with either traits `name1=value1a` or `name1=value1b`, and also with no trait matching `name2=value2`.

### Negate a segment query

You may negate a segment query by prepending `!` on the segment filter expression.

_**Note:** If you are using wildcard expressions, the negate operator comes before the wildcard (i.e., `!*`, not `*!`). If you are combining multiple queries in a single segment, the negate operator is placed inside the parenthesis, not outside (i.e., `(!expression)`, not `!(expression)`)._

> Examples:
>
> - `/*/!*foo` means "run all tests whose namespace does not end in `foo`"
> - `/*/*/(Foo*)&(!*Bar)` means "run all tests whose class name starts with `Foo` and does not end with `Bar`

_**Note:** Negating a trait query is done by using `[name!=value]`, not `![name=value]`._

### Escaping special characters

You can escape special characters (like `(` and `)`) and any other character your terminal might not directly support by encoding them using the hexadecimal HTML character encoding scheme. Escape the value with `&#x1234;` where `1234` is the 1-4 digit hex code for a UTF-16 character.

> Commonly escaped special characters:
>
> * `!` is `&#x21;`
> * `(` is `&#x28;`
> * `)` is `&#x29;`
> * `/` is `&#x2f;`
> * `=` is `&#x3d;`
> * `[` is `&#x5b;`
> * `]` is `&#x5d;`

## Specifying a query filter

The way you pass a query filter depends on whether you're interacting with an xUnit.net CLI or not.

If you specify more than one query filter, they run in a logical OR mode; that is, tests which may any one of the filters will match.

_**Note:** If you are using `dotnet run` or `dotnet test` to run your tests, passing command line options must be done after you pass `--` first, so that the `dotnet` executable knows how to differentiate between command line switches meant for `dotnet` (before the `--`) vs. command line switches meant for the test project (after the `--`)._

### Running a test project directly (xUnit.net CLI mode), via `dotnet run`, or via `xunit.v3.console.exe`

You can specify a query filter using `-filter expression` (note that if the expression contains spaces, you should quote the whole expression, like `-filter "expression"` so that the parser knows where your expression starts and ends).

If you're using `xunit.v3.console.exe` and you're running multiple test projects, then the query is passed to all test projects. Ensure that your query filter includes an assembly name filter segment if the filter is only intended to match tests from a subset of the test projects.

> Examples:
>
> * `bin/Debug/net8.0/MyTests.exe -filter /[category=fast]` (xUnit.net CLI mode)
> * `dotnet run -- -filter /[category=fast]`
> * `/path/to/xunit.v3.console.exe bin/Debug/net8.0/MyTests.dll -filter /[category=fast]`

### Running a test project directly (Microsoft Testing Platform CLI mode) or via `dotnet test`

You can specify a query filter using `--query-filter expression` (note that if the expression contains spaces, you should quote the whole expression, like `--query-filter "expression"` so that the parser knows where your expression starts and ends).

> Examples:
>
> * `bin/Debug/net8.0/MyTests.exe --query-filter /[category=fast]` (Microsoft Testing Platform CLI mode)
> * `dotnet test -- --query-filter /[category=fast]`
