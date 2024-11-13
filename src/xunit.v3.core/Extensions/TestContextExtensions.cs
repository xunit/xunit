using System.Threading.Tasks;
using Xunit.Internal;

namespace Xunit;

/// <summary>
/// Extension methods for <see cref="ITestContext"/>.
/// </summary>
public static class TestContextExtensions
{
	/// <summary>
	/// Gets a fixture that was attached to the test class. Will return <c>null</c> if there is
	/// no exact match for the requested fixture type, or if there is no test class (that is,
	/// if <see cref="ITestContext.TestClass"/> returns <c>null</c>).
	/// </summary>
	/// <remarks>
	/// This may be a fixture attached via <see cref="IClassFixture{TFixture}"/>, <see cref="ICollectionFixture{TFixture}"/>,
	/// or <see cref="AssemblyFixtureAttribute"/>.
	/// </remarks>
	/// <typeparam name="TFixture">The exact type of the fixture</typeparam>
	/// <returns>The fixture, if available; <c>null</c>, otherwise</returns>
	public static async ValueTask<TFixture?> GetFixture<TFixture>(this ITestContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var fixture = await ctxt.GetFixture(typeof(TFixture));
		return
			fixture is null
				? default
				: (TFixture)fixture;
	}
}
