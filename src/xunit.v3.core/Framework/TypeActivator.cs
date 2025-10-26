using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A static class intended to store the active type activator.
/// </summary>
public static class TypeActivator
{
	static ITypeActivator? current;

	/// <summary>
	/// Gets or sets the type activator.
	/// </summary>
	public static ITypeActivator Current
	{
		get => current ?? DefaultTypeActivator.Instance;
		set => current = Guard.ArgumentNotNull(value, nameof(Current));
	}

	sealed class DefaultTypeActivator : ITypeActivator
	{
		public static DefaultTypeActivator Instance { get; } = new();

		object ITypeActivator.CreateInstance(
			ConstructorInfo constructor,
			object?[]? arguments,
			Func<Type, IReadOnlyCollection<ParameterInfo>, string> missingArgumentMessageFormatter)
		{
			Guard.ArgumentNotNull(constructor);
			Guard.ArgumentNotNull(missingArgumentMessageFormatter);

			var type =
				constructor.ReflectedType
					?? constructor.DeclaringType
					?? throw new ArgumentException("Untyped constructors are not permitted", nameof(constructor));

			if (arguments is not null)
			{
				var parameters = constructor.GetParameters();
				if (parameters.Length != arguments.Length)
					throw new TestPipelineException(
						string.Format(
							CultureInfo.CurrentCulture,
							"Cannot create type '{0}' due to parameter count mismatch (needed {1}, got {2})",
							type.SafeName(),
							parameters.Length,
							arguments.Length
						)
					);

				var missingArguments =
					arguments
						.Select((a, idx) => a is Missing ? parameters[idx] : null)
						.WhereNotNull()
						.CastOrToReadOnlyCollection();

				if (missingArguments.Count != 0)
					throw new TestPipelineException(missingArgumentMessageFormatter(type, missingArguments));
			}

			return constructor.Invoke(arguments);
		}
	}
}
