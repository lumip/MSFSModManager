using System;
using Newtonsoft.Json.Linq;

namespace MSFSModManager.Core.PackageSources.Github
{

    public class ArtifactSelectorFactory
    {
        public static IGithubReleaseArtifactSelector Deserialize(JObject? serialized)
        {
            if (serialized == null) return new DefaultArtifactSelector();

            string type = (string)serialized["type"];
            switch (type)
            {
                case "default":
                    return DefaultArtifactSelector.Deserialize(serialized["data"]);
                case "regex":
                    return RegexArtifactSelector.Deserialize(serialized["data"]);
                default:
                    throw new Exception("Unknown artifact selector in serialization.");
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