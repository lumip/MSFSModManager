// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021,2022 Lukas <lumip> Prediger

using System;
using System.ComponentModel;
using System.Collections.Generic;

using MSFSModManager.Core;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;

namespace MSFSModManager.GUI.ViewModels
{
    class LogViewModel : ViewModelBase, ILogger, INotifyPropertyChanged
    {

        // textbox.CaretIndex = int.MaxValue;

        private string _log;
        public string Log => _log;

        private List<(LogLevel, string)> _lines;

        public ReactiveCommand<Unit, int> UpdateCaretCommand;

        // private readonly ObservableAsPropertyHelper<TextDocument> _document;
        // public TextDocument Document => _document.Value;

        public LogViewModel()
        {
            _log = string.Empty;
            _lines = new List<(LogLevel, string)>();
            // Document = new TextDocument();
            // _document = this.WhenAnyValue(x => x.Document).ToProperty(this, x => x.Document, out _document);

            UpdateCaretCommand = ReactiveCommand.Create(() => _log.Length);
        }

        void ILogger.Log(LogLevel level, string message)
        {
            _lines.Add((level, message));
            _log += message + "\n";
            this.RaisePropertyChanged(nameof(Log));
            UpdateCaretCommand.Execute().Wait();
            // this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Document)));
            // Document.BeginUpdate();
            // Document.Text += message + "\n";
            // Document.EndUpdate();
        }

        public void DumpToConsole()
        {
            foreach (var line in _lines)
            {
                Console.WriteLine(line.Item2);
            }
        }
    }
}
