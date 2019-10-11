using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Xunit.ConsoleClient
{
    public class CommandLineOptionsFile
    {


        protected CommandLineOptionsFile(string optionsFile, StreamReader streamReader = null, Predicate<string> fileExists = null)
        {
            if (fileExists == null)
                fileExists = File.Exists;


        }

        public static CommandLineOptionsFile Read(string optionsFile)
            => new CommandLineOptionsFile(optionsFile);

        public Stack<string> Options { get; protected set; }

        public static Stack<string> Read(string optionsFile, StreamReader streamReader = null, Predicate<string> fileExists = null)
        {
            if (fileExists == null)
                fileExists = File.Exists;

            if (optionsFile == null)
                throw new ArgumentException($"missing options file name");

            if (!fileExists(optionsFile))
                throw new ArgumentException($"file not found: {optionsFile}");

            var argumentsFromFile = new Stack<string>();

            if (streamReader == null)
            {
                using (streamReader = new StreamReader(optionsFile))
                {
                    argumentsFromFile = ReadInternal(streamReader);
                }
            }
            else
            {
                argumentsFromFile = ReadInternal(streamReader);
            }

            return argumentsFromFile;
        }

        private static Stack<string> ReadInternal(StreamReader streamReader)
        {
            var readFromFile = new Stack<string>();

            var lookForClosingSingleQuote = false;
            var lookForClosingDoubleQuotes = false;
            var lookingForOptionEnd = false;

            var currentOption = new StringBuilder();
            while (streamReader.Peek() >= 0)
            {
                char nextCharacter = (char)streamReader.Read();

                switch (nextCharacter)
                {
                    case '"':
                        lookForClosingDoubleQuotes = !lookForClosingDoubleQuotes;
                        break;
                    case '\'':
                        lookForClosingSingleQuote = !lookForClosingSingleQuote;
                        break;
                    case ' ':
                    case '\n':
                    case '\r':
                        if (lookForClosingSingleQuote || lookForClosingDoubleQuotes)
                        {
                            lookingForOptionEnd = true;
                            currentOption.Append(nextCharacter);
                        }
                        else
                        {
                            if (lookingForOptionEnd)
                            {
                                if (currentOption.Length > 0)
                                {
                                    readFromFile.Push(currentOption.ToString());
                                    currentOption.Clear();
                                }
                                lookingForOptionEnd = false;
                            }
                        }
                        break;
                    default:
                        lookingForOptionEnd = true;
                        currentOption.Append(nextCharacter);
                        break;
                }
            }

            return readFromFile;
        }
    }
}
