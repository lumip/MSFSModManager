// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;

namespace MSFSModManager.CLI
{
    class ProgressBar
    {
        private double _currentProgress;
        private int _row;
        private int _col;

        private string _text;
        private string _totalText;

        private ConsoleRenderer.LineHandle _consoleLine;

        public ProgressBar(string text, string totalText, ConsoleRenderer.LineHandle outputLineHandle)
        {
            _row = Console.CursorTop;
            _col = Console.CursorLeft;
            _currentProgress = 0;
            _text = text;
            _totalText = totalText;
            _consoleLine = outputLineHandle;
        }

        public ProgressBar(string text, string totalText, ConsoleRenderer renderer)
            : this(text, totalText, renderer.MakeNewLineHandle())
        { }

        public void Render()
        {
            _consoleLine.Write(
                $"{_text} [ {(int)(_currentProgress)} %] {_totalText}"
            );
        }

        public void Update(double progress)
        {
            _currentProgress = progress;
        }
    }
}