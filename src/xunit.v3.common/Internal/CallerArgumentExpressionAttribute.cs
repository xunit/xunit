#if !NETCOREAPP3_0_OR_GREATER

namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class CallerArgumentExpressionAttribute : Attribute
{
	public CallerArgumentExpressionAttribute(string parameterName)
	{
		ParameterName = parameterName;
	}

	public string ParameterName { get; }
}

#endif
