using System.Xml;
using TestUtility;
using Xunit;

public class ExternalRunnerTests : AcceptanceTest
{
    [Fact]
    public void UsesCorrectExternalRunner()
    {
        string code = @"
            using System;
            using System.Collections.Generic;
            using System.Reflection;
            using Xunit;
            using Xunit.Sdk;

            namespace Test.AcceptanceTest.ExternalRunner
            {
                public class StubExternalRunner : ITestClassCommand
                {
                    ITypeInfo typeUnderTest;

                    public object ObjectUnderTest
                    {
                        get { return null; }
                    }

                    public ITypeInfo TypeUnderTest
                    {
                        get { return typeUnderTest; }
                        set { typeUnderTest = value; }
                    }

                    public Exception ClassFinish() { return null; }

                    public Exception ClassStart() { return null; }

                    public int ChooseNextTest(ICollection<IMethodInfo> methods)
                    {
                        return 0;
                    }

                    public IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod)
                    {
                        yield return new StubRunnerTestCommand(testMethod);
                    }

                    public IEnumerable<IMethodInfo> EnumerateTestMethods()
                    {
                        yield return Reflector.Wrap(typeof(ClassUnderTest).GetMethod(""Method""));
                    }

                    public bool IsTestMethod(IMethodInfo testMethod)
                    {
                        return testMethod.Name == ""Method"";
                    }
                }

                public class StubRunnerTestCommand : TestCommand
                {
                    public StubRunnerTestCommand(IMethodInfo method)
                        : base(method, null, 0) { }

                    public override MethodResult Execute(object testClass)
                    {
                        return new PassedResult(testMethod, DisplayName);
                    }
                }

                [RunWith(typeof(StubExternalRunner))]
                public class ClassUnderTest
                {
                    public void Method() {}
                }
            }
        ";

        XmlNode assemblyNode = Execute(code);

        ResultXmlUtility.AssertResult(assemblyNode, "Pass", "Test.AcceptanceTest.ExternalRunner.ClassUnderTest.Method");
    }

    [Fact]
    public void SpecificationSampleAcceptanceTest()
    {
        string code = @"
            using System;
            using System.Collections.Generic;
            using System.Linq;
            using Xunit;
            using Xunit.Sdk;

            public class SpecificationBaseRunner : ITestClassCommand
            {
                SpecificationBase objectUnderTest;

                public SpecificationBase ObjectUnderTest
                {
                    get
                    {
                        if (objectUnderTest == null)
                        {
                            GuardTypeUnderTest();
                            objectUnderTest = (SpecificationBase)Activator.CreateInstance(TypeUnderTest.Type);
                        }

                        return objectUnderTest;
                    }
                }

                object ITestClassCommand.ObjectUnderTest
                {
                    get { return ObjectUnderTest; }
                }

                public ITypeInfo TypeUnderTest { get; set; }

                public int ChooseNextTest(ICollection<IMethodInfo> testsLeftToRun)
                {
                    return 0;
                }

                public Exception ClassFinish()
                {
                    try
                    {
                        ObjectUnderTest.OnFinish();
                        return null;
                    }
                    catch (Exception ex)
                    {
                        return ex;
                    }
                }

                public Exception ClassStart()
                {
                    try
                    {
                        ObjectUnderTest.OnStart();
                        return null;
                    }
                    catch (Exception ex)
                    {
                        return ex;
                    }
                }

                public IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod)
                {
                    string displayName = (TypeUnderTest.Type.Name + "", it "" + testMethod.Name).Replace('_', ' ');
                    return new[] { new SpecificationBaseCommand(testMethod, displayName) };
                }

                public IEnumerable<IMethodInfo> EnumerateTestMethods()
                {
                    GuardTypeUnderTest();

                    return TypeUnderTest.GetMethods().Where(IsTestMethod);
                }

                void GuardTypeUnderTest()
                {
                    if (TypeUnderTest == null)
                        throw new InvalidOperationException(""Forgot to set TypeUnderTest before calling ObjectUnderTest"");

                    if (!typeof(SpecificationBase).IsAssignableFrom(TypeUnderTest.Type))
                        throw new InvalidOperationException(""SpecificationBaseRunner can only be used with types that derive from SpecificationBase"");
                }

                public bool IsTestMethod(IMethodInfo testMethod)
                {
                    return testMethod.HasAttribute(typeof(ObservationAttribute));
                }
            }

            [RunWith(typeof(SpecificationBaseRunner))]
            public abstract class SpecificationBase
            {
                protected virtual void Because() { }

                protected virtual void DestroyContext() { }

                protected virtual void EstablishContext() { }

                internal void OnFinish()
                {
                    DestroyContext();
                }

                internal void OnStart()
                {
                    EstablishContext();
                    Because();
                }
            }

            public class SpecificationBaseCommand : TestCommand
            {
                public SpecificationBaseCommand(IMethodInfo method, string displayName)
                    : base(method, displayName, 0) { }

                public override MethodResult Execute(object testClass)
                {
                    testMethod.Invoke(testClass, null);
                    return new PassedResult(testMethod, DisplayName);
                }
            }

            [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
            public class ObservationAttribute : Attribute { }

            public class When_the_specification_runs : SpecificationBase
            {
                static int establishRunCount = 0;
                static int becauseRunCount = 0;
                static int destroyRunCount = 0;

                protected override void EstablishContext()
                {
                    ++establishRunCount;
                    Assert.Equal(1, establishRunCount);
                }

                protected override void Because()
                {
                    ++becauseRunCount;
                    Assert.Equal(1, establishRunCount);
                    Assert.Equal(1, becauseRunCount);
                }

                protected override void DestroyContext()
                {
                    ++destroyRunCount;
                    Assert.Equal(1, establishRunCount);
                    Assert.Equal(1, becauseRunCount);
                    Assert.Equal(1, destroyRunCount);
                }

                [Observation]
                public void should_ensure_run_counts_are_correct()
                {
                    Assert.Equal(1, establishRunCount);
                    Assert.Equal(1, becauseRunCount);
                    Assert.Equal(0, destroyRunCount);
                }

                [Observation]
                public void should_fail()
                {
                    Assert.True(false);
                }
            }";

        XmlNode assemblyNode = Execute(code);

        XmlNode result0 = ResultXmlUtility.GetResult(assemblyNode, 0);
        Assert.Equal("Pass", result0.Attributes["result"].Value);
        Assert.Equal("When the specification runs, it should ensure run counts are correct", result0.Attributes["name"].Value);
        XmlNode result1 = ResultXmlUtility.GetResult(assemblyNode, 1);
        Assert.Equal("Fail", result1.Attributes["result"].Value);
        Assert.Equal("When the specification runs, it should fail", result1.Attributes["name"].Value);
    }
}