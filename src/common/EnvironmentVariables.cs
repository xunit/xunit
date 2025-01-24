namespace Xunit.Internal
{
	internal static class EnvironmentVariables
	{
		public const string AssertEquivalentMaxDepth = "XUNIT_ASSERT_EQUIVALENT_MAX_DEPTH";
		public const string HidePassingOutputDiagnostics = "XUNIT_HIDE_PASSING_OUTPUT_DIAGNOSTICS";
		public const string PrintMaxEnumerableLength = "XUNIT_PRINT_MAX_ENUMERABLE_LENGTH";
		public const string PrintMaxObjectDepth = "XUNIT_PRINT_MAX_OBJECT_DEPTH";
		public const string PrintMaxObjectMemberCount = "XUNIT_PRINT_MAX_OBJECT_MEMBER_COUNT";
		public const string PrintMaxStringLength = "XUNIT_PRINT_MAX_STRING_LENGTH";
		public const string TestingPlatformDebug = "XUNIT_TESTINGPLATFORM_DEBUG";

		internal static class Defaults
		{
			public const int AssertEquivalentMaxDepth = 50;
			public const int PrintMaxEnumerableLength = 5;
			public const int PrintMaxObjectDepth = 3;
			public const int PrintMaxObjectMemberCount = 5;
			public const int PrintMaxStringLength = 50;
		}
	}
}
