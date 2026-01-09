using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

internal static class RegisteredResultWriters<TWriter, TWriterRegistration>
	where TWriterRegistration : IRegisterResultWriterAttribute
{
	public static IReadOnlyDictionary<string, TWriter> Get(
		string writerTypeDescription,
		Assembly assembly,
		List<string>? messages = null)
	{
		Guard.ArgumentNotNull(writerTypeDescription);
		Guard.ArgumentNotNull(assembly);

		messages ??= [];

		var result = new Dictionary<string, TWriter>(StringComparer.OrdinalIgnoreCase);

		foreach (var attribute in assembly.GetMatchingCustomAttributes<TWriterRegistration>(messages))
		{
			var resultWriterType = attribute.ResultWriterType;
			if (resultWriterType is null)
			{
				messages?.Add(
					string.Format(
						CultureInfo.CurrentCulture,
						"{0} result writer type '{1}' returned null from {2}",
						writerTypeDescription,
						attribute.GetType().SafeName(),
						nameof(IRegisterResultWriterAttribute.ResultWriterType)
					)
				);
				continue;
			}

			try
			{
				if (Activator.CreateInstance(resultWriterType) is not TWriter resultWriter)
				{
					messages?.Add(
						string.Format(
							CultureInfo.CurrentCulture,
							"{0} result writer type '{1}' does not implement '{2}'",
							writerTypeDescription,
							resultWriterType.SafeName(),
							typeof(TWriter).SafeName()
						)
					);
					continue;
				}

				if (result.TryGetValue(attribute.ID, out var existingWriter))
				{
					messages?.Add(
						string.Format(
							CultureInfo.CurrentCulture,
							"{0} result writer type '{1}' conflicts with existing result writer type '{2}' with the same ID",
							writerTypeDescription,
							resultWriterType.SafeName(),
							existingWriter?.GetType().SafeName()
						)
					);
					continue;
				}

				result.Add(attribute.ID, resultWriter);
			}
			catch (Exception ex)
			{
				messages?.Add(
					string.Format(
						CultureInfo.CurrentCulture,
						"Exception creating {0} result writer type '{1}': {2}",
						writerTypeDescription,
						resultWriterType.SafeName(),
						ex.Unwrap()
					)
				);
			}
		}

		return result;
	}
}
