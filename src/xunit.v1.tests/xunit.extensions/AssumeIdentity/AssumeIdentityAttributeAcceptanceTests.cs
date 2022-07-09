using System.Security;
using System.Security.Permissions;
using System.Threading;
using Xunit;
using Xunit.Extensions;

namespace Xunit1.Extensions
{
	public class AssumeIdentityAttributeAcceptanceTests
	{
		[Fact, AssumeIdentity("casper")]
		public void AttributeChangesRoleInTestMethod()
		{
			Assert.True(Thread.CurrentPrincipal.IsInRole("casper"));
		}

		[Fact]
		public void CallingSecuredMethodWillThrow()
		{
			// Mono does not appear to properly support PrincipalPermission
			if (!EnvironmentHelper.IsMono)
				Assert.Throws<SecurityException>(() => DefeatVillain());
		}

		[Fact, AssumeIdentity("Q")]
		public void CallingSecuredMethodWithWrongIdentityWillThrow()
		{
			// Mono does not appear to properly support PrincipalPermission
			if (!EnvironmentHelper.IsMono)
				Assert.Throws<SecurityException>(() => DefeatVillain());
		}

		[Fact, AssumeIdentity("007")]
		public void CallingSecuredMethodWithAssumedIdentityPasses()
		{
			// Mono does not appear to properly support PrincipalPermission
			if (!EnvironmentHelper.IsMono)
				Assert.DoesNotThrow(() => DefeatVillain());
		}

		[PrincipalPermission(SecurityAction.Demand, Role = "007")]
		public void DefeatVillain() { }
	}
}
