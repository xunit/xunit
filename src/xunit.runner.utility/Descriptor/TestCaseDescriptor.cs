using System.Collections.Generic;

namespace Xunit
{
    /// <summary>
    /// INTERNAL CLASS. DO NOT USE.
    /// </summary>
    public class TestCaseDescriptor
    {
        // Same as TestCaseDescriptorFactory
        const string Separator = "\n";
        const char SeparatorChar = '\n';
        const string SeparatorEscape = "\\n";

        /// <summary/>
        public TestCaseDescriptor() { }

        /// <summary/>
        public TestCaseDescriptor(string descriptorText)
        {
            Traits = new Dictionary<string, List<string>>();

            var lines = descriptorText.Split(SeparatorChar);

            for (var idx = 0; idx < lines.Length; ++idx)
            {
                var line = lines[idx];
                if (line.Length == 0)
                    continue;

                var text = line.Substring(2);

                switch (line[0])
                {
                    case 'C':
                        ClassName = text;
                        break;

                    case 'M':
                        MethodName = text;
                        break;

                    case 'U':
                        UniqueID = text;
                        break;

                    case 'D':
                        DisplayName = Decode(text);
                        break;

                    case 'S':
                        Serialization = text;
                        break;

                    case 'R':
                        SkipReason = Decode(text);
                        break;

                    case 'F':
                        SourceFileName = text;
                        break;

                    case 'L':
                        if (text != null)
                            SourceLineNumber = int.Parse(text);
                        break;

                    case 'T':
                        Traits.AddOrGet(text).Add(Decode(lines[++idx]));
                        break;
                }
            }
        }

        /// <summary/>
        public string ClassName { get; set; }

        /// <summary/>
        public string DisplayName { get; set; }

        /// <summary/>
        public string MethodName { get; set; }

        /// <summary/>
        public string Serialization { get; set; }

        /// <summary/>
        public string SkipReason { get; set; }

        /// <summary/>
        public string SourceFileName { get; set; }

        /// <summary/>
        public int? SourceLineNumber { get; set; }

        /// <summary/>
        public Dictionary<string, List<string>> Traits;

        /// <summary/>
        public string UniqueID { get; set; }

        static string Decode(string value)
            => string.IsNullOrEmpty(value) ? null : value.Replace(SeparatorEscape, Separator);
    }
}
