using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Xunit.ConsoleClient
{
    public class CommandLineOptionsFile
    {
        static string optionsFileName;

        protected CommandLineOptionsFile(string optionsFile, StreamReader streamReader = null, Predicate<string> fileExists = null)
        {
            if (fileExists == null)
                fileExists = File.Exists;

            optionsFileName = optionsFile;

            if (streamReader == null)
            {
                using (streamReader = new StreamReader(optionsFile))
                {
                    Options = Read(streamReader, fileExists);
                }
            }
            else
            {
                Options = Read(streamReader, fileExists);
            }
            
        }

        public static CommandLineOptionsFile Read(string optionsFile)
            => new CommandLineOptionsFile(optionsFile);

        
        public Stack<string> Options { get; protected set; }


        protected static Stack<string> Read(StreamReader streamReader, Predicate<string> fileExists)
        {
            if (optionsFileName == null)
                throw new ArgumentException($"missing options file name");

            if (!fileExists(optionsFileName))
                throw new ArgumentException($"file not found: {optionsFileName}");

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
                    case (char) 10:
                    case (char) 13:
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

            readFromFile.Push(currentOption.ToString());

            return readFromFile;
        }
    }
}
