using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class TestClassCommandRunnerTests
    {
        [Fact]
        public void StartCallbackIsCalled()
        {
            int count = 0;
            string passName = typeof(VarietyTestClass).FullName + ".PassedTest";
            string failName = typeof(VarietyTestClass).FullName + ".FailedTest";
            string skipName = typeof(VarietyTestClass).FullName + ".SkippedTest";
            bool foundPass = false;
            bool foundFail = false;
            bool foundSkip = false;

            StubTestClassCommand command = new StubTestClassCommand(typeof(VarietyTestClass));

            TestClassCommandRunner.Execute(command, null,
                                           cmd =>
                                           {
                                               ++count;
                                               if (cmd.DisplayName == passName)
                                                   foundPass = true;
                                               if (cmd.DisplayName == failName)
                                                   foundFail = true;
                                               if (cmd.DisplayName == skipName)
                                                   foundSkip = true;
                                               return true;
                                           }, null);

            Assert.Equal(3, count);
            Assert.True(foundPass);
            Assert.True(foundFail);
            Assert.True(foundSkip);
        }

        [Fact]
        public void ResultCallbackIsCalledWithCorrectResults()
        {
            int classCounter = 0;
            int errorCounter = 0;
            int failedCounter = 0;
            int passedCounter = 0;
            int skippedCounter = 0;

            StubTestClassCommand command = new StubTestClassCommand(typeof(VarietyTestClass));

            TestClassCommandRunner.Execute(command, null, null,
                                           result =>
                                           {
                                               if (result is PassedResult)
                                                   passedCounter++;
                                               else if (result is FailedResult)
                                                   failedCounter++;
                                               else if (result is SkipResult)
                                                   skippedCounter++;
                                               else if (result is ClassResult)
                                                   classCounter++;
                                               else
                                                   errorCounter++;

                                               return true;
                                           });

            Assert.Equal(1, passedCounter);
            Assert.Equal(1, failedCounter);
            Assert.Equal(1, skippedCounter);
            Assert.Equal(1, classCounter);
            Assert.Equal(0, errorCounter);
        }

        [Fact]
        public void ClassFinishException()
        {
            StubTestClassCommand command = new StubTestClassCommand(typeof(VarietyTestClass));
            command.ClassFinish__Result = new Exception();

            ClassResult result = TestClassCommandRunner.Execute(command, null, null, null);

            Assert.Equal(typeof(Exception) + " : " + command.ClassFinish__Result.Message, result.Message);
            Assert.Equal(command.ClassFinish__Result.StackTrace, result.StackTrace);
        }

        [Fact]
        public void ClassFinishExceptionSupercedesClassStartException()
        {
            StubTestClassCommand command = new StubTestClassCommand(typeof(VarietyTestClass));
            command.ClassStart__Result = new NotImplementedException();
            command.ClassFinish__Result = new Exception();

            ClassResult result = TestClassCommandRunner.Execute(command, null, null, null);

            Assert.Equal(typeof(Exception) + " : " + command.ClassFinish__Result.Message, result.Message);
            Assert.Equal(command.ClassFinish__Result.StackTrace, result.StackTrace);
        }

        [Fact]
        public void ClassStartExceptionDoesNotRunTestsButDoesCallClassFinish()
        {
            StubTestClassCommand command = new StubTestClassCommand(typeof(VarietyTestClass));
            command.ClassStart__Result = new Exception();

            ClassResult result = TestClassCommandRunner.Execute(command, null, null, null);

            Assert.True(command.ClassStart__Called);
            Assert.True(command.ClassFinish__Called);
            Assert.Equal(typeof(Exception) + " : " + command.ClassStart__Result.Message, result.Message);
            Assert.Equal(command.ClassStart__Result.StackTrace, result.StackTrace);
            Assert.Equal(0, result.Results.Count);
        }

        [Fact]
        public void ExecuteWillNotRunRequestedNonTestMethod()
        {
            StubTestClassCommand command = new StubTestClassCommand(typeof(VarietyTestClass));
            List<IMethodInfo> methods = new List<IMethodInfo> {
            Reflector.Wrap(typeof(VarietyTestClass).GetMethod("NonTestMethod"))
        };

            ClassResult result = TestClassCommandRunner.Execute(command, methods, null, null);

            Assert.Equal(0, result.Results.Count);
        }

        [Fact]
        public void ExecuteWithNullMethodsRunAllTestMethods()
        {
            StubTestClassCommand command = new StubTestClassCommand(typeof(VarietyTestClass));

            ClassResult result = TestClassCommandRunner.Execute(command, null, null, null);

            Assert.Equal(VarietyTestClass.TestMethodCount, result.Results.Count);
        }

        [Fact]
        public void ExecuteWithSpecificMethodOnlyRunsThatMethod()
        {
            StubTestClassCommand command = new StubTestClassCommand(typeof(VarietyTestClass));
            List<IMethodInfo> methods = new List<IMethodInfo> {
            Reflector.Wrap(typeof(VarietyTestClass).GetMethod("PassedTest"))
        };

            ClassResult result = TestClassCommandRunner.Execute(command, methods, null, null);

            Assert.Single(result.Results);
        }

        [Fact]
        public void UsesProvidedObjectInstanceForAllTests()
        {
            InstanceSpy originalObject = new InstanceSpy();
            StubTestClassCommand command = new StubTestClassCommand(typeof(InstanceSpy));
            command.ObjectUnderTest__Result = originalObject;
            InstanceSpy.Reset();

            TestClassCommandRunner.Execute(command, null, null, null);

            Assert.Equal(3, InstanceSpy.instances.Count);
            foreach (object obj in InstanceSpy.instances)
                Assert.Same(originalObject, obj);
        }

        internal class VarietyTestClass
        {
            public const int TestMethodCount = 3;

            [Fact]
            public void FailedTest()
            {
                Assert.Equal(3, 2);
            }

            public void NonTestMethod() { }

            [Fact]
            public void PassedTest() { }

            [Fact(Skip = "reason")]
            public void SkippedTest() { }
        }

        internal class InstanceSpy
        {
            public static List<object> instances = new List<object>();

            public static void Reset()
            {
                instances.Clear();
            }

            [Fact]
            public void Test1()
            {
                instances.Add(this);
            }

            [Fact]
            public void Test2()
            {
                instances.Add(this);
            }

            [Fact]
            public void Test3()
            {
                instances.Add(this);
            }
        }
    }
}
