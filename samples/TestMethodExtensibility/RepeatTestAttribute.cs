using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

public class RepeatTestAttribute : FactAttribute
{
    readonly int repeatCount;

    public RepeatTestAttribute(int repeatCount)
    {
        this.repeatCount = repeatCount;
    }

    protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
    {
        for (int index = 0; index < repeatCount; index++)
            yield return new FactCommand(method);
    }
}