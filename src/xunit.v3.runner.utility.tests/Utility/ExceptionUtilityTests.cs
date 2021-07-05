using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class ExceptionUtilityTests
{
	class ErrorMetadata : _IErrorMetadata, IEnumerable
	{
		readonly List<int> exceptionParentIndices = new List<int>();
		readonly List<string?> exceptionTypes = new List<string?>();
		readonly List<string> messages = new List<string>();
		readonly List<string?> stackTraces = new List<string?>();

		public string?[] ExceptionTypes => exceptionTypes.ToArray();

		public string[] Messages => messages.ToArray();

		public string?[] StackTraces => stackTraces.ToArray();

		public int[] ExceptionParentIndices => exceptionParentIndices.ToArray();

		public void Add(
			Exception ex,
			int index = -1)
		{
			Add(ex.GetType(), ex.Message, ex.StackTrace, index);
		}

		public void Add(
			Type exceptionType,
			string message = "",
			string? stackTrace = null,
			int index = -1)
		{
			exceptionTypes.Add(exceptionType.FullName);
			messages.Add(message);
			stackTraces.Add(stackTrace);
			exceptionParentIndices.Add(index);
		}

		public void AddExceptionType(string exceptionTypeName)
		{
			exceptionTypes.Add(exceptionTypeName);
		}

		public void AddIndex(int index)
		{
			exceptionParentIndices.Add(index);
		}

		public void AddMessage(string message)
		{
			messages.Add(message);
		}

		public void AddStackTrace(string stackTrace)
		{
			stackTraces.Add(stackTrace);
		}

		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}

	public class CombineMessages
	{
		[Fact]
		public void XunitException()
		{
			var errorMetadata = new ErrorMetadata { new XunitException("This is the message") };

			var result = ExceptionUtility.CombineMessages(errorMetadata);

			Assert.Equal("This is the message", result);
		}

		[Fact]
		public void NonXunitException()
		{
			var errorMetadata = new ErrorMetadata { new Exception("This is the message") };

			var result = ExceptionUtility.CombineMessages(errorMetadata);

			Assert.Equal("System.Exception : This is the message", result);
		}

		[Fact]
		public void NonXunitExceptionWithInnerExceptions()
		{
			var errorMetadata = new ErrorMetadata {
				{ new Exception("outer exception"), -1 },
				{ new DivideByZeroException("inner exception"), 0 },
				{ new XunitException("inner inner exception"), 1 }
			};

			var result = ExceptionUtility.CombineMessages(errorMetadata);

			Assert.Equal(
				"System.Exception : outer exception" + Environment.NewLine +
				"---- System.DivideByZeroException : inner exception" + Environment.NewLine +
				"-------- inner inner exception",
				result
			);
		}

		[Fact]
		public void AggregateException()
		{
			var errorMetadata = new ErrorMetadata {
				{ new AggregateException(), -1 },
				{ new DivideByZeroException("inner #1"), 0 },
				{ new NotImplementedException("inner #2"), 0 },
				{ new XunitException("this is crazy"), 0 },
			};

			var result = ExceptionUtility.CombineMessages(errorMetadata);

			Assert.Equal(
				"System.AggregateException : One or more errors occurred." + Environment.NewLine +
				"---- System.DivideByZeroException : inner #1" + Environment.NewLine +
				"---- System.NotImplementedException : inner #2" + Environment.NewLine +
				"---- this is crazy",
				result
			);
		}

		[Fact]
		public void MissingExceptionTypes()
		{
			var errorMetadata = new ErrorMetadata();
			errorMetadata.AddMessage("Message 1");
			errorMetadata.AddMessage("Message 2");
			errorMetadata.AddMessage("Message 3");
			errorMetadata.AddIndex(-1);
			errorMetadata.AddIndex(0);
			errorMetadata.AddIndex(0);
			errorMetadata.AddExceptionType("ExceptionType1");
			errorMetadata.AddExceptionType("Xunit.Sdk.ExceptionType2");

			var result = ExceptionUtility.CombineMessages(errorMetadata);

			Assert.Equal(
				"ExceptionType1 : Message 1" + Environment.NewLine +
				"---- Message 2" + Environment.NewLine +
				"----  : Message 3",
				result
			);
		}
	}

	public class CombineStackTraces
	{
		[Fact]
		public void XunitException()
		{
#if DEBUG
			Assert.Skip("Stack trace filtering is disabled in DEBUG builds");
#else
			static void testCode()
			{
				throw new XunitException();
			}
			var ex = Record.Exception(testCode)!;
			var errorMetadata = new ErrorMetadata { ex };

			var result = ExceptionUtility.CombineStackTraces(errorMetadata);

			Assert.DoesNotContain(typeof(Record).FullName!, result);
			Assert.DoesNotContain(typeof(XunitException).FullName!, result);
			Assert.Contains("XunitException", result);
#endif
		}

		[Fact]
		public void NonXunitException()
		{
#if DEBUG
			Assert.Skip("Stack trace filtering is disabled in DEBUG builds");
#else
			static void testCode()
			{
				throw new Exception();
			}
			var ex = Record.Exception(testCode)!;
			var errorMetadata = new ErrorMetadata { ex };

			var result = ExceptionUtility.CombineStackTraces(errorMetadata);

			Assert.DoesNotContain(typeof(Record).FullName!, result);
			Assert.DoesNotContain(typeof(XunitException).FullName!, result);
			Assert.Contains("NonXunitException", result);
#endif
		}

		[Fact]
		public void NonXunitExceptionWithInnerExceptions()
		{
#if DEBUG
			Assert.Skip("Stack trace filtering is disabled in DEBUG builds");
#else
			static void innerTestCode()
			{
				throw new DivideByZeroException();
			}
			var inner = Record.Exception(innerTestCode)!;
			void outerTestCode()
			{
				throw new Exception("message", inner);
			}
			var outer = Record.Exception(outerTestCode)!;
			var errorMetadata = new ErrorMetadata { { outer, -1 }, { inner, 0 } };

			var result = ExceptionUtility.CombineStackTraces(errorMetadata);

			Assert.NotNull(result);
			Assert.Collection(
				result.Split(new[] { Environment.NewLine }, StringSplitOptions.None),
				line => Assert.Contains("NonXunitExceptionWithInnerExceptions", line),
				line => Assert.Equal("----- Inner Stack Trace -----", line),
				line => Assert.Contains("NonXunitExceptionWithInnerExceptions", line)
			);
#endif
		}

		[Fact]
		public void HandlesAggregateException()
		{
#if DEBUG
			Assert.Skip("Stack trace filtering is disabled in DEBUG builds");
#else
			static void inner1TestCode()
			{
				throw new DivideByZeroException();
			}
			var inner1 = Record.Exception(inner1TestCode)!;
			static void inner2TestCode()
			{
				throw new NotImplementedException("inner #2");
			}
			var inner2 = Record.Exception(inner2TestCode)!;
			static void inner3TestCode()
			{
				throw new XunitException("this is crazy");
			}
			var inner3 = Record.Exception(inner3TestCode)!;
			void outerTestCode()
			{
				throw new AggregateException(inner1, inner2, inner3);
			}
			var outer = Record.Exception(outerTestCode)!;
			var errorMetadata = new ErrorMetadata { { outer, -1 }, { inner1, 0 }, { inner2, 0 }, { inner3, 0 } };

			var result = ExceptionUtility.CombineStackTraces(errorMetadata);

			Assert.NotNull(result);
			Assert.Collection(
				result.Split(new[] { Environment.NewLine }, StringSplitOptions.None),
				line => Assert.Contains("HandlesAggregateException", line),
				line => Assert.Equal("----- Inner Stack Trace #1 (System.DivideByZeroException) -----", line),
				line => Assert.Contains("HandlesAggregateException", line),
				line => Assert.Equal("----- Inner Stack Trace #2 (System.NotImplementedException) -----", line),
				line => Assert.Contains("HandlesAggregateException", line),
				line => Assert.Equal("----- Inner Stack Trace #3 (Xunit.Sdk.XunitException) -----", line),
				line => Assert.Contains("HandlesAggregateException", line)
			);
#endif
		}

		[Fact]
		public void MissingStackTracesAndExceptionTypes()
		{
			var errorMetadata = new ErrorMetadata();
			errorMetadata.AddMessage("Message 1");
			errorMetadata.AddMessage("Message 2");
			errorMetadata.AddMessage("Message 3");
			errorMetadata.AddIndex(-1);
			errorMetadata.AddIndex(0);
			errorMetadata.AddIndex(0);
			errorMetadata.AddExceptionType("ExceptionType1");
			errorMetadata.AddExceptionType("Xunit.Sdk.ExceptionType2");
			errorMetadata.AddStackTrace("Stack Trace 1");
			errorMetadata.AddStackTrace("Stack Trace 2");

			var result = ExceptionUtility.CombineStackTraces(errorMetadata);

			Assert.Equal(
				"Stack Trace 1" + Environment.NewLine +
				"----- Inner Stack Trace #1 (Xunit.Sdk.ExceptionType2) -----" + Environment.NewLine +
				"Stack Trace 2" + Environment.NewLine +
				"----- Inner Stack Trace #2 () -----" + Environment.NewLine,
				result
			);
		}
	}
}
