// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2022 Lukas <lumip> Prediger

using System;
using System.IO;
using System.Configuration;

using ReactiveUI;

using MSFSModManager.Core;
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

        public static UserSettingsBuilder LoadFromSettings(UserSettings settings)
        {
            var builder = new UserSettingsBuilder();
            builder.ContentPath = settings.ContentPath;

            return builder;
        }

        public void FillDefaultsForMissing()
        {
            if (string.IsNullOrWhiteSpace(ContentPath))
            {
                try
                {
                    ContentPath = ConfigReader.ReadContentPathFromDefaultLocations();
                    GlobalLogger.Log(LogLevel.Info, $"Content path read from MSFS configuration: {ContentPath} .");
                }
                catch (Exception)
                {
                    GlobalLogger.Log(LogLevel.Warning, "Could not read content path from MSFS configuration.");
                }
            }
        }
    }
}
