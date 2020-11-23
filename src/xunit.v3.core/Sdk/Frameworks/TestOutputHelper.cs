using System;
using System.Text;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// Default implementation of <see cref="ITestOutputHelper"/>.
	/// </summary>
	public class TestOutputHelper : ITestOutputHelper
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
			string testAssemblyUniqueID,
			string testCollectionUniqueID,
			string? testClassUniqueID,
			string? testMethodUniqueID,
			string testCaseUniqueID,
			string testUniqueID)
		{
			if (state != null)
				throw new InvalidOperationException("Attempted to initialize a TestOutputHelper that has already been initialized");

			state = new TestState(
				messageBus,
				testAssemblyUniqueID,
				testCollectionUniqueID,
				testClassUniqueID,
				testMethodUniqueID,
				testCaseUniqueID,
				testUniqueID
			);
		}

		void QueueTestOutput(string output)
		{
			Guard.NotNull("There is no currently active test.", state);

			state.OnOutput(output);
		}

		/// <summary>
		/// Resets the test output helper to its uninitialized state.
		/// </summary>
		public void Uninitialize() => state = null;

		/// <inheritdoc/>
		public void WriteLine(string message)
		{
			Guard.ArgumentNotNull("message", message);

			QueueTestOutput(message + Environment.NewLine);
		}

		/// <inheritdoc/>
		public void WriteLine(string format, params object[] args)
		{
			Guard.ArgumentNotNull("format", format);
			Guard.ArgumentNotNull("args", args);

			QueueTestOutput(string.Format(format, args) + Environment.NewLine);
		}

		class TestState
		{
			readonly StringBuilder buffer = new StringBuilder();
			readonly object lockObject = new object();
			readonly IMessageBus messageBus;
			readonly string testAssemblyUniqueID;
			readonly string testCollectionUniqueID;
			readonly string? testClassUniqueID;
			readonly string? testMethodUniqueID;
			readonly string testCaseUniqueID;
			readonly string testUniqueID;

			public TestState(
				IMessageBus messageBus,
				string testAssemblyUniqueID,
				string testCollectionUniqueID,
				string? testClassUniqueID,
				string? testMethodUniqueID,
				string testCaseUniqueID,
				string testUniqueID)
			{
				this.messageBus = Guard.ArgumentNotNull(nameof(messageBus), messageBus);
				this.testAssemblyUniqueID = Guard.ArgumentNotNull(nameof(testAssemblyUniqueID), testAssemblyUniqueID);
				this.testCollectionUniqueID = Guard.ArgumentNotNull(nameof(testCollectionUniqueID), testCollectionUniqueID);
				this.testClassUniqueID = testClassUniqueID;
				this.testMethodUniqueID = testMethodUniqueID;
				this.testCaseUniqueID = Guard.ArgumentNotNull(nameof(testCaseUniqueID), testCaseUniqueID);
				this.testUniqueID = Guard.ArgumentNotNull(nameof(testUniqueID), testUniqueID);
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
					else if (ch < 32 && !char.IsWhiteSpace(ch)) // C0 control char
						builder.AppendFormat(@"\x{0}", (+ch).ToString("x2"));
					else if (char.IsSurrogatePair(s, i))
					{
						// For valid surrogates, append like normal
						builder.Append(ch);
						builder.Append(s[++i]);
					}
					// Check for stray surrogates/other invalid chars
					else if (char.IsSurrogate(ch) || ch == '\uFFFE' || ch == '\uFFFF')
					{
						builder.AppendFormat(@"\x{0}", (+ch).ToString("x4"));
					}
					else
						builder.Append(ch); // Append the char like normal
				}

				return builder.ToString();
			}
		}
	}
}
