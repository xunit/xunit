using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Gui;

public class RecentlyUsedAssemblyListTests
{
    [Fact]
    [RecentlyUsedAssemblyListRollback]
    public void AddedAssemblyIsInList()
    {
        RecentlyUsedAssemblyList mruList = new RecentlyUsedAssemblyList();

        mruList.Add(@"C:\Foo\Bar.dll", @"C:\Foo\Bar.dll.config");

        RecentlyUsedAssembly firstAssembly = Assert.Single(mruList);
        Assert.Equal(@"C:\Foo\Bar.dll", firstAssembly.AssemblyFilename);
        Assert.Equal(@"C:\Foo\Bar.dll.config", firstAssembly.ConfigFilename);
    }

    [Fact]
    [RecentlyUsedAssemblyListRollback]
    public void AssemblyAddedToListPersistsBeyondListLifetime()
    {
        new RecentlyUsedAssemblyList().Add(@"C:\Foo\Bar.dll", @"C:\Foo\Bar.dll.config");

        RecentlyUsedAssembly firstAssembly = Assert.Single(new RecentlyUsedAssemblyList());
        Assert.Equal(@"C:\Foo\Bar.dll", firstAssembly.AssemblyFilename);
        Assert.Equal(@"C:\Foo\Bar.dll.config", firstAssembly.ConfigFilename);
    }

    [Fact]
    [RecentlyUsedAssemblyListRollback]
    public void NewAssemblysAreAddedToTheTopOfList()
    {
        RecentlyUsedAssemblyList mruList = new RecentlyUsedAssemblyList();

        mruList.Add(@"C:\Foo\Bar.dll", null);
        mruList.Add(@"C:\Baz\Biff.dll", null);

        RecentlyUsedAssembly firstAssembly = mruList.First();
        Assert.Equal(@"C:\Baz\Biff.dll", firstAssembly.AssemblyFilename);
    }

    [Fact]
    [RecentlyUsedAssemblyListRollback]
    public void ReAddingAlreadyPresentAssemblyReordersAssemblyToTopOfList()
    {
        RecentlyUsedAssemblyList mruList = new RecentlyUsedAssemblyList();

        mruList.Add(@"C:\Foo\Bar.dll", null);
        mruList.Add(@"C:\Baz\Biff.dll", null);
        mruList.Add(@"C:\Foo\Bar.dll", null);

        RecentlyUsedAssembly firstAssembly = mruList.First();
        Assert.Equal(2, mruList.Count());
        Assert.Equal(@"C:\Foo\Bar.dll", firstAssembly.AssemblyFilename);
    }

    [Fact]
    [RecentlyUsedAssemblyListRollback]
    public void AddingMoreThanMaximumNumberOfAssemblysPushesOldestAssemblyOffTheList()
    {
        RecentlyUsedAssemblyList mruList = new RecentlyUsedAssemblyList(1);

        mruList.Add(@"C:\Foo\Bar.dll", null);
        mruList.Add(@"C:\Baz\Biff.dll", null);

        RecentlyUsedAssembly firstAssembly = Assert.Single(mruList);
        Assert.Equal(@"C:\Baz\Biff.dll", firstAssembly.AssemblyFilename);
    }

    class RecentlyUsedAssemblyListRollbackAttribute : BeforeAfterTestAttribute
    {
        List<RecentlyUsedAssembly> assemblies = new List<RecentlyUsedAssembly>();

        public override void After(MethodInfo methodUnderTest)
        {
            RecentlyUsedAssemblyList.SaveAssemblyList(assemblies);
        }

        public override void Before(MethodInfo methodUnderTest)
        {
            assemblies = RecentlyUsedAssemblyList.LoadAssemblyList();
            RecentlyUsedAssemblyList.ClearAssemblyList();
        }
    }
}