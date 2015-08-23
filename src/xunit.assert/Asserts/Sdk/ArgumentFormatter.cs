using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Xunit.Sdk
{
    /// <summary>
    /// Formats arguments for display in theories.
    /// </summary>
    static class ArgumentFormatter
    {
        const int MAX_DEPTH = 3;
        const int MAX_ENUMERABLE_LENGTH = 5;
        const int MAX_OBJECT_PARAMETER_COUNT = 5;
        const int MAX_STRING_LENGTH = 50;

        static readonly object[] EmptyObjects = new object[0];
        static readonly Type[] EmptyTypes = new Type[0];

        // List of system types => C# type names
        static readonly Dictionary<TypeInfo, string> TypeMappings = new Dictionary<TypeInfo, string>
        {
            { typeof(bool).GetTypeInfo(), "bool" },
            { typeof(byte).GetTypeInfo(), "byte" },
            { typeof(sbyte).GetTypeInfo(), "sbyte" },
            { typeof(char).GetTypeInfo(), "char" },
            { typeof(decimal).GetTypeInfo(), "decimal" },
            { typeof(double).GetTypeInfo(), "double" },
            { typeof(float).GetTypeInfo(), "float" },
            { typeof(int).GetTypeInfo(), "int" },
            { typeof(uint).GetTypeInfo(), "uint" },
            { typeof(long).GetTypeInfo(), "long" },
            { typeof(ulong).GetTypeInfo(), "ulong" },
            { typeof(object).GetTypeInfo(), "object" },
            { typeof(short).GetTypeInfo(), "short" },
            { typeof(ushort).GetTypeInfo(), "ushort" },
            { typeof(string).GetTypeInfo(), "string" },
        };

        /// <summary>
        /// Format the value for presentation.
        /// </summary>
        /// <param name="value">The value to be formatted.</param>
        /// <returns>The formatted value.</returns>
        public static string Format(object value)
        {
            return Format(value, 1);
        }

        static string Format(object value, int depth)
        {
            if (value == null)
                return "null";

            var valueAsType = value as Type;
            if (valueAsType != null)
                return string.Format("typeof({0})", new object[] { FormatTypeName(valueAsType) });

            if (value is char)
            {
                var charValue = (char)value;
                if (char.IsLetterOrDigit(charValue) || char.IsPunctuation(charValue) || char.IsSymbol(charValue) || charValue == ' ')
                    return string.Format("'{0}'", new object[] { value });

                return string.Format("0x{0:x4}", new object[] { (int)charValue });
            }

            if (value is DateTime || value is DateTimeOffset)
                return string.Format("{0:o}", new object[] { value });

            var stringParameter = value as string;
            if (stringParameter != null)
            {
                if (stringParameter.Length > MAX_STRING_LENGTH)
                    return string.Format("\"{0}\"...", new object[] { stringParameter.Substring(0, MAX_STRING_LENGTH) });

                return string.Format("\"{0}\"", new object[] { stringParameter });
            }

            var enumerable = value as IEnumerable;
            if (enumerable != null)
                return FormatEnumerable(enumerable.Cast<object>(), depth);

            var type = value.GetType();
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsValueType)
                return Convert.ToString(value, CultureInfo.CurrentCulture);

            var task = value as Task;
            if (task != null)
            {
                var typeParameters = typeInfo.GenericTypeArguments;
                var typeName = typeParameters.Length == 0 ? "Task" : string.Format("Task<{0}>", string.Join(",", typeParameters.Select(FormatTypeName)));
                return string.Format("{0} {{ Status = {1} }}", typeName, task.Status);
            }

#if PLATFORM_DOTNET
            var toString = type.GetRuntimeMethod("ToString", EmptyTypes);
#else
            var toString = type.GetMethod("ToString", EmptyTypes);
#endif

            if (toString != null && toString.DeclaringType != typeof(Object))
                return (string)toString.Invoke(value, EmptyObjects);

            return FormatComplexValue(value, depth, type);
        }

        static string FormatComplexValue(object value, int depth, Type type)
        {
            if (depth == MAX_DEPTH)
                return string.Format("{0} {{ ... }}", new object[] { type.Name });

            var fields = type.GetRuntimeFields()
                             .Where(f => f.IsPublic && !f.IsStatic)
                             .Select(f => new { name = f.Name, value = WrapAndGetFormattedValue(() => f.GetValue(value), depth) });
            var properties = type.GetRuntimeProperties()
                                 .Where(p => p.GetMethod != null && p.GetMethod.IsPublic && !p.GetMethod.IsStatic)
                                 .Select(p => new { name = p.Name, value = WrapAndGetFormattedValue(() => p.GetValue(value), depth) });
            var parameters = fields.Concat(properties)
                                   .OrderBy(p => p.name)
                                   .Take(MAX_OBJECT_PARAMETER_COUNT + 1)
                                   .ToList();

            if (parameters.Count == 0)
                return string.Format("{0} {{ }}", new object[] { type.Name });

            var formattedParameters = string.Join(", ", parameters.Take(MAX_OBJECT_PARAMETER_COUNT)
                                                                  .Select(p => string.Format("{0} = {1}", new object[] { p.name, p.value })));

            if (parameters.Count > MAX_OBJECT_PARAMETER_COUNT)
                formattedParameters += ", ...";

            return string.Format("{0} {{ {1} }}", new object[] { type.Name, formattedParameters });
        }

        static string FormatEnumerable(IEnumerable<object> enumerableValues, int depth)
        {
            if (depth == MAX_DEPTH)
                return "[...]";

            var values = enumerableValues.Take(MAX_ENUMERABLE_LENGTH + 1).ToList();
            var printedValues = string.Join(", ", values.Take(MAX_ENUMERABLE_LENGTH).Select(x => Format(x, depth + 1)));

            if (values.Count > MAX_ENUMERABLE_LENGTH)
                printedValues += ", ...";

            return string.Format("[{0}]", new object[] { printedValues });
        }

        static string FormatTypeName(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            var arraySuffix = "";

            // Deconstruct and re-construct array
            while (typeInfo.IsArray)
            {
                var rank = typeInfo.GetArrayRank();
                arraySuffix += string.Format("[{0}]", new object[] { new string(',', rank - 1) });
                typeInfo = typeInfo.GetElementType().GetTypeInfo();
            }

            // Map C# built-in type names
            string result;
            if (TypeMappings.TryGetValue(typeInfo, out result))
                return result + arraySuffix;

            // Strip off generic suffix
            var name = typeInfo.FullName;
            var tickIdx = name.IndexOf('`');
            if (tickIdx > 0)
                name = name.Substring(0, tickIdx);

            if (typeInfo.IsGenericTypeDefinition)
                name = string.Format("{0}<{1}>", new object[] { name, new string(',', typeInfo.GenericTypeParameters.Length - 1) });
            else if (typeInfo.IsGenericType)
            {
                if (typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
                    name = FormatTypeName(typeInfo.GenericTypeArguments[0]) + "?";
                else
                    name = string.Format("{0}<{1}>", new object[] { name, string.Join(", ", typeInfo.GenericTypeArguments.Select(FormatTypeName)) });
            }

            return name + arraySuffix;
        }

        static string WrapAndGetFormattedValue(Func<object> getter, int depth)
        {
            try
            {
                return Format(getter(), depth + 1);
            }
            catch (Exception ex)
            {
                return string.Format("(throws {0})", new object[] { UnwrapException(ex).GetType().Name });
            }
        }

        static Exception UnwrapException(Exception ex)
        {
            while (true)
            {
                var tiex = ex as TargetInvocationException;
                if (tiex == null)
                    return ex;

                ex = tiex.InnerException;
            }
        }
    }
}
