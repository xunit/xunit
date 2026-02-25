namespace Xunit.Generators;

public static class Types
{
	public static class System
	{
		const string ns = $"{nameof(System)}.";

		public const string IAsyncDisposable = ns + nameof(IAsyncDisposable);
		public const string IDisposable = ns + nameof(IDisposable);
		public const string ObsoleteAttribute = ns + nameof(ObsoleteAttribute);

		public static class Collections
		{
			public static class Generic
			{
				const string ns = $"{nameof(System)}.{nameof(Collections)}.{nameof(Generic)}.";

				public const string IAsyncEnumerableOfT = ns + "IAsyncEnumerable<>";
				public const string IEnumerableOfT = ns + "IEnumerable<>";
			}
		}

		public static class Runtime
		{
			public static class CompilerServices
			{
				const string ns = $"{nameof(System)}.{nameof(Runtime)}.{nameof(CompilerServices)}.";

				public const string ITuple = ns + nameof(ITuple);
			}
		}

		public static class Threading
		{
			public static class Tasks
			{
				const string ns = $"{nameof(System)}.{nameof(Threading)}.{nameof(Tasks)}.";

				public const string Task = ns + nameof(Task);
				public const string TaskOfT = Task + "<>";
				public const string ValueTask = ns + nameof(ValueTask);
				public const string ValueTaskOfT = ValueTask + "<>";
			}
		}
	}

	public static class Xunit
	{
		const string ns = $"{nameof(Xunit)}.";

		public const string AssemblyFixtureAttribute = ns + nameof(AssemblyFixtureAttribute);
		public const string ClassDataAttribute = ns + nameof(ClassDataAttribute);
		public const string CollectionBehaviorAttribute = ns + nameof(CollectionBehaviorAttribute);
		public const string CollectionDefinitionAttribute = ns + nameof(CollectionDefinitionAttribute);
		public const string CulturedFactAttribute = ns + nameof(CulturedFactAttribute);
		public const string CulturedTheoryAttribute = ns + nameof(CulturedTheoryAttribute);
		public const string FactAttribute = ns + nameof(FactAttribute);
		public const string IAsyncLifetime = ns + nameof(IAsyncLifetime);
		public const string IClassFixtureOfT = ns + "IClassFixture<>";
		public const string ICollectionFixtureOfT = ns + "ICollectionFixture<>";
		public const string InlineDataAttribute = ns + nameof(InlineDataAttribute);
		public const string ITheoryDataRow = ns + nameof(ITheoryDataRow);
		public const string MemberDataAttribute = ns + nameof(MemberDataAttribute);
		public const string TestCaseOrdererAttribute = ns + nameof(TestCaseOrdererAttribute);
		public const string TestClassOrdererAttribute = ns + nameof(TestClassOrdererAttribute);
		public const string TestCollectionOrdererAttribute = ns + nameof(TestCollectionOrdererAttribute);
		public const string TestFrameworkAttribute = ns + nameof(TestFrameworkAttribute);
		public const string TestMethodOrdererAttribute = ns + nameof(TestMethodOrdererAttribute);
		public const string TheoryAttribute = ns + nameof(TheoryAttribute);
		public const string TraitAttribute = ns + nameof(TraitAttribute);

		public static class Runner
		{
			public static class Common
			{
				const string ns = nameof(Xunit) + "." + nameof(Runner) + "." + nameof(Common) + ".";

				public const string IConsoleResultWriter = ns + nameof(IConsoleResultWriter);
				public const string IMicrosoftTestingPlatformResultWriter = ns + nameof(IMicrosoftTestingPlatformResultWriter);
				public const string IRunnerReporter = ns + nameof(IRunnerReporter);
				public const string RegisterConsoleResultWriterAttribute = ns + nameof(RegisterConsoleResultWriterAttribute);
				public const string RegisterMicrosoftTestingPlatformResultWriterAttribute = ns + nameof(RegisterMicrosoftTestingPlatformResultWriterAttribute);
				public const string RegisterResultWriterAttribute = ns + nameof(RegisterResultWriterAttribute);
				public const string RegisterRunnerReporterAttribute = ns + nameof(RegisterRunnerReporterAttribute);
			}
		}

		public static class v3
		{
			const string ns = $"{nameof(Xunit)}.{nameof(v3)}.";

			public const string BeforeAfterTestAttribute = ns + nameof(BeforeAfterTestAttribute);
			public const string CollectionPerAssemblyTestCollectionFactory = ns + nameof(CollectionPerAssemblyTestCollectionFactory);
			public const string CollectionPerClassTestCollectionFactory = ns + nameof(CollectionPerClassTestCollectionFactory);
			public const string DataAttribute = ns + nameof(DataAttribute);
			public const string ICodeGenTestAssembly = ns + nameof(ICodeGenTestAssembly);
			public const string ICodeGenTestCollectionFactory = ns + nameof(ICodeGenTestCollectionFactory);
			public const string ITestCaseOrderer = ns + nameof(ITestCaseOrderer);
			public const string ITestClassOrderer = ns + nameof(ITestClassOrderer);
			public const string ITestCollectionOrderer = ns + nameof(ITestCollectionOrderer);
			public const string ITestFramework = ns + nameof(ITestFramework);
			public const string ITestMethodOrderer = ns + nameof(ITestMethodOrderer);
			public const string ITestPipelineStartup = ns + nameof(ITestPipelineStartup);
			public const string TestPipelineStartupAttribute = ns + nameof(TestPipelineStartupAttribute);
		}
	}
}
