// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2022 Lukas <lumip> Prediger

using System.Configuration;

namespace MSFSModManager.GUI.Settings
{
    public sealed class UserSettingsSection : ConfigurationSection
    {

        private ConfigurationPropertyCollection _properties;
        protected override ConfigurationPropertyCollection Properties => _properties;

        private static readonly ConfigurationProperty _contentPath = new ConfigurationProperty(
            "contentPath", typeof(ContentPathConfigurationElement)
        );

        public UserSettingsSection()
        {
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_contentPath);
        }

        public string ContentPath
        {
            get
            {
                return ((ContentPathConfigurationElement)base["contentPath"]).Path;
            }
            set
            {
                base["contentPath"] = new ContentPathConfigurationElement(value);
            }
        }
    }
}
