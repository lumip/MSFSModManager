// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021-2022 Lukas <lumip> Prediger

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MSFSModManager.GUI.Views
{
    public partial class DependencyResolutionPageView : UserControl
    {
        public DependencyResolutionPageView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}