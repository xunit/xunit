namespace Xunit.Generators;

public static class Names
{
	public static class Xunit
	{
		public static class CollectionDefinitionAttribute
		{
			public const string DisableParallelization = nameof(DisableParallelization);
		}

		public static class Internal
		{
			public static class FactAttributeBase
			{
				public const string DisplayName = nameof(DisplayName);
				public const string Explicit = nameof(Explicit);
				public const string Skip = nameof(Skip);
				public const string SkipExceptions = nameof(SkipExceptions);
				public const string SkipType = nameof(SkipType);
				public const string SkipUnless = nameof(SkipUnless);
				public const string SkipWhen = nameof(SkipWhen);
				public const string Timeout = nameof(Timeout);
			}

			public static class TheoryAttributeBase
			{
				public const string DisableDiscoveryEnumeration = nameof(DisableDiscoveryEnumeration);
				public const string SkipTestWithoutData = nameof(SkipTestWithoutData);
			}
		}

		public static class v3
		{
			public static class DataAttribute
			{
				public static readonly HashSet<string> AllProperties = [Explicit, Label, Skip, SkipType, SkipUnless, SkipWhen, TestDisplayName, Timeout, Traits];

				public const string Explicit = nameof(Explicit);
				public const string Label = nameof(Label);
				public const string Skip = nameof(Skip);
				public const string SkipType = nameof(SkipType);
				public const string SkipUnless = nameof(SkipUnless);
				public const string SkipWhen = nameof(SkipWhen);
				public const string TestDisplayName = nameof(TestDisplayName);
				public const string Timeout = nameof(Timeout);
				public const string Traits = nameof(Traits);
			}

			public static class MemberDataAttributeBase
			{
				public const string DisableDiscoveryEnumeration = nameof(DisableDiscoveryEnumeration);
				public const string MemberType = nameof(MemberType);
			}
		}
	}
}
