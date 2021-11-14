using System;
using MSFSModManager.Core;

namespace MSFSModManager.CLI
{
    class ConsoleLogger : ILogger
    {

        private ConsoleRenderer _renderer;

        public ConsoleLogger(ConsoleRenderer renderer)
        {
            _renderer = renderer;
        }

        public void Log(LogLevel level, string message)
        {
            ConsoleColor color;
            switch (level)
            {
                case LogLevel.Warning:
                    color = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    color = ConsoleColor.DarkRed;
                    break;
                case LogLevel.CriticalError:
                    color = ConsoleColor.Red;
                    break;
                default:
                    color = ConsoleColor.Gray;
                    break;                
            }
            _renderer.WriteLine(message, color);
        }
    }
}