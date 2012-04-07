using System.Collections.Generic;
using System.Linq;
using Moq;
using TestUtility;
using Xunit;

public class MultiAssemblyTestEnvironmentAcceptanceTests
{
    [Fact]
    public void SingleAssemblyAcceptanceTest()
    {
        string code =
            @"
                using System;
                using Xunit;

                public class MockTestClass
                {
                    [Fact]
                    public void SuccessTest()
                    {
                        Assert.Equal(2, 2);
                    }
                    [Fact]
                    public void FailureTest()
                    {
                        Assert.Equal(2, 3);
                    }
                    [Fact(Skip=""I'm too lazy to run today"")]
                    public void SkippingTest()
                    {
                        Assert.Equal(2, 4);
                    }
                }
            ";

        using (MockAssembly mockAssembly = new MockAssembly())
        {
            mockAssembly.Compile(code);
            Mock<ITestMethodRunnerCallback> callback = new Mock<ITestMethodRunnerCallback>();
            callback.Setup(c => c.TestStart(It.IsAny<TestMethod>())).Returns(true);
            callback.Setup(c => c.TestFinished(It.IsAny<TestMethod>())).Returns(true);
            MultiAssemblyTestEnvironment mate = new MultiAssemblyTestEnvironment();
            mate.Load(mockAssembly.FileName);
            TestAssembly testAssembly = mate.EnumerateTestAssemblies().Single();
            TestMethod passingMethod = testAssembly.EnumerateTestMethods().Where(m => m.MethodName == "SuccessTest").Single();
            TestMethod failingMethod = testAssembly.EnumerateTestMethods().Where(m => m.MethodName == "FailureTest").Single();
            TestMethod skippedMethod = testAssembly.EnumerateTestMethods().Where(m => m.MethodName == "SkippingTest").Single();

            mate.Run(mate.EnumerateTestMethods(), callback.Object);

            callback.Verify(c => c.AssemblyStart(testAssembly));
            callback.Verify(c => c.TestStart(passingMethod));
            callback.Verify(c => c.TestFinished(passingMethod));
            callback.Verify(c => c.TestStart(failingMethod));
            callback.Verify(c => c.TestFinished(failingMethod));
            callback.Verify(c => c.TestStart(skippedMethod), Times.Never());
            callback.Verify(c => c.TestFinished(skippedMethod));
            callback.Verify(c => c.AssemblyFinished(testAssembly, 3, 1, 1, It.IsAny<double>()));
            var passingMethodResult = Assert.IsType<TestPassedResult>(passingMethod.RunResults[0]);
            Assert.Null(passingMethodResult.Output);
            var failingMethodResult = Assert.IsType<TestFailedResult>(failingMethod.RunResults[0]);
            Assert.Null(failingMethodResult.Output);
            Assert.Equal("Xunit.Sdk.EqualException", failingMethodResult.ExceptionType);
            var skippedMethodResult = Assert.IsType<TestSkippedResult>(skippedMethod.RunResults[0]);
        }
    }

    [Fact]
    public void MultiAssemblyAcceptanceTest()
    {
        string code =
            @"
                using System;
                using Xunit;

                public class MockTestClass
                {
                    [Fact]
                    public void SuccessTest()
                    {
                        Assert.Equal(2, 2);
                    }
                }
            ";

        using (MockAssembly mockAssembly1 = new MockAssembly())
        using (MockAssembly mockAssembly2 = new MockAssembly())
        {
            mockAssembly1.Compile(code);
            mockAssembly2.Compile(code);
            Mock<ITestMethodRunnerCallback> callback = new Mock<ITestMethodRunnerCallback>();
            callback.Setup(c => c.TestStart(It.IsAny<TestMethod>())).Returns(true);
            callback.Setup(c => c.TestFinished(It.IsAny<TestMethod>())).Returns(true);
            MultiAssemblyTestEnvironment mate = new MultiAssemblyTestEnvironment();
            mate.Load(mockAssembly1.FileName);
            mate.Load(mockAssembly2.FileName);
            TestAssembly testAssembly1 = mate.EnumerateTestAssemblies().Where(a => a.AssemblyFilename == mockAssembly1.FileName).Single();
            TestAssembly testAssembly2 = mate.EnumerateTestAssemblies().Where(a => a.AssemblyFilename == mockAssembly2.FileName).Single();
            TestMethod assembly1Method = testAssembly1.EnumerateTestMethods().Single();
            TestMethod assembly2Method = testAssembly1.EnumerateTestMethods().Single();

            mate.Run(mate.EnumerateTestMethods(), callback.Object);

            callback.Verify(c => c.AssemblyStart(testAssembly1));
            callback.Verify(c => c.TestStart(assembly1Method));
            callback.Verify(c => c.TestFinished(assembly1Method));
            callback.Verify(c => c.AssemblyFinished(testAssembly1, 1, 0, 0, It.IsAny<double>()));
            callback.Verify(c => c.AssemblyStart(testAssembly2));
            callback.Verify(c => c.TestStart(assembly2Method));
            callback.Verify(c => c.TestFinished(assembly2Method));
            callback.Verify(c => c.AssemblyFinished(testAssembly2, 1, 0, 0, It.IsAny<double>()));
        }
    }

    [Fact]
    public void MultiAssemblySimpleFilterAcceptanceTest()
    {
        string code =
            @"
                using System;
                using Xunit;

                public class MockTestClass
                {
                    [Fact]
                    public void SuccessTest()
                    {
                        Assert.Equal(2, 2);
                    }
                }
            ";

        using (MockAssembly mockAssembly1 = new MockAssembly())
        using (MockAssembly mockAssembly2 = new MockAssembly())
        {
            mockAssembly1.Compile(code);
            mockAssembly2.Compile(code);
            Mock<ITestMethodRunnerCallback> callback = new Mock<ITestMethodRunnerCallback>();
            callback.Setup(c => c.TestStart(It.IsAny<TestMethod>())).Returns(true);
            callback.Setup(c => c.TestFinished(It.IsAny<TestMethod>())).Returns(true);
            MultiAssemblyTestEnvironment mate = new MultiAssemblyTestEnvironment();
            mate.Load(mockAssembly1.FileName);
            mate.Load(mockAssembly2.FileName);
            TestAssembly testAssembly1 = mate.EnumerateTestAssemblies().Where(a => a.AssemblyFilename == mockAssembly1.FileName).Single();
            TestAssembly testAssembly2 = mate.EnumerateTestAssemblies().Where(a => a.AssemblyFilename == mockAssembly2.FileName).Single();
            TestMethod assembly1Method = testAssembly1.EnumerateTestMethods().Single();
            TestMethod assembly2Method = testAssembly1.EnumerateTestMethods().Single();

            mate.Run(mate.EnumerateTestMethods(m => m.TestClass.TestAssembly == testAssembly1), callback.Object);

            callback.Verify(c => c.AssemblyStart(testAssembly1));
            callback.Verify(c => c.TestStart(assembly1Method));
            callback.Verify(c => c.TestFinished(assembly1Method));
            callback.Verify(c => c.AssemblyFinished(testAssembly1, 1, 0, 0, It.IsAny<double>()));
            callback.Verify(c => c.AssemblyStart(testAssembly2), Times.Never());
            var runResult = Assert.IsType<TestPassedResult>(assembly1Method.RunResults[0]);
            Assert.Null(runResult.Output);
        }
    }

    [Fact]
    public void MultiAssemblyTraitFilterAcceptanceTest()
    {
        string code =
            @"
                using System;
                using Xunit;

                public class MockTestClass
                {
                    [Fact]
                    [Trait(""Trait1"", ""Value1"")]
                    public void Value1Test()
                    {
                    }

                    [Fact]
                    [Trait(""Trait1"", ""Value2"")]
                    public void Value2Test()
                    {
                    }
                }
            ";

        using (MockAssembly mockAssembly1 = new MockAssembly())
        using (MockAssembly mockAssembly2 = new MockAssembly())
        {
            mockAssembly1.Compile(code);
            mockAssembly2.Compile(code);
            Mock<ITestMethodRunnerCallback> callback = new Mock<ITestMethodRunnerCallback>();
            callback.Setup(c => c.TestStart(It.IsAny<TestMethod>())).Returns(true);
            callback.Setup(c => c.TestFinished(It.IsAny<TestMethod>())).Returns(true);
            MultiAssemblyTestEnvironment mate = new MultiAssemblyTestEnvironment();
            mate.Load(mockAssembly1.FileName);
            mate.Load(mockAssembly2.FileName);
            TestAssembly testAssembly1 = mate.EnumerateTestAssemblies().Where(a => a.AssemblyFilename == mockAssembly1.FileName).Single();
            TestAssembly testAssembly2 = mate.EnumerateTestAssemblies().Where(a => a.AssemblyFilename == mockAssembly2.FileName).Single();
            TestMethod assembly1Value1Method = testAssembly1.EnumerateTestMethods().Where(m => m.MethodName == "Value1Test").Single();
            TestMethod assembly1Value2Method = testAssembly1.EnumerateTestMethods().Where(m => m.MethodName == "Value2Test").Single();
            TestMethod assembly2Value1Method = testAssembly2.EnumerateTestMethods().Where(m => m.MethodName == "Value1Test").Single();
            TestMethod assembly2Value2Method = testAssembly2.EnumerateTestMethods().Where(m => m.MethodName == "Value2Test").Single();

            mate.Run(mate.EnumerateTestMethods(m => m.Traits["Trait1"].FirstOrDefault() == "Value1"), callback.Object);

            callback.Verify(c => c.TestStart(assembly1Value1Method));
            callback.Verify(c => c.TestStart(assembly1Value2Method), Times.Never());
            callback.Verify(c => c.TestStart(assembly2Value1Method));
            callback.Verify(c => c.TestStart(assembly2Value2Method), Times.Never());
        }
    }

    [Fact]
    public void MultiAssemblyGetTraitsAcceptanceTest()
    {
        string code1 =
            @"
                using System;
                using Xunit;

                public class MockTestClass
                {
                    [Fact]
                    [Trait(""Trait1"", ""Value1"")]
                    public void Value1Test()
                    {
                    }

                    [Fact]
                    [Trait(""Trait1"", ""Value2"")]
                    public void Value2Test()
                    {
                    }

                    [Fact]
                    [Trait(""Trait2"", ""Value1"")]
                    public void Trait2Value1Test()
                    {
                    }
                }
            ";

        string code2 =
            @"
                using System;
                using Xunit;

                public class MockTestClass
                {
                    [Fact]
                    [Trait(""Trait1"", ""Value1"")]
                    public void OtherTest1()
                    {
                    }

                    [Fact]
                    [Trait(""Trait3"", ""Value42"")]
                    public void Crazy()
                    {
                    }
                }
            ";

        using (MockAssembly mockAssembly1 = new MockAssembly())
        using (MockAssembly mockAssembly2 = new MockAssembly())
        {
            mockAssembly1.Compile(code1);
            mockAssembly2.Compile(code2);
            MultiAssemblyTestEnvironment mate = new MultiAssemblyTestEnvironment();
            mate.Load(mockAssembly1.FileName);
            mate.Load(mockAssembly2.FileName);

            MultiValueDictionary<string, string> result = mate.EnumerateTraits();

            var trait1 = result["Trait1"];
            Assert.Equal(2, trait1.Count());
            Assert.Contains("Value1", trait1);
            Assert.Contains("Value2", trait1);
            var trait2 = result["Trait2"];
            Assert.Single(trait2);
            Assert.Contains("Value1", trait2);
            var trait3 = result["Trait3"];
            Assert.Single(trait3);
            Assert.Contains("Value42", trait3);
        }
    }

    [Fact]
    public void MultiAssemblySearchFilterAcceptanceTest()
    {
        string code =
            @"
                using System;
                using Xunit;

                public class MockTestClass
                {
                    [Fact]
                    public void Test1()
                    {
                    }

                    [Fact]
                    public void Test2()
                    {
                    }
                }
            ";

        using (MockAssembly mockAssembly1 = new MockAssembly())
        using (MockAssembly mockAssembly2 = new MockAssembly())
        {
            mockAssembly1.Compile(code);
            mockAssembly2.Compile(code);
            Mock<ITestMethodRunnerCallback> callback = new Mock<ITestMethodRunnerCallback>();
            callback.Setup(c => c.TestStart(It.IsAny<TestMethod>())).Returns(true);
            callback.Setup(c => c.TestFinished(It.IsAny<TestMethod>())).Returns(true);
            MultiAssemblyTestEnvironment mate = new MultiAssemblyTestEnvironment();
            mate.Load(mockAssembly1.FileName);
            mate.Load(mockAssembly2.FileName);
            TestAssembly testAssembly1 = mate.EnumerateTestAssemblies().Where(a => a.AssemblyFilename == mockAssembly1.FileName).Single();
            TestAssembly testAssembly2 = mate.EnumerateTestAssemblies().Where(a => a.AssemblyFilename == mockAssembly2.FileName).Single();
            TestMethod assembly1Test1Method = testAssembly1.EnumerateTestMethods().Where(m => m.MethodName == "Test1").Single();
            TestMethod assembly1Test2Method = testAssembly1.EnumerateTestMethods().Where(m => m.MethodName == "Test2").Single();
            TestMethod assembly2Test1Method = testAssembly2.EnumerateTestMethods().Where(m => m.MethodName == "Test1").Single();
            TestMethod assembly2Test2Method = testAssembly2.EnumerateTestMethods().Where(m => m.MethodName == "Test2").Single();

            mate.Run(mate.EnumerateTestMethods(m => m.MethodName.Contains("t2")), callback.Object);

            callback.Verify(c => c.TestStart(assembly1Test1Method), Times.Never());
            callback.Verify(c => c.TestStart(assembly1Test2Method));
            callback.Verify(c => c.TestStart(assembly2Test1Method), Times.Never());
            callback.Verify(c => c.TestStart(assembly2Test2Method));
        }
    }
}
