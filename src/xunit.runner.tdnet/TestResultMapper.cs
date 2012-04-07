using TestDriven.Framework;

namespace Xunit.Runner.TdNet
{
    public static class TestResultMapper
    {
        public static TestRunState Map(TestRunnerResult result)
        {
            switch (result)
            {
                case TestRunnerResult.Passed:
                    return TestRunState.Success;

                case TestRunnerResult.Failed:
                    return TestRunState.Failure;

                default:
                    return TestRunState.NoTests;
            }
        }

        public static TestRunState Merge(TestRunState current, TestRunState toMerge)
        {
            if (toMerge == TestRunState.Success)
            {
                if (current == TestRunState.NoTests)
                    return TestRunState.Success;
            }
            else if (toMerge == TestRunState.Failure)
            {
                return TestRunState.Failure;
            }

            return current;
        }
    }
}