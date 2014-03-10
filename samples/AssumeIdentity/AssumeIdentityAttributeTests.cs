using System.Security;
using System.Security.Permissions;
using System.Threading;
using Xunit;

public class AssumeIdentityAttributeTests
{
    [Fact, AssumeIdentity("casper")]
    public void AttributeChangesRoleInTestMethod()
    {
        Assert.True(Thread.CurrentPrincipal.IsInRole("casper"));
    }

    [Fact]
    public void CallingSecuredMethodWillThrow()
    {
        Assert.Throws<SecurityException>(() => DefeatVillian());
    }

    [Fact, AssumeIdentity("Q")]
    public void CallingSecuredMethodWithWrongIdentityWillThrow()
    {
        Assert.Throws<SecurityException>(() => DefeatVillian());
    }

    [Fact, AssumeIdentity("007")]
    public void CallingSecuredMethodWithAssumedIdentityPasses()
    {
        Assert.DoesNotThrow(() => DefeatVillian());
    }

    [PrincipalPermission(SecurityAction.Demand, Role = "007")]
    public void DefeatVillian() { }
}