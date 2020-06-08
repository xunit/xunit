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
            Console.WriteLine("optionsFile: " + optionsFile);
            Console.WriteLine("streamReader " + (streamReader == null ? "is" : "is not") + " null");
            Console.WriteLine("fileExists " + (fileExists == null ? "is" : "is not") + " null");

            if (fileExists == null)
                fileExists = File.Exists;

            optionsFileName = optionsFile;

            if (streamReader == null)
            {
                Console.WriteLine("streamReader = new StreamReader(optionsFile)");
                using (streamReader = new StreamReader(optionsFile))
                {
                    Console.WriteLine("calling Read(streamReader, fileExists)");
                    Options = Read(streamReader, fileExists);
                }
            }
            else
            {
                Console.WriteLine("calling Read(streamReader, fileExists)");
                Options = Read(streamReader, fileExists);
            }
            
        }

        public static CommandLineOptionsFile Read(string optionsFile)
            => new CommandLineOptionsFile(optionsFile);

        
        public Stack<string> Options { get; protected set; }


        protected static Stack<string> Read(StreamReader streamReader, Predicate<string> fileExists)
        {
            Console.WriteLine("in protected static Stack<string> Read(StreamReader streamReader, Predicate<string> fileExists)");
            if (optionsFileName == null)
                throw new ArgumentException($"missing options file name");

            Console.WriteLine("in protected static Stack<string> Read(StreamReader streamReader, Predicate<string> fileExists)");
            if (!fileExists(optionsFileName))
                throw new ArgumentException($"file not found: {optionsFileName}");

            var readFromFile = new Stack<string>();

            var lookForClosingSingleQuote = false;
            var lookForClosingDoubleQuotes = false;
            var lookingForOptionEnd = false;

            var currentOption = new StringBuilder();

            Console.WriteLine("before while (streamReader.Peek() >= 0)");
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
