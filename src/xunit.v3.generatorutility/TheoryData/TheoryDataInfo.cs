#nullable enable

using System;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Xunit.Generators
{
	/// <summary>
	/// Represents information about a theory data source.
	/// </summary>
	public class TheoryDataInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TheoryDataInfo"/> class.
		/// </summary>
		/// <param name="enumerableType">The type of the enumerable</param>
		/// <param name="isAsync">A flag to indicate if the data source is async (i.e., wrapped in
		/// <see cref="Task{TResult}"/> or <see cref="ValueTask{TResult}"/>)</param>
		/// <param name="isAsyncEnumerable">A flag to indicate if the data source is async enumerable
		/// (i.e., using <c>IAsyncEnumerable&lt;T&gt;</c>)</param>
		public TheoryDataInfo(
			ITypeSymbol enumerableType,
			bool isAsync,
			bool isAsyncEnumerable)
		{
			EnumerableType = enumerableType ?? throw new ArgumentNullException(nameof(enumerableType));
			IsAsync = isAsync;
			IsAsyncEnumerable = isAsyncEnumerable;
		}

		/// <summary>
		/// Gets the type of the enumerable.
		/// </summary>
		/// <remarks>
		/// Returns the <c>T</c> for <c>IEnumerable&lt;T&gt;</c> or <c>IAsyncEnumerable&lt;T&gt;</c>,
		/// or <see cref="object"/> for <see cref="IEnumerable"/>.
		/// </remarks>
		public ITypeSymbol EnumerableType { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the enumerable is wrapped in <see cref="Task{TResult}"/>
		/// or <see cref="ValueTask{TResult}"/> (and thus requires an <c>await</c> in the generated code).
		/// </summary>
		public bool IsAsync { get; }

		/// <summary>
		/// Returns <see langword="true"/> for <c>IAsyncEnumerable&lt;T&gt;</c>; <see langword="false"/>, otherwise.
		/// </summary>
		public bool IsAsyncEnumerable { get; }
	}
}
