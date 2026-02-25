using Xunit;
using Xunit.Sdk;
using Xunit.v3;

partial class FixtureMappingManagerTests
{
	// Approximation of what the code generator writes
	static Func<FixtureMappingManager?, ValueTask<object>> FixtureWithDependencyFactory(FixtureMappingManager manager) =>
		async _ =>
		{
			var obj = await manager.TryGetFixtureArgument<object>();
			if (!obj.Success)
				throw new TestPipelineException($"Testable fixture type '{typeof(FixtureWithDependency).SafeName()}' had one or more unresolved constructor arguments: Object dependency");

			return new FixtureWithDependency(obj.Result!);
		};

	// Approximation of what the code generator writes
	static Func<FixtureMappingManager?, ValueTask<object>> FixtureWithMessageSinkAndTestContextFactory(FixtureMappingManager manager) =>
		async _ =>
		{
			var missingParameters = new List<(string Type, string Name)>();

			var messageSink = await manager.TryGetFixtureArgument<IMessageSink>();
			if (!messageSink.Success)
				missingParameters.Add((typeof(IMessageSink).Name, "messageSink"));

			var contextAccessor = await manager.TryGetFixtureArgument<ITestContextAccessor>();
			if (!contextAccessor.Success)
				missingParameters.Add((typeof(ITestContextAccessor).Name, "contextAccessor"));

			if (missingParameters.Count != 0)
				throw new TestPipelineException($"Testable fixture type '{typeof(FixtureWithMessageSinkAndTestContext).SafeName()}' had one or more unresolved constructor arguments: {string.Join(", ", missingParameters.Select(p => $"{p.Type} {p.Name}"))}");

			return new FixtureWithMessageSinkAndTestContext(messageSink.Result!, contextAccessor.Result!);
		};

	class TestableFixtureMappingManager : FixtureMappingManager
	{
		Dictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>>? fixtureFactories = null;

		public TestableFixtureMappingManager(FixtureMappingManager parent) :
			base("Testable", TestData.EmptyFixtureFactories, parent)
		{ }

		public TestableFixtureMappingManager(params object[] cachedFixtureValues) :
			base("Testable", cachedFixtureValues)
		{ }

		protected override IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> FixtureFactories =>
			fixtureFactories ?? base.FixtureFactories;

		public ValueTask InitializeAsync(
			Type fixtureType,
			Func<FixtureMappingManager?, ValueTask<object>> fixtureFactory,
			bool createInstances = true)
		{
			fixtureFactories = new() { [fixtureType] = fixtureFactory };

			return InitializeAsync(createInstances);
		}
	}
}
