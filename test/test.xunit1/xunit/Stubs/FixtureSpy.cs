using System;
using Xunit;

public class FixtureSpy : IUseFixture<FixtureSpyData>, IDisposable
{
    public static string callOrder;
    public static int ctorCalled;
    public static int dataCtorCalled;
    public static int dataDisposeCalled;
    public static int disposeCalled;
    public static int setFixtureCalled;

    public FixtureSpy()
    {
        callOrder += "ctor ";
        ctorCalled++;
    }

    public virtual void Dispose()
    {
        callOrder += "dispose ";
        disposeCalled++;
    }

    public static void Reset()
    {
        callOrder = "";
        ctorCalled = 0;
        disposeCalled = 0;
        dataCtorCalled = 0;
        dataDisposeCalled = 0;
        setFixtureCalled = 0;
    }

    public void SetFixture(FixtureSpyData data)
    {
        setFixtureCalled++;
        callOrder += "setFixture ";
    }
}

public class FixtureSpyData : IDisposable
{
    public FixtureSpyData()
    {
        FixtureSpy.dataCtorCalled++;
        FixtureSpy.callOrder += "ctorData ";
    }

    public void Dispose()
    {
        FixtureSpy.dataDisposeCalled++;
        FixtureSpy.callOrder += "disposeData ";
    }
}
