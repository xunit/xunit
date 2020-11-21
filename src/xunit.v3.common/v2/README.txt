Everything in this folder will eventually be moved to xunit.v3.runner.common, as it's
specific to xUnit.net v2. It lives here for now until all ties to xUnit.net v2 abstractions
can be cut from xunit.v3.core and friends.

The reference to Xunit.Abstractions from xunit.v3.common.csproj should then be removed.
