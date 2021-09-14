using System;
using System.Security.Principal;
using System.Threading;
using Xunit;
using Xunit.Extensions;

namespace Xunit1.Extensions
{
    public class AssumeIdentityAttributeFacts
    {
        [Fact]
        public void CreatingWithNullIdentityThrows()
        {
            Assert.Throws<ArgumentException>(() => new AssumeIdentityAttribute(null));
        }

        [Fact]
        public void CreatingWithEmptyIdentityThrows()
        {
            Assert.Throws<ArgumentException>(() => new AssumeIdentityAttribute(""));
        }

        [Fact]
        public void IdentityIsChangedWithinTest()
        {
            AssumeIdentityAttribute attr = new AssumeIdentityAttribute("007");

            attr.Before(null);

            Assert.True(Thread.CurrentPrincipal.IsInRole("007"));
            attr.After(null);
        }

        [Fact]
        public void OriginalIdentityIsReinstatedAfterTest()
        {
            IPrincipal original = Thread.CurrentPrincipal;
            AssumeIdentityAttribute attr = new AssumeIdentityAttribute("007");
            attr.Before(null);

            attr.After(null);

            Assert.Equal(original, Thread.CurrentPrincipal);
        }
    }
}
