using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Provides a class which wraps <see cref="IAttributeInfo"/> instances to implement <see cref="_IAttributeInfo"/>.
	/// </summary>
	public class Xunit3AttributeInfo : _IAttributeInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit3AttributeInfo"/> class.
		/// </summary>
		/// <param name="v2AttributeInfo">The v2 attribute info to wrap.</param>
		public Xunit3AttributeInfo(IAttributeInfo v2AttributeInfo)
		{
			V2AttributeInfo = Guard.ArgumentNotNull(nameof(v2AttributeInfo), v2AttributeInfo);
		}

		/// <summary>
		/// Gets the underlying xUnit.net v2 <see cref="IAttributeInfo"/> that this class is wrapping.
		/// </summary>
		public IAttributeInfo V2AttributeInfo { get; }

		/// <inheritdoc/>
		public IReadOnlyCollection<object?> GetConstructorArguments() =>
			V2AttributeInfo.GetConstructorArguments().CastOrToReadOnlyCollection();

		/// <inheritdoc/>
		public IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName) =>
			V2AttributeInfo.GetCustomAttributes(assemblyQualifiedAttributeTypeName).Select(a => new Xunit3AttributeInfo(a)).CastOrToReadOnlyCollection();

		/// <inheritdoc/>
		public TValue GetNamedArgument<TValue>(string argumentName) =>
			V2AttributeInfo.GetNamedArgument<TValue>(argumentName);
	}
}
