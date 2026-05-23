#nullable enable

namespace Xunit.Generators
{
	/// <summary>
	/// A container for member names
	/// </summary>
	/// <remarks>
	/// All types in this hierarchy are partial so they can be extended by consuming developers
	/// </remarks>
	public static partial class Names
	{
		/// <summary>
		/// Member names from <c>Xunit.v3.DataAttribute</c>
		/// </summary>
		public static partial class DataAttribute
		{
			/// <summary/>
			public const string Explicit = nameof(Explicit);
			/// <summary/>
			public const string Label = nameof(Label);
			/// <summary/>
			public const string Skip = nameof(Skip);
			/// <summary/>
			public const string SkipType = nameof(SkipType);
			/// <summary/>
			public const string SkipUnless = nameof(SkipUnless);
			/// <summary/>
			public const string SkipWhen = nameof(SkipWhen);
			/// <summary/>
			public const string TestDisplayName = nameof(TestDisplayName);
			/// <summary/>
			public const string Timeout = nameof(Timeout);
			/// <summary/>
			public const string Traits = nameof(Traits);
		}

		/// <summary>
		/// Member names from <c>Xunit.FactAttribute</c>
		/// </summary>
		public static partial class FactAttribute
		{
			/// <summary/>
			public const string DisplayName = nameof(DisplayName);
			/// <summary/>
			public const string Explicit = nameof(Explicit);
			/// <summary/>
			public const string Skip = nameof(Skip);
			/// <summary/>
			public const string SkipExceptions = nameof(SkipExceptions);
			/// <summary/>
			public const string SkipType = nameof(SkipType);
			/// <summary/>
			public const string SkipUnless = nameof(SkipUnless);
			/// <summary/>
			public const string SkipWhen = nameof(SkipWhen);
			/// <summary/>
			public const string Timeout = nameof(Timeout);
		}

		/// <summary>
		/// Member names from <c>Xunit.TheoryAttribute</c>
		/// </summary>
		public static partial class TheoryAttribute
		{
			/// <summary/>
			public const string DisableDiscoveryEnumeration = nameof(DisableDiscoveryEnumeration);
			/// <summary/>
			public const string IncludeTestCaseIndex = nameof(IncludeTestCaseIndex);
			/// <summary/>
			public const string SkipTestWithoutData = nameof(SkipTestWithoutData);
		}
	}
}
