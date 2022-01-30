// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using System.Reactive;

using ReactiveUI;

using MSFSModManager.GUI.ViewModels;
using System.ComponentModel;

namespace MSFSModManager.GUI.Views
{
    partial class InstallDialogView : ReactiveWindow<InstallDialogViewModel>
    {
        public InstallDialogView()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.WhenActivated(d => d(ViewModel!.CloseCommand.Subscribe(_ => Close())));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ViewModel!.CancelCommand.Execute().Subscribe();
            base.OnClosing(e);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}