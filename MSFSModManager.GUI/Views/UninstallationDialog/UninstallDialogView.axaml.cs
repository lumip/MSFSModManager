// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2022 Lukas <lumip> Prediger

using System;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ReactiveUI;

using MSFSModManager.GUI.ViewModels;
using System.ComponentModel;

namespace MSFSModManager.GUI.Views
{
    partial class UninstallDialogView : ReactiveWindow<UninstallDialogViewModel>
    {
        public UninstallDialogView()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.WhenActivated(d => d(ViewModel!.CloseCommand.Subscribe(Close)));
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
