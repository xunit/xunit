using System;
using System.Collections;
using System.Collections.Generic;
#if NET452 || NET35 || NETSTANDARD2_0
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
#if NET452 || NETSTANDARD2_0
using System.Runtime.ExceptionServices;
#endif
using System.Text;
#endif
using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Utility classes for dealing with Exception objects.
    /// </summary>
    public static class ExceptionUtility
    {
        /// <summary>
        /// Combines multiple levels of messages into a single message.
        /// </summary>
        /// <param name="failureInfo">The failure information from which to get the messages.</param>
        /// <returns>The combined string.</returns>
        public static string CombineMessages(IFailureInformation failureInfo)
        {
            return GetMessage(failureInfo, 0, 0);
        }

        /// <summary>
        /// Combines multiple levels of stack traces into a single stack trace.
        /// </summary>
        /// <param name="failureInfo">The failure information from which to get the stack traces.</param>
        /// <returns>The combined string.</returns>
        public static string CombineStackTraces(IFailureInformation failureInfo)
        {
            return GetStackTrace(failureInfo, 0);
        }

        static bool ExcludeStackFrame(string stackFrame)
        {
            Guard.ArgumentNotNull("stackFrame", stackFrame);

            return stackFrame.StartsWith("at Xunit.", StringComparison.Ordinal);
        }

        static string FilterStackTrace(string stack)
        {
            if (stack == null)
                return null;

            var results = new List<string>();

            foreach (var line in SplitLines(stack))
            {
                var trimmedLine = line.TrimStart();
                if (!ExcludeStackFrame(trimmedLine))
                    results.Add(line);
            }

            return string.Join(Environment.NewLine, results.ToArray());
        }

        static string GetAt(string[] values, int index)
        {
            if (values == null || index < 0 || values.Length <= index)
                return string.Empty;

            return values[index] ?? string.Empty;
        }

        static int GetAt(int[] values, int index)
        {
            if (values == null || values.Length <= index)
                return -1;

            return values[index];
        }

        static string GetMessage(IFailureInformation failureInfo, int index, int level)
        {
            var result = "";

            if (level > 0)
            {
                for (var idx = 0; idx < level; idx++)
                    result += "----";

                result += " ";
            }

            var exceptionType = GetAt(failureInfo.ExceptionTypes, index);
            if (GetNamespace(exceptionType) != "Xunit.Sdk")
                result += exceptionType + " : ";

            result += GetAt(failureInfo.Messages, index);

            for (var subIndex = index + 1; subIndex < failureInfo.ExceptionParentIndices.Length; ++subIndex)
                if (GetAt(failureInfo.ExceptionParentIndices, subIndex) == index)
                    result += Environment.NewLine + GetMessage(failureInfo, subIndex, level + 1);

            return result;
        }

        static string GetNamespace(string exceptionType)
        {
            var nsIndex = exceptionType.LastIndexOf('.');
            if (nsIndex > 0)
                return exceptionType.Substring(0, nsIndex);

            return "";
        }

        static string GetStackTrace(IFailureInformation failureInfo, int index)
        {
            var result = FilterStackTrace(GetAt(failureInfo.StackTraces, index));

            var children = new List<int>();
            for (var subIndex = index + 1; subIndex < failureInfo.ExceptionParentIndices.Length; ++subIndex)
                if (GetAt(failureInfo.ExceptionParentIndices, subIndex) == index)
                    children.Add(subIndex);

            if (children.Count > 1)
                for (var idx = 0; idx < children.Count; ++idx)
                    result += $"{Environment.NewLine}----- Inner Stack Trace #{idx + 1} ({GetAt(failureInfo.ExceptionTypes, children[idx])}) -----{Environment.NewLine}{GetStackTrace(failureInfo, children[idx])}";
            else if (children.Count == 1)
                result += $"{Environment.NewLine}----- Inner Stack Trace -----{Environment.NewLine}{GetStackTrace(failureInfo, children[0])}";

            return result;
        }

        // Our own custom string.Split because Silverlight/CoreCLR doesn't support the version we were using
        static IEnumerable<string> SplitLines(string input)
        {
            while (true)
            {
                var idx = input.IndexOf(Environment.NewLine, StringComparison.Ordinal);

                if (idx < 0)
                {
                    yield return input;
                    break;
                }

                yield return input.Substring(0, idx);
                input = input.Substring(idx + Environment.NewLine.Length);
            }
        }

        /// <summary>
        /// Unwraps exceptions and their inner exceptions.
        /// </summary>
        /// <param name="ex">The exception to be converted.</param>
        /// <returns>The failure information.</returns>
        public static IFailureInformation ConvertExceptionToFailureInformation(Exception ex)
        {
            var exceptionTypes = new List<string>();
            var messages = new List<string>();
            var stackTraces = new List<string>();
            var indices = new List<int>();

            ConvertExceptionToFailureInformation(ex, -1, exceptionTypes, messages, stackTraces, indices);

            return new FailureInformation
            {
                ExceptionParentIndices = indices.ToArray(),
                ExceptionTypes = exceptionTypes.ToArray(),
                Messages = messages.ToArray(),
                StackTraces = stackTraces.ToArray(),
            };
        }

        static void ConvertExceptionToFailureInformation(Exception ex, int parentIndex, List<string> exceptionTypes, List<string> messages, List<string> stackTraces, List<int> indices)
        {
            var myIndex = exceptionTypes.Count;
            
            exceptionTypes.Add(ex.GetType().FullName);
            messages.Add(ex.Message);
            stackTraces.Add(ex.GetCleanStackTrace());
            indices.Add(parentIndex);

#if XUNIT_FRAMEWORK
            var aggEx = ex as AggregateException;
            if (aggEx != null)
                foreach (var innerException in aggEx.InnerExceptions)
                    ConvertExceptionToFailureInformation(innerException, myIndex, exceptionTypes, messages, stackTraces, indices);
            else
#endif
                if (ex.InnerException != null)
                ConvertExceptionToFailureInformation(ex.InnerException, myIndex, exceptionTypes, messages, stackTraces, indices);
        }

        class FailureInformation : IFailureInformation
        {
            public string[] ExceptionTypes { get; set; }
            public string[] Messages { get; set; }
            public string[] StackTraces { get; set; }
            public int[] ExceptionParentIndices { get; set; }
        }

        static string GetCleanStackTrace(this Exception exception)
        {
#if !(NET452 || NETSTANDARD2_0)
            return exception.StackTrace;
#else
            var stackTrace = new System.Diagnostics.StackTrace(exception, true);
            var stackFrames = stackTrace.GetFrames();

            if (stackFrames == null)
            {
                return exception.StackTrace;
            }

            var isFirst = true;
            var builder = new StringBuilder();
            for (var i = 0; i < stackFrames.Length; i++)
            {
                var frame = stackFrames[i];
                var method = frame.GetMethod();

                // Always show last stackFrame
                if (!ShowInStackTrace(method) && i < stackFrames.Length - 1)
                {
                    continue;
                }

                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    builder.AppendLine();
                }

                builder.Append("   at ");

                AppendStackLine(builder, frame.GetMethod());

                try
                {
                    var fileName = frame.GetFileName();
                    if (fileName != null)
                    {
                        // tack on " in c:\tmp\MyFile.cs:line 5"
                        builder.AppendFormat(CultureInfo.InvariantCulture, " in {0}:line {1}", fileName, frame.GetFileLineNumber());
                    } 
                }
                catch { }
            }

            return builder.ToString();
#endif
        }

