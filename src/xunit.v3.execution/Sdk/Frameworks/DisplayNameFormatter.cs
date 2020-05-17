using System;
using System.Collections.Generic;
using System.Text;
using static Xunit.Sdk.TestMethodDisplay;
using static Xunit.Sdk.TestMethodDisplayOptions;

namespace Xunit.Sdk
{
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
        public DisplayNameFormatter()
        {
            rule = new CharacterRule();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayNameFormatter"/> class.
        /// </summary>
        /// <param name="display">The <see cref="TestMethodDisplay"/> used by the formatter.</param>
        /// <param name="displayOptions">The <see cref="TestMethodDisplayOptions"/> used by the formatter.</param>
        public DisplayNameFormatter(TestMethodDisplay display, TestMethodDisplayOptions displayOptions)
        {
            rule = new CharacterRule();

            if ((displayOptions & UseEscapeSequences) == UseEscapeSequences)
                rule = new ReplaceEscapeSequenceRule() { Next = rule };

            if ((displayOptions & ReplaceUnderscoreWithSpace) == ReplaceUnderscoreWithSpace)
                rule = new ReplaceUnderscoreRule() { Next = rule };

            if ((displayOptions & UseOperatorMonikers) == UseOperatorMonikers)
                rule = new ReplaceOperatorMonikerRule() { Next = rule };

            if (display == ClassAndMethod)
            {
                if ((displayOptions & ReplacePeriodWithComma) == ReplacePeriodWithComma)
                    rule = new ReplacePeriodRule() { Next = rule };
                else
                    rule = new KeepPeriodRule() { Next = rule };
            }
        }

        /// <summary>
        /// Formats the specified display name.
        /// </summary>
        /// <param name="displayName">The display name to format.</param>
        /// <returns>The formatted display name.</returns>
        public string Format(string displayName)
        {
            var context = new FormatContext(displayName);

            while (context.HasMoreText)
                rule.Evaluate(context, context.ReadNext());

            context.Flush();

            return context.FormattedDisplayName.ToString();
        }

        sealed class FormatContext
        {
            readonly string text;
            readonly int length;
            int position;

            public FormatContext(string text)
            {
                this.text = text;
                length = text.Length;
            }

            public StringBuilder FormattedDisplayName { get; } = new StringBuilder();

            public StringBuilder Buffer { get; } = new StringBuilder();

            public bool HasMoreText => position < length;

            public char ReadNext() => text[position++];

            public void Flush()
            {
                FormattedDisplayName.Append(Buffer);
                Buffer.Clear();
            }
        }

        class CharacterRule
        {
            public virtual void Evaluate(FormatContext context, char character) => context.Buffer.Append(character);

            public CharacterRule Next { get; set; }
        }

        sealed class ReplaceOperatorMonikerRule : CharacterRule
        {
            static readonly Dictionary<string, string> tokenMonikers =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["eq"] = "=",
                    ["ne"] = "!=",
                    ["lt"] = "<",
                    ["le"] = "<=",
                    ["gt"] = ">",
                    ["ge"] = ">="
                };

            public override void Evaluate(FormatContext context, char character)
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

            bool TryConsumeMoniker(FormatContext context, string token)
            {
                if (!tokenMonikers.TryGetValue(token, out string @operator))
                    return false;

                context.Buffer.Clear();
                context.Buffer.Append(@operator);
                return true;
            }
        }

        sealed class ReplaceUnderscoreRule : CharacterRule
        {
            public override void Evaluate(FormatContext context, char character)
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
            public override void Evaluate(FormatContext context, char character)
            {
                switch (character)
                {
                    case 'U':
                        // same as \uHHHH without the leading '\u'
                        TryConsumeEscapeSequence(context, character, 4);
                        break;

                    case 'X':
                        // same as \xHH without the leading '\x'
                        TryConsumeEscapeSequence(context, character, 2);
                        break;

                    default:
                        Next?.Evaluate(context, character);
                        break;
                }
            }

            static void TryConsumeEscapeSequence(FormatContext context, char @char, int allowedLength)
            {
                var escapeSequence = new char[allowedLength];
                var consumed = 0;

                while (consumed < allowedLength && context.HasMoreText)
                {
                    var nextChar = context.ReadNext();

                    escapeSequence[consumed++] = nextChar;

                    if (IsHex(nextChar))
                        continue;

                    context.Buffer.Append(@char);
                    context.Buffer.Append(escapeSequence, 0, consumed);
                    return;
                }

                context.Buffer.Append(char.ConvertFromUtf32(HexToInt32(escapeSequence)));
            }

            static bool IsHex(char c) => (c > 64 && c < 71) || (c > 47 && c < 58);

            static int HexToInt32(char[] hex)
            {
                var @int = 0;
                var length = hex.Length - 1;

                for (var i = 0; i <= length; i++)
                {
                    var c = hex[i];
                    var v = c < 58 ? c - 48 : c - 55;
                    @int += v << ((length - i) << 2);
                }

                return @int;
            }
        }

        sealed class ReplacePeriodRule : CharacterRule
        {
            public override void Evaluate(FormatContext context, char character)
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
            public override void Evaluate(FormatContext context, char character)
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
}
