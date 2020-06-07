// This code was adapted from https://github.com/Microsoft/msbuild/blob/ab090d1255caa87e742cbdbc6d7fe904ecebd975/src/Build/Logging/BaseConsoleLogger.cs#L361-L401
// Under the MIT license https://github.com/Microsoft/msbuild/blob/ab090d1255caa87e742cbdbc6d7fe904ecebd975/LICENSE

using System;

namespace Xunit
{
    internal static class ConsoleHelper
    {
        internal static Action ResetColor;
        internal static Action<ConsoleColor> SetForegroundColor;

        static ConsoleHelper()
        {
#if NETFRAMEWORK
            ResetColor = ResetColorConsole;
            SetForegroundColor = SetForegroundColorConsole;
#else
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                ResetColor = ResetColorConsole;
                SetForegroundColor = SetForegroundColorConsole;
            }
            else
            {
                ResetColor = ResetColorANSI;
                SetForegroundColor = SetForegroundColorANSI;
            }
#endif
        }

        static void SetForegroundColorANSI(ConsoleColor c)
        {
            string colorString = "\x1b[";
            switch (c)
            {
                case ConsoleColor.Black: colorString += "30"; break;
                case ConsoleColor.DarkBlue: colorString += "34"; break;
                case ConsoleColor.DarkGreen: colorString += "32"; break;
                case ConsoleColor.DarkCyan: colorString += "36"; break;
                case ConsoleColor.DarkRed: colorString += "31"; break;
                case ConsoleColor.DarkMagenta: colorString += "35"; break;
                case ConsoleColor.DarkYellow: colorString += "33"; break;
                case ConsoleColor.Gray: colorString += "37"; break;
                case ConsoleColor.DarkGray: colorString += "90"; break;
                case ConsoleColor.Blue: colorString += "94"; break;
                case ConsoleColor.Green: colorString += "92"; break;
                case ConsoleColor.Cyan: colorString += "96"; break;
                case ConsoleColor.Red: colorString += "91"; break;
                case ConsoleColor.Magenta: colorString += "95"; break;
                case ConsoleColor.Yellow: colorString += "93"; break;
                case ConsoleColor.White: colorString += "97"; break;
                default: colorString = ""; break;
            }
            if ("" != colorString)
            {
                colorString += "m";
                Console.Out.Write(colorString);
            }
        }

        static void SetForegroundColorConsole(ConsoleColor c)
            => Console.ForegroundColor = c;

        static void ResetColorANSI()
            => Console.Out.Write("\x1b[m");

        static void ResetColorConsole()
            => Console.ResetColor();
    }
}
