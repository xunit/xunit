using System;
using System.Linq;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk;

/// <summary>
/// The implementation of <see cref="ITestFrameworkTypeDiscoverer"/> that supports attributes
/// of type <see cref="TestFrameworkDiscovererAttribute"/>.
/// </summary>
public class TestFrameworkTypeDiscoverer : ITestFrameworkTypeDiscoverer
{
	/// <inheritdoc/>
	public Type? GetTestFrameworkType(_IAttributeInfo attribute)
	{
		Guard.ArgumentNotNull(attribute);

		var args = attribute.GetConstructorArguments().ToArray();
		if (args.Length == 1)
			return (Type)args[0]!;

		var stringArgs = args.Cast<string>().ToArray();
		return TypeHelper.GetType(stringArgs[1], stringArgs[0]);
	}
}
