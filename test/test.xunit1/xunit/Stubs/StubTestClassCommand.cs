using System;
using System.Collections.Generic;
using Xunit.Sdk;

class StubTestClassCommand : ITestClassCommand
{
    public bool ChooseNextTest__Called;
    public ICollection<IMethodInfo> ChooseNextTest_TestsLeftToRun;
    public bool ClassFinish__Called;
    public Exception ClassFinish__Result = null;
    public bool ClassStart__Called;
    public Exception ClassStart__Result = null;
    public bool EnumerateTestCommands__Called;
    public IEnumerable<ITestCommand> EnumerateTestCommands__Result = null;
    public IMethodInfo EnumerateTestCommands_TestMethod;
    public bool EnumerateTestMethods__Called;
    public IEnumerable<IMethodInfo> EnumerateTestMethods__Result = null;
    public bool IsTestMethod__Called;
    public bool? IsTestMethod__Result = null;
    public IMethodInfo IsTestMethod_TestMethod;
    public object ObjectUnderTest__Result = null;
    TestClassCommand testClassCommand;
    ITypeInfo typeUnderTest;

    public StubTestClassCommand() { }

    public StubTestClassCommand(Type typeUnderTest)
    {
        this.typeUnderTest = Reflector.Wrap(typeUnderTest);
        testClassCommand = new TestClassCommand(typeUnderTest);
    }

    public object ObjectUnderTest
    {
        get { return ObjectUnderTest__Result; }
    }

    public ITypeInfo TypeUnderTest
    {
        get { return typeUnderTest; }
        set { typeUnderTest = value; }
    }

    public int ChooseNextTest(ICollection<IMethodInfo> testsLeftToRun)
    {
        ChooseNextTest__Called = true;
        ChooseNextTest_TestsLeftToRun = testsLeftToRun;

        return 0;
    }

    public Exception ClassFinish()
    {
        ClassFinish__Called = true;

        return ClassFinish__Result;
    }

    public Exception ClassStart()
    {
        ClassStart__Called = true;

        return ClassStart__Result;
    }

    public IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod)
    {
        EnumerateTestCommands__Called = true;
        EnumerateTestCommands_TestMethod = testMethod;

        return EnumerateTestCommands__Result ?? testClassCommand.EnumerateTestCommands(testMethod);
    }

    public IEnumerable<IMethodInfo> EnumerateTestMethods()
    {
        EnumerateTestMethods__Called = true;

        return EnumerateTestMethods__Result ?? testClassCommand.EnumerateTestMethods();
    }

    public bool IsTestMethod(IMethodInfo testMethod)
    {
        IsTestMethod__Called = true;
        IsTestMethod_TestMethod = testMethod;

        return IsTestMethod__Result.HasValue ? IsTestMethod__Result.Value : testClassCommand.IsTestMethod(testMethod);
    }
}
