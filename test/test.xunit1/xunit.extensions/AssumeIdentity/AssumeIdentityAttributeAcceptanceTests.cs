using System;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using Xunit;
using Xunit.Extensions;

namespace Xunit1.Extensions
{
    public class AssumeIdentityAttributeAcceptanceTests
    {
        static readonly bool isMono;

        static AssumeIdentityAttributeAcceptanceTests()
        {
            isMono = Type.GetType("Mono.Runtime") != null;
        }

        [Fact, AssumeIdentity("casper")]
        public void AttributeChangesRoleInTestMethod()
        {
            Assert.True(Thread.CurrentPrincipal.IsInRole("casper"));
        }

        [Fact]
        public void CallingSecuredMethodWillThrow()
        {
            if (!isMono)  // Mono does not appear to properly support PrincipalPermission
                Assert.Throws<SecurityException>(() => DefeatVillian());
        }

        [Fact, AssumeIdentity("Q")]
        public void CallingSecuredMethodWithWrongIdentityWillThrow()
        {
            if (!isMono)  // Mono does not appear to properly support PrincipalPermission
                Assert.Throws<SecurityException>(() => DefeatVillian());
        }

        [Fact, AssumeIdentity("007")]
        public void CallingSecuredMethodWithAssumedIdentityPasses()
        {
            if (!isMono)  // Mono does not appear to properly support PrincipalPermission
                Assert.DoesNotThrow(() => DefeatVillian());
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "007")]
        public void DefeatVillian() { }
    }
}
