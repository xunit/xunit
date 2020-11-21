using System;
using System.Text;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.v2;

namespace Xunit.Sdk
{
	/// <summary>
	/// Default implementation of <see cref="ITestOutputHelper"/>.
	/// </summary>
	public class TestOutputHelper : ITestOutputHelper
	{
		readonly object lockObject = new object();
		TestState? state;

		/// <summary>
		/// Gets the output provided by the test.
		/// </summary>
		public string Output
		{
			get
			{
				lock (lockObject)
				{
					Guard.NotNull("There is no currently active test.", state);

					return state.Buffer.ToString();
				}
			}
		}

		/// <summary>
		/// Initialize the test output helper with information about a test.
		/// </summary>
		public void Initialize(
			IMessageBus messageBus,
			ITest test)
		{
			if (state != null)
				throw new InvalidOperationException("Attempted to initialize a TestOutputHelper that has already been initialized");

			state = new TestState(messageBus, test);
		}

		void QueueTestOutput(string output)
		{
			output = EscapeInvalidHexChars(output);

			lock (lockObject)
			{
				Guard.NotNull("There is no currently active test.", state);

				state.Buffer.Append(output);
			}

			state.MessageBus.QueueMessage(new TestOutput(state.Test, output));
		}

		private static string EscapeInvalidHexChars(string s)
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
			public TestState(IMessageBus messageBus, ITest test)
			{
				Buffer = new StringBuilder();
				MessageBus = Guard.ArgumentNotNull(nameof(messageBus), messageBus);
				Test = Guard.ArgumentNotNull(nameof(test), test);
			}

			public StringBuilder Buffer { get; }

			public IMessageBus MessageBus { get; }

			public ITest Test { get; }
		}
	}
}
