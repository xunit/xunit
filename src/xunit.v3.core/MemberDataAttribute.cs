using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Provides a data source for a data theory, with the data coming from one of the following sources:
/// 1. A public static property
/// 2. A public static field
/// 3. A public static method (with parameters)
/// </summary>
/// <remarks>
/// The member must return data in a form that is compatible, which means collections of <c>object?[]</c>,
/// <c>ITheoryDataRow</c>, or tuple values. Those collections may come via <see cref="IEnumerable{T}"/> or
/// <see cref="IAsyncEnumerable{T}"/>, and those collections may optionally be wrapped in either
/// <see cref="Task{TResult}"/> or <see cref="ValueTask{TResult}"/>.
/// </remarks>
/// <param name="memberName">
/// The name of the public static member on the test class that will provide the test data
/// It is recommended to use the <c>nameof</c> operator to ensure compile-time safety, e.g., <c>nameof(SomeMemberName)</c>.
/// </param>
/// <param name="arguments">The arguments to be passed to the member (only supported for methods; ignored for everything else)</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class MemberDataAttribute(
	string memberName,
	params object?[] arguments) :
		MemberDataAttributeBase(memberName, arguments)
{ }
