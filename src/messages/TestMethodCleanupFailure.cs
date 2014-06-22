//using System;
//using Xunit.Abstractions;

//#if XUNIT_CORE_DLL
//namespace Xunit.Sdk
//#else
//namespace Xunit
//#endif
//{
//    /// <summary>
//    /// Default implementation of <see cref="IErrorMessage"/>.
//    /// </summary>
//    public class ErrorMessage : LongLivedMarshalByRefObject, IErrorMessage
//    {
//        /// <summary>
//        /// Initializes a new instance of the <see cref="ErrorMessage"/> class.
//        /// </summary>
//        public ErrorMessage(string[] exceptionTypes, string[] messages, string[] stackTraces, int[] exceptionParentIndices)
//        {
//            StackTraces = stackTraces;
//            Messages = messages;
//            ExceptionTypes = exceptionTypes;
//            ExceptionParentIndices = exceptionParentIndices;
//        }

//#if XUNIT_CORE_DLL
//        /// <summary>
//        /// Initializes a new instance of the <see cref="ErrorMessage"/> class.
//        /// </summary>
//        /// <param name="ex">The exception that represents the error message.</param>
//        public ErrorMessage(Exception ex)
//        {
//            var failureInfo = ExceptionUtility.ConvertExceptionToFailureInformation(ex);
//            ExceptionTypes = failureInfo.ExceptionTypes;
//            Messages = failureInfo.Messages;
//            StackTraces = failureInfo.StackTraces;
//            ExceptionParentIndices = failureInfo.ExceptionParentIndices;
//        }
//#endif

//        /// <inheritdoc/>
//        public string[] ExceptionTypes { get; private set; }

//        /// <inheritdoc/>
//        public string[] Messages { get; private set; }

//        /// <inheritdoc/>
//        public string[] StackTraces { get; private set; }

//        /// <inheritdoc/>
//        public int[] ExceptionParentIndices { get; private set; }
//    }
//}