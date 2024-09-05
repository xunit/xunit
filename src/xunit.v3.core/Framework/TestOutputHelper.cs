using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Default implementation of <see cref="ITestOutputHelper"/>.
/// </summary>
public class TestOutputHelper : ITestOutputHelper
{
	TestState? state;

	/// <inheritdoc/>
	public string Output
	{
		get
		{
			Guard.NotNull("There is no currently active test.", state);

			state.SendPendingOutput();
			return state.BufferedOutput;
		}
	}

	/// <summary>
	/// Initialize the test output helper with information about a test.
	/// </summary>
	public void Initialize(
		IMessageBus messageBus,
		ITest test)
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
	public void Uninitialize()
	{
		state?.SendPendingOutput();
		state = null;
	}

	/// <inheritdoc/>
	public void Write(string message)
	{
		Guard.ArgumentNotNull(message);

		QueueTestOutput(message);
	}

	/// <inheritdoc/>
	public void Write(
		string format,
		params object[] args)
	{
		Guard.ArgumentNotNull(format);
		Guard.ArgumentNotNull(args);

		QueueTestOutput(string.Format(CultureInfo.CurrentCulture, format, args));
	}

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

	sealed class TestState
	{
		static readonly Regex ansiSgrRegex = new("^\\e\\[\\d*(;\\d*)*m");
		readonly StringBuilder buffer = new();
		readonly object lockObject = new();
		readonly IMessageBus messageBus;
		static readonly int numberOfMinimalRemainingCharsForValidAnsiSgr = 3 + Environment.NewLine.Length;  // ESC + [ + m
		readonly StringBuilder residual = new();
		readonly string testAssemblyUniqueID;
		readonly string testCollectionUniqueID;
		readonly string? testClassUniqueID;
		readonly string? testMethodUniqueID;
		readonly string testCaseUniqueID;
		readonly string testUniqueID;

		public TestState(
			IMessageBus messageBus,
			ITest test)
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

			string[]? lines = default;

			lock (lockObject)
			{
				buffer.Append(output);

				residual.Append(output);
				var residualLines = residual.ToString().Split([Environment.NewLine], StringSplitOptions.None);
				if (residualLines.Length > 1)
				{
					residual.Clear();
					residual.Append(residualLines.Last());
					lines = residualLines.Take(residualLines.Length - 1).ToArray();
				}
			}

			if (lines is not null)
				foreach (var line in lines)
					SendOutput(line);
		}

		void SendOutput(string line) =>
			messageBus.QueueMessage(new TestOutput
			{
				AssemblyUniqueID = testAssemblyUniqueID,
				Output = line + Environment.NewLine,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID
			});

		public void SendPendingOutput()
		{
			lock (lockObject)
			{
				if (residual.Length != 0)
					SendOutput(residual.ToString());

				residual.Clear();
			}
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
