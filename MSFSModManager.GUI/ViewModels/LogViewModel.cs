// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.ComponentModel;
using AvaloniaEdit.Document;

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

        public ReactiveCommand<Unit, int> UpdateCaretCommand;

        // private readonly ObservableAsPropertyHelper<TextDocument> _document;
        // public TextDocument Document => _document.Value;

        public LogViewModel()
        {
            _log = string.Empty;
            // Document = new TextDocument();
            // _document = this.WhenAnyValue(x => x.Document).ToProperty(this, x => x.Document, out _document);

            UpdateCaretCommand = ReactiveCommand.Create(() => _log.Length);
        }

        void ILogger.Log(LogLevel level, string message)
        {
            _log += message + "\n";
            this.RaisePropertyChanged(nameof(Log));
            UpdateCaretCommand.Execute().Wait();
            // this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Document)));
            // Document.BeginUpdate();
            // Document.Text += message + "\n";
            // Document.EndUpdate();
        }
    }
}
