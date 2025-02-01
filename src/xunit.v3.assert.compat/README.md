These projects are here to validate compatibility (mostly backward), since the assertion library currently has several optionally enabled features.

The minimum target framework support for the assertion library is:

* .NET Standard 2.0+
* .NET Framework 4.7.2+
* .NET 8+

The compatibility projects are mostly compiled with one or more of the minimum target frameworks, except for `xunit.v3.assert.all-on` which adds additional targets for newer versions of target frameworks as they become available (to help ensure that source-based consumers don't encounter new issues from newer target frameworks).

The minimum C# language versions is 7.3, as this aligns with the default when building for `net472`. Adding support for `XUNIT_NULLABLE` requires a minimum C# language version of 9, as this is what's required for our nullability annotations.
