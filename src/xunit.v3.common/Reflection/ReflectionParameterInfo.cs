using System.Reflection;
using Xunit.Abstractions;
using Xunit.Internal;

namespace Xunit.Sdk
{
	/// <summary>
	/// Reflection-based implementation of <see cref="IReflectionParameterInfo"/>.
	/// </summary>
	public class ReflectionParameterInfo : IReflectionParameterInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ReflectionParameterInfo"/> class.
		/// </summary>
		/// <param name="parameterInfo">The parameter to be wrapped.</param>
		public ReflectionParameterInfo(ParameterInfo parameterInfo)
		{
			ParameterInfo = Guard.ArgumentNotNull(nameof(parameterInfo), parameterInfo);
		}

		/// <inheritdoc/>
		public string Name => ParameterInfo.Name!;

		/// <inheritdoc/>
		public ParameterInfo ParameterInfo { get; }

		/// <inheritdoc/>
		public ITypeInfo ParameterType => Reflector.Wrap(ParameterInfo.ParameterType);
	}
}
