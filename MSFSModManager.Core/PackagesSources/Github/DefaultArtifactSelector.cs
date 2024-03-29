// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021,2022 Lukas <lumip> Prediger

using Newtonsoft.Json.Linq;

namespace MSFSModManager.Core.PackageSources.Github
{
    public class DefaultArtifactSelector : IGithubReleaseArtifactSelector
    {
        public int SelectReleaseArtifact(string[] artifacts)
        {
            if (artifacts.Length == 0)
            {
                throw new ArtifactSelectionException("Github repository release has no assets!");
            }
            else if (artifacts.Length > 1)
            {
                throw new ArtifactSelectionException("Github repository release has more than one asset!");
            }
            return 0;
        }

        public JToken Serialize()
        {
            return new JObject();
        }

        public static DefaultArtifactSelector Deserialize(JToken serialized)
        {
            return new DefaultArtifactSelector();
        }

        public override string ToString()
        {
            return "single zip";
        }
    }
}
