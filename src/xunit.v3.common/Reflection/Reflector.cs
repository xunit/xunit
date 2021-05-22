using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// Wrapper to return IReflection*Info implementations for System.Reflection types.
	/// </summary>
	public static class Reflector
	{
		internal readonly static object?[] EmptyArgs = new object?[0];
		internal readonly static Type[] EmptyTypes = new Type[0];

		readonly static MethodInfo EnumerableCast =
			typeof(Enumerable)
				.GetRuntimeMethods()
				.First(
					m => m.Name == "Cast"
					&& m.GetParameters().Length == 1
					&& m.GetParameters()[0].ParameterType == typeof(IEnumerable)
				);
		readonly static MethodInfo EnumerableToArray =
			typeof(Enumerable)
				.GetRuntimeMethods()
				.First(
					m => m.Name == "ToArray"
					&& m.GetParameters().Length == 1
					&& m.GetParameters()[0].ParameterType.IsGenericType
					&& m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
				);

		/// <summary>
		/// Converts arguments into their target types. Can be particularly useful when pulling attribute
		/// constructor arguments, whose types may not strictly match the parameter types.
		/// </summary>
		/// <param name="args">The arguments to be converted.</param>
		/// <param name="types">The target types for the conversion.</param>
		/// <returns>The converted arguments.</returns>
		public static object?[] ConvertArguments(object?[]? args, Type[]? types)
		{
			if (args == null)
				args = EmptyArgs;
			if (types == null)
				types = EmptyTypes;

			if (args.Length == types.Length)
				for (var idx = 0; idx < args.Length; idx++)
					args[idx] = ConvertArgument(args[idx], types[idx]);

			return args;
		}

		internal static object? ConvertArgument(object? arg, Type type)
		{
			if (arg != null && !type.IsAssignableFrom(arg.GetType()))
			{
				try
				{
					if (type.IsArray)
					{
						var elementType = type.GetElementType();
						if (elementType == null)
							throw new ArgumentException("Could not determine array element type", nameof(type));

						var enumerable = (IEnumerable<object>)arg;
						var castMethod = EnumerableCast.MakeGenericMethod(elementType);
						var toArrayMethod = EnumerableToArray.MakeGenericMethod(elementType);
						return toArrayMethod.Invoke(null, new object?[] { castMethod.Invoke(null, new object[] { enumerable }) });
					}
					else
					{
						if (type == typeof(Guid))
							return Guid.Parse(arg.ToString()!);

						if (type == typeof(DateTime))
							return DateTime.Parse(arg.ToString()!, CultureInfo.InvariantCulture);

						if (type == typeof(DateTimeOffset))
							return DateTimeOffset.Parse(arg.ToString()!, CultureInfo.InvariantCulture);

						return Convert.ChangeType(arg, type);
					}
				}
				catch { } // Eat conversion-related exceptions; they'll get re-surfaced during execution
			}

			return arg;
		}

		/// <summary>
		/// Converts an <see cref="Assembly"/> into an <see cref="_IReflectionAssemblyInfo"/>.
		/// </summary>
		/// <param name="assembly">The assembly to wrap.</param>
		/// <returns>The wrapper</returns>
		[return: NotNullIfNotNull("assembly")]
		public static _IReflectionAssemblyInfo? Wrap(Assembly? assembly) =>
			assembly == null ? null : new ReflectionAssemblyInfo(assembly);

		/// <summary>
		/// Converts an <see cref="Attribute"/> into an <see cref="_IAttributeInfo"/> using reflection.
		/// </summary>
		/// <param name="attribute">The attribute to wrap.</param>
		/// <returns>The wrapper</returns>
		[return: NotNullIfNotNull("attribute")]
		public static _IReflectionAttributeInfo? Wrap(CustomAttributeData? attribute) =>
			attribute == null ? null : new ReflectionAttributeInfo(attribute);

		/// <summary>
		/// Converts a <see cref="MethodInfo"/> into an <see cref="_IMethodInfo"/> using reflection.
		/// </summary>
		/// <param name="method">The method to wrap</param>
		/// <returns>The wrapper</returns>
		[return: NotNullIfNotNull("method")]
		public static _IReflectionMethodInfo? Wrap(MethodInfo? method) =>
			method == null ? null : new ReflectionMethodInfo(method);

		/// <summary>
		/// Converts a <see cref="ParameterInfo"/> into an <see cref="_IParameterInfo"/> using reflection.
		/// </summary>
		/// <param name="parameter">THe parameter to wrap</param>
		/// <returns>The wrapper</returns>
		[return: NotNullIfNotNull("parameter")]
		public static _IReflectionParameterInfo? Wrap(ParameterInfo? parameter) =>
			parameter == null ? null : new ReflectionParameterInfo(parameter);

		/// <summary>
		/// Converts a <see cref="Type"/> into an <see cref="_ITypeInfo"/> using reflection.
		/// </summary>
		/// <param name="type">The type to wrap</param>
		/// <returns>The wrapper</returns>
		[return: NotNullIfNotNull("type")]
		public static _IReflectionTypeInfo? Wrap(Type? type) =>
			type == null ? null : new ReflectionTypeInfo(type);
	}
}
