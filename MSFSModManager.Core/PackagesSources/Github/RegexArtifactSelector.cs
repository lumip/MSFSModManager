using System;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MSFSModManager.Core.PackageSources.Github
{

    public class RegexArtifactSelector : IGithubReleaseArtifactSelector
    {

        Regex _regex;

        public RegexArtifactSelector(string pattern)
            : this(new Regex(pattern))
        { }

        public RegexArtifactSelector(Regex regex)
        {
            _regex = regex;
        }

        public int SelectReleaseArtifact(string[] artifacts)
        {
            int[] matchedArtifacts = artifacts.Where(a => _regex.IsMatch(a)).Select((a, i) => i).ToArray();
            if (matchedArtifacts.Length == 0)
            {
                throw new Exception("Github repository release has no matching assets!");
            }
            else if (matchedArtifacts.Length > 1)
            {
                throw new Exception("Github repository release has more than one matching asset!");
            }
            return matchedArtifacts[0];
        }

        public JToken Serialize()
        {
            return JValue.FromObject(_regex.ToString());
        }

        public static RegexArtifactSelector Deserialize(JToken serialized)
        {
            string pattern = Parsing.JsonUtils.Cast<string>(serialized);
            return new RegexArtifactSelector(pattern);
        }

        public override string ToString()
        {
            return _regex.ToString();
        }
    }
}
