using System;
using System.Collections.Generic;
using System.Linq;

namespace MSFSModManager.CLI
{

    class ConsoleRenderer
    {
        public class LineHandle
        {

            public ConsoleRenderer Renderer { get; }

            public int Line { get; internal set; }

            private int _lineLength;

            public LineHandle(ConsoleRenderer renderer, int line)
            {
                Renderer = renderer;
                Line = line;
                _lineLength = 0;
            }

            public void Write(string text, ConsoleColor color)
            {
                Renderer.Write(text, this, color);
                _lineLength = text.Length;
            }

            public void Write(string text)
            {
                Renderer.Write(text, this);
                _lineLength = text.Length;
            }

            public void Clear()
            {
                Renderer.Write(new string(' ', _lineLength), this);
            }
        }
        
        private HashSet<LineHandle> _lineHandles;
        private object _lockObject;

        public ConsoleRenderer()
        {
            _lineHandles = new HashSet<LineHandle>();
            _lockObject = new object();
        }

        private void UpdateHandles(int offset)
        {
            foreach (var line in _lineHandles)
            {
                line.Line += offset;
            }
        }

        public void WriteLine(string text)
        {
            Console.ResetColor();
            ConsoleColor color = Console.ForegroundColor;
            WriteLine(text, color);
        }

        public void WriteLine(string text, ConsoleColor color)
        {
            lock (_lockObject)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(text);
                Console.ResetColor();

                int lineBreaks = text.Count(c => c == '\n') + 1;

                UpdateHandles(lineBreaks);
            }
        }

        public void Write(string text)
        {
            Console.ResetColor();
            ConsoleColor color = Console.ForegroundColor;
            Write(text, color);
        }

        public void Write(string text, ConsoleColor color)
        {
            lock (_lockObject)
            {
                Console.ForegroundColor = color;
                Console.Write(text);
                Console.ResetColor();

                int lineBreaks = text.Count(c => c == '\n') + 1;
                UpdateHandles(lineBreaks);
            }
        }

        public void Write(string text, LineHandle handle)
        {
            Console.ResetColor();
            ConsoleColor color = Console.ForegroundColor;
            Write(text, handle, color);
        }

        public void Write(string text, LineHandle handle, ConsoleColor color)
        {
            if (text.Contains('\n')) throw new NotSupportedException("Line breaks not supported with line handles.");
            lock (_lockObject)
            {
                int currentRow = Console.CursorTop;
                int currentCol = Console.CursorLeft;
                try
                {
                    Console.SetCursorPosition(0, handle.Line);
                    Console.ForegroundColor = color;
                    Console.Write(text);
                }
                finally
                {
                    Console.ResetColor();
                    Console.SetCursorPosition(currentCol, currentRow);
                }
            }
        }

        public LineHandle MakeNewLineHandle()
        {
            lock (_lockObject)
            {
                LineHandle handle = new LineHandle(this, Console.CursorTop);
                _lineHandles.Add(handle);
                UpdateHandles(-1);
                Console.WriteLine();
                return handle;
            }
        }
    }
}