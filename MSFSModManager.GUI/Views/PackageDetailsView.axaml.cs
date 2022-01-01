// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MSFSModManager.Core.PackageSources;
using MSFSModManager.GUI.ViewModels;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Avalonia.Controls.ApplicationLifetimes;

namespace MSFSModManager.GUI.Views
{
    class PackageDetailsView : UserControl
    {
        public PackageDetailsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


    }
}