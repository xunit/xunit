using System.Globalization;
using System.Reflection;
using Xunit.Abstractions;

/// <summary>
/// Extension methods for <see cref="IAssemblyInfo"/>.
/// </summary>
public static class IAssemblyInfoExtensions
{
    /// <summary>
    /// Computes the simple assembly name from <see cref="IAssemblyInfo.Name"/>.
    /// </summary>
    /// <returns>The simple assembly name.</returns>
    public static string SimpleAssemblyName(this IAssemblyInfo assemblyInfo)
    {
        Guard.ArgumentNotNull(nameof(assemblyInfo), assemblyInfo);
        Guard.ArgumentNotNullOrEmpty(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", nameof(assemblyInfo), nameof(IAssemblyInfo.Name)), assemblyInfo.Name);

        var parsedAssemblyName = new AssemblyName(assemblyInfo.Name);
        Guard.ArgumentValid(nameof(assemblyInfo), !string.IsNullOrEmpty(parsedAssemblyName.Name), "{0}.{1} must include a name component", nameof(assemblyInfo), nameof(IAssemblyInfo.Name));

        return parsedAssemblyName.Name;
    }
}
