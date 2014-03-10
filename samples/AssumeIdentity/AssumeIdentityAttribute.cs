using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using Xunit.Sdk;


/// <summary>
/// Apply this attribute to your test method to replace the 
/// <see cref="Thread.CurrentPrincipal"/> with another role.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
[SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is designed as an extensibility point.")]
public class AssumeIdentityAttribute : BeforeAfterTestAttribute
{
    IPrincipal originalPrincipal;

    /// <summary>
    /// Replaces the identity of the current thread with <paramref name="roleName"/>.
    /// </summary>
    /// <param name="roleName">The role's name</param>
    public AssumeIdentityAttribute(string roleName)
    {
        Name = roleName;
    }

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Restores the original <see cref="Thread.CurrentPrincipal"/>.
    /// </summary>
    /// <param name="methodUnderTest">The method under test</param>
    public override void After(MethodInfo methodUnderTest)
    {
        Thread.CurrentPrincipal = originalPrincipal;
    }

    /// <summary>
    /// Stores the current <see cref="Thread.CurrentPrincipal"/> and replaces it with
    /// a new role identified in constructor.
    /// </summary>
    /// <param name="methodUnderTest">The method under test</param>
    public override void Before(MethodInfo methodUnderTest)
    {
        originalPrincipal = Thread.CurrentPrincipal;
        var identity = new GenericIdentity("xUnit");
        var principal = new GenericPrincipal(identity, new string[] { Name });
        Thread.CurrentPrincipal = principal;
    }
}

