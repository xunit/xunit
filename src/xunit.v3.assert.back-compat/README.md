These projects are here to validate backward compatibility, since the assertion library currently
supports both xUnit.net v2 and v3 users, and has several optionally enabled features.

All back-compat projects are compiled with .NET Standard 1.1, as that's the bar set by xUnit.net v2.
C# language version is 6 for `XUNIT_SPAN` and `XUNIT_VALUETASK`, and 7.3 for `XUNIT_NULLABLE`.
The following projects exist for back-compat testing:

| Project                                | Language | Flags                                             |
| -------------------------------------- | -------- | ------------------------------------------------- |
| `xunit.v3.assert.cs6.csproj`           | C# 6     | _None_                                            |
| `xunit.v3.assert.cs6.span.csproj`      | C# 6     | `XUNIT_SPAN`                                      |
| `xunit.v3.assert.cs6.valuetask.csproj` | C# 6     | `XUNIT_VALUETASK`                                 |
| `xunit.v3.assert.cs8.nullable.csproj`  | C# 8     | `XUNIT_NULLABLE`                                  |
| `xunit.v3.assert.cs8.on-all.csproj`    | C# 8     | `XUNIT_NULLABLE`, `XUNIT_SPAN`, `XUNIT_VALUETASK` |
