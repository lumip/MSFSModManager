// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021-2022 Lukas <lumip> Prediger

using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

using MSFSModManager.GUI.ViewModels;
using Avalonia.ReactiveUI;
using System.Reactive;
using System.Collections.Generic;

namespace MSFSModManager.GUI.Views
{
    partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.WhenActivated(d => d(ViewModel!.AddPackageDialogInteraction.RegisterHandler(ShowAddPackageDialogAsync)));
            this.WhenActivated(d => d(ViewModel!.InstallPackagesDialogInteraction.RegisterHandler(ShowInstallDialogAsync)));
            this.WhenActivated(d => d(ViewModel!.SettingsDialogInteraction.RegisterHandler(ShowSettingsDialogAsync)));
            this.WhenActivated(d => d(ViewModel!.UninstallPackagesDialogInteraction.RegisterHandler(ShowUninstallDialogAsync)));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async Task ShowAddPackageDialogAsync(InteractionContext<AddPackageViewModel, AddPackageDialogReturnValues> interaction)
        {
            var dialog = new AddPackageView();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<AddPackageDialogReturnValues>(this);
            interaction.SetOutput(result);
        }

        private async Task ShowInstallDialogAsync(InteractionContext<InstallDialogViewModel, IEnumerable<string>> interaction)
        {
            var dialog = new InstallDialogView();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<IEnumerable<string>>(this);
            interaction.SetOutput(result);
        }

        private async Task ShowSettingsDialogAsync(InteractionContext<SettingsViewModel, Settings.UserSettings> interaction)
        {
            var dialog = new SettingsView();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<Settings.UserSettings>(this);
            interaction.SetOutput(result);
        }

        private async Task ShowUninstallDialogAsync(InteractionContext<UninstallDialogViewModel, IEnumerable<string>> interaction)
        {
            var dialog = new UninstallDialogView();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<IEnumerable<string>>(this);
            interaction.SetOutput(result);
        }
    }
}
