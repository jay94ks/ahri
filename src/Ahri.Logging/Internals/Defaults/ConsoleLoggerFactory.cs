using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Logging.Internals.Defaults
{
    internal class ConsoleLoggerFactory : ILoggerFactory
    {
        public ILogger CreateLogger(string Category) => new Logger(Category);
        private class Logger : ILogger
        {
            private static object PADLOCK = new object();
            private static string LABEL = null;

            private string m_Category;

            /// <summary>
            /// Initialize a new <see cref="Logger"/> instance.
            /// </summary>
            /// <param name="Category"></param>
            public Logger(string Category) => m_Category = Category;


            /// <inheritdoc/>
            public void Log(LogLevel Level, string Message, Exception Error = null)
            {
                var Kind = "info";
                var Color = ConsoleColor.White;
                var BgColor = ConsoleColor.Black;

                switch (Level)
                {
                    case LogLevel.Debug:
                        if (!Debugger.IsAttached)
                            return;

                        Color = ConsoleColor.Cyan;
                        Kind = "dbug";
                        break;

                    case LogLevel.Error:
                        Color = ConsoleColor.Black;
                        BgColor = ConsoleColor.Red;
                        Kind = "fail";
                        break;

                    case LogLevel.Info:
                        Color = ConsoleColor.Green;
                        Kind = "info";
                        break;

                    case LogLevel.Warning:
                        Color = ConsoleColor.Yellow;
                        Kind = "warn";
                        break;

                    case LogLevel.Critical:
                        Color = ConsoleColor.White;
                        BgColor = ConsoleColor.Red;
                        Kind = "crit";
                        break;
                }

                var Label = Kind + m_Category;

                lock (PADLOCK)
                {
                    if (LABEL != Label)
                    {
                        LABEL = Label;
                        Console.ForegroundColor = Color;
                        Console.BackgroundColor = BgColor;
                        Console.Write($"{Kind}");

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.WriteLine($": {m_Category}");
                    }

                    if (!string.IsNullOrWhiteSpace(Message))
                        Console.WriteLine($"      {Message}");

                    if (Error != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("      Exception");

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine(": " + Error.GetType().FullName);

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("      => " + Error.Message);

                        Console.ForegroundColor = ConsoleColor.White;
                        if (!string.IsNullOrWhiteSpace(Error.StackTrace))
                            Console.WriteLine(Error.StackTrace);
                    }

                }
            }

            /// <inheritdoc/>
            public void Dispose() { }
        }
    }

}
