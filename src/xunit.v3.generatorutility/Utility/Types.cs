#nullable enable

namespace Xunit.Generators
{
	/// <summary>
	/// A container for type names
	/// </summary>
	/// <remarks>
	/// All types in this hierarchy are partial so they can be extended by consuming developers
	/// </remarks>
	public static partial class Types
	{
		/// <summary/>
		public static partial class System
		{
			/// <summary/>
			public const string ObsoleteAttribute = "System.ObsoleteAttribute";

			/// <summary/>
			public static partial class Collections
			{
				/// <summary/>
				public const string IEnumerable = "System.Collections.IEnumerable";

				/// <summary/>
				public static partial class Generic
				{
					/// <summary/>
					public const string IAsyncEnumerableOfT = "System.Collections.Generic.IAsyncEnumerable<>";
					/// <summary/>
					public const string IEnumerableOfT = "System.Collections.Generic.IEnumerable<>";
				}
			}

			/// <summary/>
			public static partial class Runtime
			{
				/// <summary/>
				public static partial class CompilerServices
				{
					/// <summary/>
					public const string CallerFilePathAttribute = "System.Runtime.CompilerServices.CallerFilePathAttribute";
					/// <summary/>
					public const string CallerLineNumberAttribute = "System.Runtime.CompilerServices.CallerLineNumberAttribute";
					/// <summary/>
					public const string ITuple = "System.Runtime.CompilerServices.ITuple";
				}
			}

			/// <summary/>
			public static partial class Threading
			{
				/// <summary/>
				public static partial class Tasks
				{
					/// <summary/>
					public const string Task = "System.Threading.Tasks.Task";
					/// <summary/>
					public const string TaskOfT = "System.Threading.Tasks.Task<>";
					/// <summary/>
					public const string ValueTask = "System.Threading.Tasks.ValueTask";
					/// <summary/>
					public const string ValueTaskOfT = "System.Threading.Tasks.ValueTask<>";
				}
			}
		}

		/// <summary/>
		public static partial class Xunit
		{
			/// <summary/>
			public const string CollectionDefinitionAttribute = "Xunit.CollectionDefinitionAttribute";
			/// <summary/>
			public const string IClassFixtureOfT = "Xunit.IClassFixture<>";
			/// <summary/>
			public const string ICollectionFixtureOfT = "Xunit.ICollectionFixture<>";
			/// <summary/>
			public const string ITheoryDataRow = "Xunit.ITheoryDataRow";
			/// <summary/>
			public const string TestCaseOrdererAttribute = "Xunit.TestCaseOrdererAttribute";
			/// <summary/>
			public const string TestCaseOrdererAttributeOfT = "Xunit.TestCaseOrdererAttribute<>";
			/// <summary/>
			public const string TestClassAttribute = "Xunit.TestClassAttribute";
			/// <summary/>
			public const string TestClassOrdererAttribute = "Xunit.TestClassOrdererAttribute";
			/// <summary/>
			public const string TestClassOrdererAttributeOfT = "Xunit.TestClassOrdererAttribute<>";
			/// <summary/>
			public const string TestCollectionOrdererAttribute = "Xunit.TestCollectionOrdererAttribute";
			/// <summary/>
			public const string TestCollectionOrdererAttributeOfT = "Xunit.TestCollectionOrdererAttribute<>";
			/// <summary/>
			public const string TestMethodOrdererAttribute = "Xunit.TestMethodOrdererAttribute";
			/// <summary/>
			public const string TestMethodOrdererAttributeOfT = "Xunit.TestMethodOrdererAttribute<>";
			/// <summary/>
			public const string TraitAttribute = "Xunit.TraitAttribute";

			/// <summary/>
			public static partial class v3
			{
				/// <summary/>
				public const string BeforeAfterTestAttribute = "Xunit.v3.BeforeAfterTestAttribute";
				/// <summary/>
				public const string ICodeGenTestAssembly = "Xunit.v3.ICodeGenTestAssembly";
				/// <summary/>
				public const string ICodeGenTestCollectionFactory = "Xunit.v3.ICodeGenTestCollectionFactory";
				/// <summary/>
				public const string INotifyLifecycle = "Xunit.v3.INotifyLifecycle";
				/// <summary/>
				public const string ITestCaseOrderer = "Xunit.v3.ITestCaseOrderer";
				/// <summary/>
				public const string ITestClassOrderer = "Xunit.v3.ITestClassOrderer";
				/// <summary/>
				public const string ITestCollectionOrderer = "Xunit.v3.ITestCollectionOrderer";
				/// <summary/>
				public const string ITestFramework = "Xunit.v3.ITestFramework";
				/// <summary/>
				public const string ITestMethodOrderer = "Xunit.v3.ITestMethodOrderer";
				/// <summary/>
				public const string ITestPipelineStartup = "Xunit.v3.ITestPipelineStartup";
			}
		}
	}
}
