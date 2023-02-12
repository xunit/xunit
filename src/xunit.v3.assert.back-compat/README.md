These projects are here to validate backward compatibility, since the assertion library currently
supports both xUnit.net v2 and v3 users, and has several optionally enabled features.

All back-compat projects are compiled with .NET Standard 1.1, as that's the bar set by xUnit.net v2.
The minimum C# language version is 6 (which supports `XUNIT_SPAN` and `XUNIT_VALUETASK`), as this is
what shipped with Visual Studio 2015. To compile for `XUNIT_NULLABLE`, you need C# language version 9
(nullable was introduced in C# 8, but we use constraints that were introduced in C# 9).

_**All assertion code that is written outside the context of `XUNIT_NULLABLE` must be compilable
by C# 6.**_

The following projects exist for back-compat testing:

| Project                                | Language | Flags                                             |
| -------------------------------------- | -------- | ------------------------------------------------- |
| `xunit.v3.assert.cs6.csproj`           | C# 6     | _None_                                            |
| `xunit.v3.assert.cs6.span.csproj`      | C# 6     | `XUNIT_SPAN`                                      |
| `xunit.v3.assert.cs6.valuetask.csproj` | C# 6     | `XUNIT_VALUETASK`                                 |
| `xunit.v3.assert.cs9.nullable.csproj`  | C# 9     | `XUNIT_NULLABLE`                                  |
| `xunit.v3.assert.cs9.on-all.csproj`    | C# 9     | `XUNIT_NULLABLE`, `XUNIT_SPAN`, `XUNIT_VALUETASK` |

For a list of language features, see:
[C# version 6](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history#c-version-60),
[C# version 9](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history#c-version-9)
