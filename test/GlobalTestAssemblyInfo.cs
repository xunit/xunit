using TestDriven.Framework;
using Xunit;
using Xunit.Runner.TdNet;

[assembly: CustomTestRunner(typeof(TdNetRunner))]

// This makes the tests slightly slower, but it should also help us catch any Task-related
// deadlocks in the test execution pipeline.
[assembly: CollectionBehavior(MaxDegreeOfParallelism = 1)]
