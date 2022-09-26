These projects are here to validate backward compatibility, since the assertion library currently
supports both xUnit.net v2 and v3 users, and has several optionally enabled features.

All back-compat projects are compiled with .NET Standard 1.1, as that's the bar set by xUnit.net v2.
C# language version is 6 for `XUNIT_SPAN` and `XUNIT_VALUETASK`, and 7.3 for `XUNIT_NULLABLE`.
The following projects exist for back-compat testing:

- `xunit.v3.assert.cs6.all-off.csproj` has no flags enabled
- `xunit.v3.assert.cs73.nullable.csproj` has `XUNIT_NULLABLE` enabled
- `xunit.v3.assert.cs6.span.csproj` has `XUNIT_SPAN` enabled
- `xunit.v3.assert.cs6.valuetask.csproj` has `XUNIT_VALUETASK` enabled
- `xunit.v3.assert.cs73.all-on.csproj` has all 3 flags enabled
