// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2022 Lukas <lumip> Prediger

using System.Configuration;

namespace MSFSModManager.GUI.Settings
{
    public class ContentPathConfigurationElement : ConfigurationElement
    {
        public ContentPathConfigurationElement() : base() { }

        public ContentPathConfigurationElement(string path)
            : base()
        {
            base["Path"] = path;
        }

        [ConfigurationProperty("Path")]
        public string Path
        {
            get
            {
                return (string)base["Path"];
            }
        }
    }
}
