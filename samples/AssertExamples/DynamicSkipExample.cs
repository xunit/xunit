using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

namespace AssertExtensibility
{
    public class MyAssert
    {
        public static void Skip(string reason)
        {
            throw new SkipException(reason);
        }
    }

    public class DynamicSkipExample
    {
        [SkippableFact]
        public void WatchMeSkipLikeALittleGirl()
        {
            MyAssert.Skip("This is a skipped method!");

            throw new NotImplementedException(); // Never get here
        }
    }

    class SkipException : Exception
    {
        public SkipException(string reason)
        {
            Reason = reason;
        }

        public string Reason { get; set; }
    }

    class SkippableFactAttribute : FactAttribute
    {
        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            yield return new SkippableTestCommand(method);
        }

        class SkippableTestCommand : FactCommand
        {
            public SkippableTestCommand(IMethodInfo method) : base(method) { }

            public override MethodResult Execute(object testClass)
            {
                try
                {
                    return base.Execute(testClass);
                }
                catch (SkipException e)
                {
                    return new SkipResult(testMethod, DisplayName, e.Reason);
                }
            }
        }
    }
}
