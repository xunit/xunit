using System;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.v2;

namespace Xunit.Sdk
{
	/// <summary>
	/// The implementation of <see cref="ITestFrameworkTypeDiscoverer"/> that supports attributes
	/// of type <see cref="TestFrameworkDiscovererAttribute"/>.
	/// </summary>
	public class TestFrameworkTypeDiscoverer : ITestFrameworkTypeDiscoverer
	{
		/// <inheritdoc/>
		public Type? GetTestFrameworkType(IAttributeInfo attribute)
		{
			Guard.ArgumentNotNull(nameof(attribute), attribute);

			var args = attribute.GetConstructorArguments().ToArray();
			if (args.Length == 1)
				return (Type)args[0];

			var stringArgs = args.Cast<string>().ToArray();
			return SerializationHelper.GetType(stringArgs[1], stringArgs[0]);
		}
	}
}
