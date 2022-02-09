// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021-2022 Lukas <lumip> Prediger

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MSFSModManager.GUI.Views
{
    public partial class PackageInstallationProgressView : UserControl
    {
        public PackageInstallationProgressView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}