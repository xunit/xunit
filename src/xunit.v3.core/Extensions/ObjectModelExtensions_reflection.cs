namespace Xunit.v3;

internal static class ObjectModelExtensions
{
	public static bool IsStaticallySkipped(this IXunitTestCase testCase) =>
		!string.IsNullOrWhiteSpace(testCase.SkipReason) &&
		string.IsNullOrWhiteSpace(testCase.SkipUnless) &&
		string.IsNullOrWhiteSpace(testCase.SkipWhen);
}
