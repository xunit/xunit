using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Provides a class which wraps <see cref="_IAttributeInfo"/> instances to implement <see cref="IAttributeInfo"/>.
	/// </summary>
	public class Xunit2AttributeInfo : LongLivedMarshalByRefObject, IAttributeInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit2AttributeInfo"/> class.
		/// </summary>
		/// <param name="v3AttributeInfo">The v3 attribute info to wrap.</param>
		public Xunit2AttributeInfo(_IAttributeInfo v3AttributeInfo)
		{
			V3AttributeInfo = Guard.ArgumentNotNull(nameof(v3AttributeInfo), v3AttributeInfo);
		}

		/// <summary>
		/// Gets the underlying xUnit.net v3 <see cref="_IAttributeInfo"/> that this class is wrapping.
		/// </summary>
		public _IAttributeInfo V3AttributeInfo { get; }

		/// <inheritdoc/>
		public IEnumerable<object?> GetConstructorArguments() =>
			V3AttributeInfo.GetConstructorArguments();

		/// <inheritdoc/>
		public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName) =>
			V3AttributeInfo
				.GetCustomAttributes(assemblyQualifiedAttributeTypeName)
				.Select(a => new Xunit2AttributeInfo(a))
				.CastOrToArray();

		/// <inheritdoc/>
		[return: MaybeNull]
		public TValue GetNamedArgument<TValue>(string argumentName) =>
			V3AttributeInfo.GetNamedArgument<TValue>(argumentName);
	}
}
