using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Gui;

public class RecentlyUsedProjectListTests
{
    [Fact]
    [RecentlyUsedProjectListRollback]
    public void AddedProjectIsInList()
    {
        RecentlyUsedProjectList mruList = new RecentlyUsedProjectList();

        mruList.Add(@"C:\Foo\Bar.xunit");

        string filename = Assert.Single(mruList);
        Assert.Equal(@"C:\Foo\Bar.xunit", filename);
    }

    [Fact]
    [RecentlyUsedProjectListRollback]
    public void ProjectAddedToListPersistsBeyondListLifetime()
    {
        new RecentlyUsedProjectList().Add(@"C:\Foo\Bar.xunit");

        string filename = Assert.Single(new RecentlyUsedProjectList());
        Assert.Equal(@"C:\Foo\Bar.xunit", filename);
    }

    [Fact]
    [RecentlyUsedProjectListRollback]
    public void NewProjectsAreAddedToTheTopOfList()
    {
        RecentlyUsedProjectList mruList = new RecentlyUsedProjectList();

        mruList.Add(@"C:\Foo\Bar.xunit");
        mruList.Add(@"C:\Baz\Biff.xunit");

        Assert.Equal(@"C:\Baz\Biff.xunit", mruList.First());
    }

    [Fact]
    [RecentlyUsedProjectListRollback]
    public void ReAddingAlreadyPresentProjectReordersProjectToTopOfList()
    {
        RecentlyUsedProjectList mruList = new RecentlyUsedProjectList();

        mruList.Add(@"C:\Foo\Bar.xunit");
        mruList.Add(@"C:\Baz\Biff.xunit");
        mruList.Add(@"C:\Foo\Bar.xunit");

        Assert.Equal(2, mruList.Count());
        Assert.Equal(@"C:\Foo\Bar.xunit", mruList.First());
    }

    [Fact]
    [RecentlyUsedProjectListRollback]
    public void AddingMoreThanMaximumNumberOfProjectsPushesOldestProjectOffTheList()
    {
        RecentlyUsedProjectList mruList = new RecentlyUsedProjectList(1);

        mruList.Add(@"C:\Foo\Bar.xunit");
        mruList.Add(@"C:\Baz\Biff.xunit");

        string filename = Assert.Single(mruList);
        Assert.Equal(@"C:\Baz\Biff.xunit", filename);
    }

    class RecentlyUsedProjectListRollbackAttribute : BeforeAfterTestAttribute
    {
        List<string> projects = new List<string>();

        public override void After(MethodInfo methodUnderTest)
        {
            RecentlyUsedProjectList.SaveProjectList(projects);
        }

        public override void Before(MethodInfo methodUnderTest)
        {
            projects = RecentlyUsedProjectList.LoadProjectList();
            RecentlyUsedProjectList.ClearProjectList();
        }
    }
}