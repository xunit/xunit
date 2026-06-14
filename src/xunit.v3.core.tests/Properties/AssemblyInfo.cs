using Xunit;

[assembly: Trait("Assembly", "Trait")]

[CollectionDefinition("Shared state in FixtureWithEvents", DisableParallelization = true)]
public class FixtureWithEventsCollection { }
