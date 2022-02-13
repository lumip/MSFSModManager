// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2022 Lukas <lumip> Prediger

using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Windows.Input;
using ReactiveUI;
using System.Reactive.Linq;
using System.IO;
using System.Threading.Tasks;

using MSFSModManager.Core;
using MSFSModManager.Core.PackageSources;
using Avalonia.Media;

using MSFSModManager.GUI.Settings;

namespace MSFSModManager.GUI.ViewModels
{


    class SettingsViewModel : ViewModelBase
    {

        private UserSettingsBuilder _settingsBuilder;


# region Direct UI properties
        public string ContentPath
        {
            get => _settingsBuilder.ContentPath != null ? _settingsBuilder.ContentPath : "";
            set
            {
                this.RaisePropertyChanging(nameof(ContentPath));
                _settingsBuilder.ContentPath = value;
                this.RaisePropertyChanged(nameof(ContentPath));
            }
        }
#endregion

#region Derived UI Properties
        private readonly ObservableAsPropertyHelper<IBrush> _contentPathTextColor;
        public IBrush ContentPathTextColor => _contentPathTextColor.Value;
#endregion


#region UI Commands

        public ReactiveCommand<Unit, UserSettings?> ApplyCommand { get; }
        public ReactiveCommand<Unit, UserSettings?> CancelCommand { get; }


        public Interaction<Unit, string?> ContentPathFolderDialogInteraction { get; }
        public IReactiveCommand OpenContentPathFolderDialog { get; }

#endregion


        public SettingsViewModel(UserSettingsBuilder settingsBuilder)
        {
            _settingsBuilder = settingsBuilder;

            _contentPathTextColor = _settingsBuilder
                                        .WhenAnyValue(x => x.IsContentPathValid, isValid => isValid ? Brushes.Black : Brushes.Red)
                                        .ToProperty(this, x => x.ContentPathTextColor, out _contentPathTextColor);

            var applyButtonEnabled = _settingsBuilder.WhenAnyValue(x => x.IsComplete).Select(isComplete => isComplete);

            ApplyCommand = ReactiveCommand.Create(
                () => (UserSettings?)_settingsBuilder.Build(),
                applyButtonEnabled
            );
            CancelCommand = ReactiveCommand.Create(
                () => (UserSettings?)null
            );

            ContentPathFolderDialogInteraction = new Interaction<Unit, string?>();
            OpenContentPathFolderDialog = ReactiveCommand.CreateFromTask(DoOpenContentPathFolderDialog);
        }

        private bool CheckValidPath(string path)
        {
            return !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);
        }

        private async Task DoOpenContentPathFolderDialog()
        {
            string? result = await ContentPathFolderDialogInteraction.Handle(Unit.Default);
            if (!string.IsNullOrWhiteSpace(result))
            {
                ContentPath = result;
            }
        }

    }
}
