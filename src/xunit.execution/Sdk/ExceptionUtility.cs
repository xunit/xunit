using System;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Utility methods for dealing with exceptions.
    /// </summary>
    internal static class ExceptionUtility
    {
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
            stackTraces.Add(ex.StackTrace);
            indices.Add(parentIndex);

            var aggEx = ex as AggregateException;
            if (aggEx != null)
                foreach (var innerException in aggEx.InnerExceptions)
                    ConvertExceptionToFailureInformation(innerException, myIndex, exceptionTypes, messages, stackTraces, indices);
            else if (ex.InnerException != null)
                ConvertExceptionToFailureInformation(ex.InnerException, myIndex, exceptionTypes, messages, stackTraces, indices);
        }

        class FailureInformation : IFailureInformation
        {
            public string[] ExceptionTypes { get; set; }
            public string[] Messages { get; set; }
            public string[] StackTraces { get; set; }
            public int[] ExceptionParentIndices { get; set; }
        }
    }
}
