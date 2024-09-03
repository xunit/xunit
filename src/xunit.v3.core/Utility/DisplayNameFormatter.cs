using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents a formatter that formats the display name of a class and/or method into a more
/// human readable form using additional options.
/// </summary>
public class DisplayNameFormatter
{
	readonly CharacterRule rule;

	/// <summary>
	/// Initializes a new instance of the <see cref="DisplayNameFormatter"/> class.
	/// </summary>
	public DisplayNameFormatter() =>
		rule = new CharacterRule();

	/// <summary>
	/// Initializes a new instance of the <see cref="DisplayNameFormatter"/> class.
	/// </summary>
	/// <param name="display">The <see cref="TestMethodDisplay"/> used by the formatter.</param>
	/// <param name="displayOptions">The <see cref="TestMethodDisplayOptions"/> used by the formatter.</param>
	public DisplayNameFormatter(
		TestMethodDisplay display,
		TestMethodDisplayOptions displayOptions)
	{
		rule = new CharacterRule();

		if ((displayOptions & TestMethodDisplayOptions.UseEscapeSequences) == TestMethodDisplayOptions.UseEscapeSequences)
			rule = new ReplaceEscapeSequenceRule() { Next = rule };

		if ((displayOptions & TestMethodDisplayOptions.ReplaceUnderscoreWithSpace) == TestMethodDisplayOptions.ReplaceUnderscoreWithSpace)
			rule = new ReplaceUnderscoreRule() { Next = rule };

		if ((displayOptions & TestMethodDisplayOptions.UseOperatorMonikers) == TestMethodDisplayOptions.UseOperatorMonikers)
			rule = new ReplaceOperatorMonikerRule() { Next = rule };

		if (display == TestMethodDisplay.ClassAndMethod)
			rule =
				(displayOptions & TestMethodDisplayOptions.ReplacePeriodWithComma) == TestMethodDisplayOptions.ReplacePeriodWithComma
					? new ReplacePeriodRule() { Next = rule }
					: new KeepPeriodRule() { Next = rule };
	}

	/// <summary>
	/// Formats the specified display name.
	/// </summary>
	/// <param name="displayName">The display name to format.</param>
	/// <returns>The formatted display name.</returns>
	public string Format(string displayName)
	{
		Guard.ArgumentNotNull(displayName);

		var context = new FormatContext(displayName);

		while (context.HasMoreText)
			rule.Evaluate(context, context.ReadNext());

		context.Flush();

		return context.FormattedDisplayName.ToString();
	}

	sealed class FormatContext(string text)
	{
		int position;

		public StringBuilder FormattedDisplayName { get; } = new();

		public StringBuilder Buffer { get; } = new();

		public bool HasMoreText => position < text.Length;

		public char ReadNext() => text[position++];

		public char? TryLookAhead(int offset)
		{
			var lookAheadPosition = position + offset;

			return lookAheadPosition < text.Length
				? text[lookAheadPosition]
				: null;
		}

		public void Flush()
		{
			FormattedDisplayName.Append(Buffer);
			Buffer.Clear();
		}
	}

	class CharacterRule
	{
		public virtual void Evaluate(
			FormatContext context,
			char character) =>
				context.Buffer.Append(character);

		public CharacterRule? Next { get; set; }
	}

	sealed class ReplaceOperatorMonikerRule : CharacterRule
	{
		static readonly Dictionary<string, string> tokenMonikers =
			new(StringComparer.OrdinalIgnoreCase)
			{
				["eq"] = "=",
				["ne"] = "!=",
				["lt"] = "<",
				["le"] = "<=",
				["gt"] = ">",
				["ge"] = ">="
			};

		public override void Evaluate(
			FormatContext context,
			char character)
		{
			if (character == '_')
			{
				if (TryConsumeMoniker(context, context.Buffer.ToString()))
				{
					context.Buffer.Append(' ');
					context.Flush();
				}
				else
					Next?.Evaluate(context, character);

				return;
			}

			if (context.HasMoreText)
				Next?.Evaluate(context, character);
			else if (TryConsumeMoniker(context, context.Buffer.ToString() + character))
				context.Flush();
			else
				Next?.Evaluate(context, character);
		}

		static bool TryConsumeMoniker(
			FormatContext context,
			string token)
		{
			if (!tokenMonikers.TryGetValue(token, out var @operator))
				return false;

			context.Buffer.Clear();
			context.Buffer.Append(@operator);
			return true;
		}
	}

	sealed class ReplaceUnderscoreRule : CharacterRule
	{
		public override void Evaluate(
			FormatContext context,
			char character)
		{
			if (character == '_')
			{
				context.Buffer.Append(' ');
				context.Flush();
			}
			else
				Next?.Evaluate(context, character);
		}
	}

	sealed class ReplaceEscapeSequenceRule : CharacterRule
	{
		public override void Evaluate(
			FormatContext context,
			char character)
		{
			var consumed = character switch
			{
				// same as \uHHHH without the leading '\u'
				'U' => TryConsumeEscapeSequence(context, 4),
				// same as \xHH without the leading '\x'
				'X' => TryConsumeEscapeSequence(context, 2),
				_ => 0,
			};

			if (consumed == 0)
				Next?.Evaluate(context, character);
			else
				for (var i = 0; i < consumed; i++)
					context.ReadNext();
		}

		static int TryConsumeEscapeSequence(
			FormatContext context,
			int length)
		{
			var escapeSequence = new char[length];
			int consumed;

			for (consumed = 0; consumed < length; consumed++)
			{
				var nextChar = context.TryLookAhead(consumed);

				if (nextChar.HasValue && IsHex(nextChar.Value))
					escapeSequence[consumed] = nextChar.Value;
				else
					return 0;
			}

			context.Buffer.Append(char.ConvertFromUtf32(HexToInt32(escapeSequence)));
			return consumed;
		}

		static bool IsHex(char c) =>
			c is (>= '0' and <= '9') or (>= 'A' and <= 'F');

		static int HexToInt32(char[] hex)
		{
			var @int = 0;
			var length = hex.Length - 1;

			for (var i = 0; i <= length; i++)
			{
				var c = hex[i];
				var v = c <= '9' ? c - '0' : c - 'A' + 10;
				@int += v << ((length - i) << 2);
			}

			return @int;
		}
	}

	sealed class ReplacePeriodRule : CharacterRule
	{
		public override void Evaluate(
			FormatContext context,
			char character)
		{
			if (character == '.')
			{
				context.Buffer.Append(", ");
				context.Flush();
			}
			else
				Next?.Evaluate(context, character);
		}
	}

	sealed class KeepPeriodRule : CharacterRule
	{
		public override void Evaluate(
			FormatContext context,
			char character)
		{
			if (character == '.')
			{
				context.Buffer.Append(character);
				context.Flush();
			}
			else
				Next?.Evaluate(context, character);
		}
	}
}
