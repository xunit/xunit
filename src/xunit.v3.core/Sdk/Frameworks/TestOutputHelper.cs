using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk;

/// <summary>
/// Default implementation of <see cref="_ITestOutputHelper"/>.
/// </summary>
public class TestOutputHelper : _ITestOutputHelper
{
	TestState? state;

	/// <summary>
	/// Gets the output provided by the test.
	/// </summary>
	public string Output
	{
		get
		{
			Guard.NotNull("There is no currently active test.", state);

			return state.BufferedOutput;
		}
	}

	/// <summary>
	/// Initialize the test output helper with information about a test.
	/// </summary>
	public void Initialize(
		IMessageBus messageBus,
		_ITest test)
	{
		if (state is not null)
			throw new InvalidOperationException("Attempted to initialize a TestOutputHelper that has already been initialized");

		state = new TestState(messageBus, test);
	}

	void QueueTestOutput(string output)
	{
		Guard.NotNull("There is no currently active test.", state);

		state.OnOutput(output);
	}

	/// <summary>
	/// Resets the test output helper to its uninitialized state.
	/// </summary>
	public void Uninitialize() =>
		state = null;

	/// <inheritdoc/>
	public void WriteLine(string message)
	{
		Guard.ArgumentNotNull(message);

		QueueTestOutput(message + Environment.NewLine);
	}

	/// <inheritdoc/>
	public void WriteLine(
		string format,
		params object[] args)
	{
		Guard.ArgumentNotNull(format);
		Guard.ArgumentNotNull(args);

		QueueTestOutput(string.Format(CultureInfo.CurrentCulture, format, args) + Environment.NewLine);
	}

	class TestState
	{
		readonly static Regex ansiSgrRegex = new("^\\e\\[\\d*(;\\d*)*m");
		readonly StringBuilder buffer = new();
		readonly object lockObject = new();
		readonly IMessageBus messageBus;
		readonly static int numberOfMinimalRemainingCharsForValidAnsiSgr = 3 + Environment.NewLine.Length;  // ESC + [ + m
		readonly string testAssemblyUniqueID;
		readonly string testCollectionUniqueID;
		readonly string? testClassUniqueID;
		readonly string? testMethodUniqueID;
		readonly string testCaseUniqueID;
		readonly string testUniqueID;

		public TestState(
			IMessageBus messageBus,
			_ITest test)
		{
			Guard.ArgumentNotNull(test);

			this.messageBus = Guard.ArgumentNotNull(messageBus);

			testAssemblyUniqueID = test.TestCase.TestCollection.TestAssembly.UniqueID;
			testCollectionUniqueID = test.TestCase.TestCollection.UniqueID;
			testClassUniqueID = test.TestCase.TestClass?.UniqueID;
			testMethodUniqueID = test.TestCase.TestMethod?.UniqueID;
			testCaseUniqueID = test.TestCase.UniqueID;
			testUniqueID = test.UniqueID;
		}

		public string BufferedOutput
		{
			get
			{
				lock (lockObject)
					return buffer.ToString();
			}
		}

		public void OnOutput(string output)
		{
			output = EscapeInvalidHexChars(output);

			lock (lockObject)
				buffer.Append(output);

			var message = new _TestOutput
			{
				AssemblyUniqueID = testAssemblyUniqueID,
				Output = output,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID
			};

			messageBus.QueueMessage(message);
		}

		static string EscapeInvalidHexChars(string s)
		{
			var builder = new StringBuilder(s.Length);

			for (var i = 0; i < s.Length; i++)
			{
				var ch = s[i];
				if (ch == '\0')
					builder.Append("\\0");
				else if (ch == 0x1b)
					HandleEscapeCharacter(builder, s, i);
				else if (ch < 32 && !char.IsWhiteSpace(ch)) // C0 control char
					builder.AppendFormat(CultureInfo.CurrentCulture, @"\x{0}", (+ch).ToString("x2", CultureInfo.CurrentCulture));
				else if (char.IsSurrogatePair(s, i))
				{
					// For valid surrogates, append like normal
					builder.Append(ch);
					builder.Append(s[++i]);
				}
				// Check for stray surrogates/other invalid chars
				else if (char.IsSurrogate(ch) || ch == '\uFFFE' || ch == '\uFFFF')
					builder.AppendFormat(CultureInfo.CurrentCulture, @"\x{0}", (+ch).ToString("x4", CultureInfo.CurrentCulture));
				else
					builder.Append(ch); // Append the char like normal
			}

			return builder.ToString();
		}

		static void HandleEscapeCharacter(
			StringBuilder builder,
			string message,
			int i)
		{
			if (numberOfMinimalRemainingCharsForValidAnsiSgr <= message.Length - i && message[i + 1] == '[')
				HandlePossibleAnsiSgr(builder, message, i);
			else
				builder.Append("\\x1b"); // This can't be the beginning of an ANSI-SGR
		}

		static void HandlePossibleAnsiSgr(
			StringBuilder builder,
			string message,
			int i)
		{
			var remaining = message.Substring(i);
			if (ansiSgrRegex.IsMatch(remaining))
				builder.Append(message[i]);
			else
				builder.Append("\\x1b");
		}
	}
}
