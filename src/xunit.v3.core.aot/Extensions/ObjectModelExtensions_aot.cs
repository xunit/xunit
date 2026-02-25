namespace Xunit.v3;

internal static class ObjectModelExtensions
{
	public static bool IsStaticallySkipped(this ICodeGenTestCase testCase) =>
		!string.IsNullOrWhiteSpace(testCase.SkipReason) &&
		testCase.SkipUnless is null &&
		testCase.SkipWhen is null;
}
