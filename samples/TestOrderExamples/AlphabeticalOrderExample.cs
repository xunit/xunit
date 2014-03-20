using Xunit;

[TestCaseOrderer("AlphabeticalOrderer", "TestOrderExamples")]
public class AlphabeticalOrderExample
{
    public static bool Test1Called;
    public static bool Test2Called;
    public static bool Test3Called;

    [Fact]
    public void Test1()
    {
        Test1Called = true;

        Assert.False(Test2Called);
        Assert.False(Test3Called);
    }

    [Fact]
    public void Test2()
    {
        Test2Called = true;

        Assert.True(Test1Called);
        Assert.False(Test3Called);
    }

    [Fact]
    public void Test3()
    {
        Test3Called = true;

        Assert.True(Test1Called);
        Assert.True(Test2Called);
    }
}

//// The default priority is 0. Within a given priority, the tests are run in
//// reflection order, just like the example above. Since reflection order should
//// not be relied upon, use test priority to ensure certain classes of tests run
//// before others as appropriate.

//[PrioritizedFixture]
//public class CustomOrdering
//{
//    public static bool Test1Called;
//    public static bool Test2Called;
//    public static bool Test3Called;

//    [Fact, TestPriority(5)]
//    public void Test3()
//    {
//        Test3Called = true;

//        Assert.True(Test1Called);
//        Assert.True(Test2Called);
//    }

//    [Fact]
//    public void Test2()
//    {
//        Test2Called = true;

//        Assert.True(Test1Called);
//        Assert.False(Test3Called);
//    }

//    [Fact, TestPriority(-5)]
//    public void Test1()
//    {
//        Test1Called = true;

//        Assert.False(Test2Called);
//        Assert.False(Test3Called);
//    }
//}