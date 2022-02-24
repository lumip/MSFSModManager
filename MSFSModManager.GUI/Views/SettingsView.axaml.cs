// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2022 Lukas <lumip> Prediger

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

using System.Reactive.Linq;
using System;
using System.Threading.Tasks;

using MSFSModManager.GUI.ViewModels;
using System.Reactive;

namespace MSFSModManager.GUI.Views
{
    partial class SettingsView : ReactiveWindow<SettingsViewModel>
    {
        public SettingsView()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.WhenActivated(d => d(ViewModel!.ApplyCommand.Subscribe(Close)));
            this.WhenActivated(d => d(ViewModel!.CancelCommand.Subscribe(Close)));

            this.WhenActivated(d => d(ViewModel!.ContentPathFolderDialogInteraction.RegisterHandler(ShowOpenContentPathFolderDialog)));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async Task ShowOpenContentPathFolderDialog(InteractionContext<string?, string?> interaction)
        {
            var dialog = new OpenFolderDialog();
            dialog.Directory = interaction.Input;
            var res = await dialog.ShowAsync(this);

            interaction.SetOutput(res);
        }
    }
}
