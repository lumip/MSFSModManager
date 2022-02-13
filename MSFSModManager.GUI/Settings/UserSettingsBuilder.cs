// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2022 Lukas <lumip> Prediger

using System;
using System.IO;
using System.Configuration;

using ReactiveUI;

namespace MSFSModManager.GUI.Settings
{
    class UserSettingsBuilder : ReactiveObject
    {

        private string? _contentPath;
        public string? ContentPath
        {
            get => _contentPath;
            set => this.RaiseAndSetIfChanged(ref _contentPath, value);
        }

        private readonly ObservableAsPropertyHelper<bool> _isContentPathValid;
        public bool IsContentPathValid => _isContentPathValid.Value;

        private readonly ObservableAsPropertyHelper<bool> _isComplete;
        public bool IsComplete => _isComplete.Value;

        public UserSettingsBuilder()
        {
            _isContentPathValid = this
                                    .WhenAnyValue(x => x.ContentPath, path => !string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                                    .ToProperty(this, x => x.IsContentPathValid, out _isContentPathValid);

            _isComplete = this
                            .WhenAnyValue(x => x.IsContentPathValid)
                            .ToProperty(this, x => x.IsComplete, out _isComplete);
        }

        public UserSettings Build()
        {
            if (!IsComplete) throw new ArgumentException("Builder is incomplete!");

            return new UserSettings(ContentPath!);
        }

        public static UserSettingsBuilder LoadFromConfigFile()
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoaming);
            var builder = new UserSettingsBuilder();

            if (configuration.Sections[UserSettings.SECTION_NAME] is UserSettingsSection settingsSection)
            {
                builder.ContentPath = settingsSection.ContentPath;
            }

            return builder;
        }
    }
}