#if NET452 || NETSTANDARD2_0
        static bool ShowInStackTrace(MethodBase method)
        {
            // Don't show any methods marked with the StackTraceHiddenAttribute
            // https://github.com/dotnet/coreclr/pull/14652
            foreach (var attibute in method.CustomAttributes)
            {
                // internal Attribute, match on name
                if (attibute.AttributeType.Name == "StackTraceHiddenAttribute")
                {
                    return false;
                }
            }

            var type = method.DeclaringType;
            if (type == null)
            {
                return true;
            }

            foreach (var attibute in type.CustomAttributes)
            {
                // internal Attribute, match on name
                if (attibute.AttributeType.Name == "StackTraceHiddenAttribute")
                {
                    return false;
                }
            }

            // Fallbacks for runtime pre-StackTraceHiddenAttribute
            if (type == typeof(ExceptionDispatchInfo) && method.Name == "Throw")
            {
                return false;
            }

            if (type == typeof(TaskAwaiter) || 
                type == typeof(TaskAwaiter<>) ||
                type == typeof(ConfiguredTaskAwaitable.ConfiguredTaskAwaiter) ||
                type == typeof(ConfiguredTaskAwaitable<>.ConfiguredTaskAwaiter))
            {
                switch (method.Name)
                {
                    case "HandleNonSuccessAndDebuggerNotification":
                    case "ThrowForNonSuccess":
                    case "ValidateEnd":
                    case "GetResult":
                        return false;
                }
            }

            return true;
        }

        internal static void AppendStackLine(StringBuilder builder, MethodBase method)
        {
            // Special case: no method available
            if (method == null)
            {
                return;
            }

            // Type name
            var type = method.DeclaringType;

            var methodName = method.Name;
            var subMethod = String.Empty;
            if (type != null && type.IsDefined(typeof(CompilerGeneratedAttribute)) &&
                (typeof(IAsyncStateMachine).IsAssignableFrom(type) || typeof(IEnumerator).IsAssignableFrom(type)))
            {
                // Convert StateMachine methods to correct overload +MoveNext()
                if (TryResolveStateMachineMethod(ref method, out type))
                {
                    subMethod = methodName;
                }
            }

            // ResolveStateMachineMethod may have set declaringType to null
            if (type != null)
            {
                builder
                    .Append(TypeNameHelper.GetTypeDisplayName(type))
                    .Append(".");
            }

            // Method name
            builder.Append(method.Name);

            // Generic arguments
            var isFirst = true;
            if (method.IsGenericMethod)
            {
                builder.Append("<");

                foreach (var genericArgument in method.GetGenericArguments())
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        builder.Append(", ");
                    }

                    builder.Append(genericArgument);
                }

                builder.Append(">");
            }

            // Method parameters
            isFirst = true;
            builder.Append("(");
            foreach (var parameter in method.GetParameters())
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    builder.Append(", ");
                }

                var parameterType = parameter.ParameterType;
                if (parameter.IsOut)
                {
                    builder.Append("out ");
                }
                else if (parameterType != null && parameterType.IsByRef)
                {
                    builder.Append("ref");
                }

                if (parameterType != null)
                {
                    if (parameterType.IsByRef)
                    {
                        parameterType = parameterType.GetElementType();
                    }

                    builder.Append(TypeNameHelper.GetTypeDisplayName(parameterType, fullName: false));
                }
                else
                {
                    builder.Append("?");
                }

                builder.Append(parameter.Name);
            }
            builder.Append(")");

            // Append statemachine submethod
            if (!string.IsNullOrEmpty(subMethod))
            {
                builder.Append("+");
                builder.Append(subMethod);
                builder.Append("()");
            }
        }

        static bool TryResolveStateMachineMethod(ref MethodBase method, out Type declaringType)
        {
            declaringType = method.DeclaringType;

            var parentType = declaringType.DeclaringType;
            if (parentType == null)
            {
                return false;
            }

            var methods = parentType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (methods == null)
            {
                return false;
            }

            foreach (var candidateMethod in methods)
            {
                var attributes = candidateMethod.GetCustomAttributes<StateMachineAttribute>();
                if (attributes == null)
                {
                    continue;
                }

                foreach (var asma in attributes)
                {
                    if (asma.StateMachineType == declaringType)
                    {
                        method = candidateMethod;
                        declaringType = candidateMethod.DeclaringType;
                        // Mark the iterator as changed; so it gets the + annotation of the original method
                        // async statemachines resolve directly to their builder methods so aren't marked as changed
                        return asma is IteratorStateMachineAttribute;
                    }
                }
            }

            return false;
        }

        // From: https://github.com/aspnet/Common Microsoft.Extensions.StackTrace.Sources
        internal class TypeNameHelper
        {
            private static readonly Dictionary<Type, string> _builtInTypeNames = new Dictionary<Type, string>
            {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(object), "object" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(string), "string" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(ushort), "ushort" }
            };

            public static string GetTypeDisplayName(object item, bool fullName = true)
            {
                return item == null ? null : GetTypeDisplayName(item.GetType(), fullName);
            }

            public static string GetTypeDisplayName(Type type, bool fullName = true)
            {
                var sb = new StringBuilder();
                ProcessTypeName(type, sb, fullName);
                return sb.ToString();
            }

            static void AppendGenericArguments(Type[] args, int startIndex, int numberOfArgsToAppend, StringBuilder sb, bool fullName)
            {
                var totalArgs = args.Length;
                if (totalArgs >= startIndex + numberOfArgsToAppend)
                {
                    sb.Append("<");
                    for (int i = startIndex; i < startIndex + numberOfArgsToAppend; i++)
                    {
                        ProcessTypeName(args[i], sb, fullName);
                        if (i + 1 < startIndex + numberOfArgsToAppend)
                        {
                            sb.Append(", ");
                        }
                    }
                    sb.Append(">");
                }
            }

            static void ProcessTypeName(Type t, StringBuilder sb, bool fullName)
            {
                if (t.IsGenericType)
                {
                    ProcessNestedGenericTypes(t, sb, fullName);
                    return;
                }
                if (_builtInTypeNames.ContainsKey(t))
                {
                    sb.Append(_builtInTypeNames[t]);
                }
                else
                {
                    sb.Append(fullName ? t.FullName : t.Name);
                }
            }

            static void ProcessNestedGenericTypes(Type t, StringBuilder sb, bool fullName)
            {
                var genericFullName = t.GetGenericTypeDefinition().FullName;
                var genericSimpleName = t.GetGenericTypeDefinition().Name;
                var parts = genericFullName.Split('+');
                var genericArguments = t.GenericTypeArguments;
                var index = 0;
                var totalParts = parts.Length;
                if (totalParts == 1)
                {
                    var part = parts[0];
                    var num = part.IndexOf('`');
                    if (num == -1) return;

                    var name = part.Substring(0, num);
                    var numberOfGenericTypeArgs = int.Parse(part.Substring(num + 1));
                    sb.Append(fullName ? name : genericSimpleName.Substring(0, genericSimpleName.IndexOf('`')));
                    AppendGenericArguments(genericArguments, index, numberOfGenericTypeArgs, sb, fullName);
                    return;
                }
                for (var i = 0; i < totalParts; i++)
                {
                    var part = parts[i];
                    var num = part.IndexOf('`');
                    if (num != -1)
                    {
                        var name = part.Substring(0, num);
                        var numberOfGenericTypeArgs = int.Parse(part.Substring(num + 1));
                        if (fullName || i == totalParts - 1)
                        {
                            sb.Append(name);
                            AppendGenericArguments(genericArguments, index, numberOfGenericTypeArgs, sb, fullName);
                        }
                        if (fullName && i != totalParts - 1)
                        {
                            sb.Append("+");
                        }
                        index += numberOfGenericTypeArgs;
                    }
                    else
                    {
                        if (fullName || i == totalParts - 1)
                        {
                            sb.Append(part);
                        }
                        if (fullName && i != totalParts - 1)
                        {
                            sb.Append("+");
                        }
                    }
                }
            }
        }
#endif
    }
}
