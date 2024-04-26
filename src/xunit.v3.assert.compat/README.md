These projects are here to validate compatibility (mostly backward), since the assertion library currently
supports both xUnit.net v2 and v3 users, and has several optionally enabled features.

All compatibility projects are compiled with .NET Standard 1.1 (except the .NET 6 project), as that's the bar
set by xUnit.net v2. The minimum C# language version is 6 (which supports `XUNIT_IMMUTABLE_COLLECTIONS` and
`XUNIT_SPAN`), as this is what shipped with Visual Studio 2015. To compile for `XUNIT_NULLABLE`, you need C#
language version 9 (nullable was introduced in C# 8, but we use constraints that were introduced in C# 9).

The .NET 6 project exists because there are assertions to support `IReadOnlySet<T>`, which is a type that was
introduced after .NET Standard 2.0 (which is the target for v3). To access these assertions, you must import the
assertion library via source and target .NET 5 or later (we test with .NET 6, since .NET 5 is no longer a supported
version). To find these assertions and tests, search for `#if NET5_0_OR_GREATER` in the source code.

_**All assertion code that is written outside the context of `XUNIT_NULLABLE` must be compilable by C# 6.**_

The following projects exist for compatibility testing:

| Project                                  | Language | Flags                                                               |
| ---------------------------------------- | -------- | ------------------------------------------------------------------- |
| `xunit.v3.assert.all-off.csproj`         | C# 6     | _None_                                                              |
| `xunit.v3.assert.all-on.csproj`          | C# 9     | `XUNIT_IMMUTABLE_COLLECTIONS`<br/>`XUNIT_NULLABLE`<br/>`XUNIT_SPAN` |
| `xunit.v3.assert.immutable.csproj`       | C# 6     | `XUNIT_IMMUTABLE_COLLECTIONS`                                       |
| `xunit.v3.assert.nullable.csproj`        | C# 9     | `XUNIT_NULLABLE`                                                    |
| `xunit.v3.assert.nullable-mixed.csproj`  | C# 9     | _None_ (used to test `#nullable enable` without `XUNIT_NULLABLE`)   |
| `xunit.v3.assert.span.csproj`            | C# 6     | `XUNIT_SPAN`                                                        |

For a list of language features, see:
[C# version 6](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history#c-version-60),
[C# version 9](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history#c-version-9)
