// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2022 Lukas <lumip> Prediger

using System.Configuration;

namespace MSFSModManager.GUI.Settings
{
    class UserSettings
    {

        internal static readonly string SECTION_NAME = "userSettings";

        public string ContentPath { get; }

        internal UserSettings(string contentPath)
        {
            ContentPath = contentPath;
        }

        public void Save()
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoaming);
            UserSettingsSection? settingsSection = configuration.Sections[SECTION_NAME] as UserSettingsSection;
            if (settingsSection == null)
            {
                settingsSection = new UserSettingsSection();
                configuration.Sections.Add(SECTION_NAME, settingsSection);
            }

            settingsSection.ContentPath = ContentPath;
            configuration.Save(ConfigurationSaveMode.Modified);
        }

    }
}
