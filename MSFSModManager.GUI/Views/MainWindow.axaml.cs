// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

using MSFSModManager.GUI.ViewModels;
using MSFSModManager.Core.PackageSources;
using Avalonia.ReactiveUI;
using System.Reactive;

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

        private async Task ShowInstallDialogAsync(InteractionContext<InstallDialogViewModel, Unit> interaction)
        {
            var dialog = new InstallDialogView();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<Unit>(this);
            interaction.SetOutput(result);
        }
    }
}