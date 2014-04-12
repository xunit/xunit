using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Xunit.Sdk
{
    /// <summary>
    /// Formats arguments for display in theories.
    /// </summary>
    public static class ArgumentFormatter
    {
        /// <summary>
        /// Format the value for presentation.
        /// </summary>
        /// <param name="value">The value to be formatted.</param>
        /// <returns>The formatted value.</returns>
        public static string Format(object value)
        {
            return Format(value, 1);
        }

        private static string Format(object value, int depth)
        {
            if (value == null)
                return "null";

            if (value is char)
                return String.Format("'{0}'", value);

            string stringParameter = value as string;
            if (stringParameter != null)
            {
                if (stringParameter.Length > 50)
                    return String.Format("\"{0}\"...", stringParameter.Substring(0, 50));

                return String.Format("\"{0}\"", stringParameter);
            }

            var enumerable = value as IEnumerable;
            if (enumerable != null)
                return String.Format("[{0}]", String.Join(", ", enumerable.Cast<object>().Select(x => Format(x, depth + 1))));

            var type = value.GetType();
            if (type.GetTypeInfo().IsValueType)
                return Convert.ToString(value, CultureInfo.CurrentCulture);

            if (depth == 3)
                return String.Format("{0} {{ ... }}", type.Name);

            var fields = type.GetRuntimeFields()
                             .Where(f => f.IsPublic && !f.IsStatic)
                             .Select(f => new { name = f.Name, value = WrapAndGetFormattedValue(() => f.GetValue(value), depth) })
                             .ToList();
            var properties = type.GetRuntimeProperties()
                                 .Where(p => p.GetMethod != null && p.GetMethod.IsPublic && !p.GetMethod.IsStatic)
                                 .Select(p => new { name = p.Name, value = WrapAndGetFormattedValue(() => p.GetValue(value), depth) })
                                 .ToList();
            var formattedParameters = fields.Concat(properties)
                                            .OrderBy(p => p.name)
                                            .Select(p => String.Format("{0} = {1}", p.name, p.value))
                                            .ToList();
            var parameterValues = formattedParameters.Count == 0 ? "{ }" : String.Format("{{ {0} }}", String.Join(", ", formattedParameters));

            return String.Format("{0} {1}", type.Name, parameterValues);
        }

        private static string WrapAndGetFormattedValue(Func<object> getter, int depth)
        {
            try
            {
                return Format(getter(), depth + 1);
            }
            catch (Exception ex)
            {
                return String.Format("(throws {0})", ex.Unwrap().GetType().Name);
            }
        }
    }
}
