// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using AvaloniaEdit;

namespace MSFSModManager.GUI.Views
{
    public partial class LogView : UserControl
    {
        public LogView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}