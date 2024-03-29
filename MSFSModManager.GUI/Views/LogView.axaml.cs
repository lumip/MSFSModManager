// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

using AvaloniaEdit;

using MSFSModManager.GUI.ViewModels;

namespace MSFSModManager.GUI.Views
{
    partial class LogView : ReactiveUserControl<LogViewModel>
    {
        public LogView()
        {
            InitializeComponent();

            var logTextBox = this.FindControl<TextBox>("LogTextBox");
            this.WhenActivated(d => d(this.ViewModel!.UpdateCaretCommand.Subscribe(caret => logTextBox.CaretIndex = caret)));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}