// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using Newtonsoft.Json.Linq;

namespace MSFSModManager.Core.PackageSources.Github
{

    public class ArtifactSelectorFactory
    {
        public static IGithubReleaseArtifactSelector Deserialize(JObject? serialized)
        {
            if (serialized == null) return new DefaultArtifactSelector();

            if (!serialized.ContainsKey("type")) throw new Parsing.JsonParsingException("JSON did not contain ArtifactSelector type.");
            if (!serialized.ContainsKey("data")) throw new Parsing.JsonParsingException("JSON did not contain ArtifactSelector data.");
        
            string type = (string)serialized["type"]!;

            switch (type)
            {
                case "default":
                    return DefaultArtifactSelector.Deserialize(serialized["data"]!);
                case "regex":
                    return RegexArtifactSelector.Deserialize(serialized["data"]!);
                default:
                    throw new Parsing.JsonParsingException("Unknown ArtifactSelector type in JSON.");
            }
        }

        public static JObject Serialize(IGithubReleaseArtifactSelector selector)
        {
            JObject serialized = new JObject();
            if (selector is DefaultArtifactSelector)
            {
                serialized.Add("type", "default");
            }
            else if (selector is RegexArtifactSelector)
            {
                serialized.Add("type", "regex");
            }
            serialized.Add("data", selector.Serialize());
            return serialized;
        }
    }

}