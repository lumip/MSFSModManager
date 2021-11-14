using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MSFSModManager.Core.PackageSources.Github;

namespace MSFSModManager.Core.PackageSources
{
    public class PackageSourceRegistry : IPackageSourceRegistry
    {

        PackageCache _cache;
        HttpClient _client;

        public PackageSourceRegistry(PackageCache cache, HttpClient client)
        {
            _cache = cache;
            _client = client;
        }

        public IPackageSource Deserialize(string packageId, JToken serialized)
        {
            if (!(serialized is JObject)) throw new Parsing.JsonParsingException("Expected object in parsing.");
            string type = Parsing.JsonUtils.Cast<string>(serialized["type"]);
            switch (type)
            {
                case nameof(GithubReleasePackageSource):
                    return GithubReleasePackageSource.Deserialize(packageId, serialized["data"], _cache, _client);
                case nameof(GithubBranchPackageSource):
                    return GithubBranchPackageSource.Deserialize(packageId, serialized["data"], _cache, _client);
                default:
                    throw new ArgumentException();
            }
        }

        public static JToken Serialize(IPackageSource source)
        {
            JObject obj = new JObject();
            if (source is GithubReleasePackageSource)
            {
                obj.Add("type", nameof(GithubReleasePackageSource));
                obj.Add("data", source.Serialize());
            }
            else if (source is GithubBranchPackageSource)
            {
                obj.Add("type", nameof(GithubBranchPackageSource));
                obj.Add("data", source.Serialize());
            }
            else
            {
                throw new ArgumentException("Unknown source type.", nameof(source));
            }
            return obj;
        }

        public IPackageSource ParseSourceStrings(string packageId, string[] sourceString)
        {
            string url = sourceString[0];
            if (url.Contains("github.com"))
            {
                if (url.Contains('@'))
                {
                    string[] urlSplits = url.Split('@', 2);
                    GithubRepository repository = GithubRepository.FromUrl(urlSplits[0]);
                    string branch = urlSplits[1];
                    return new GithubBranchPackageSource(packageId, repository, branch, _cache);
                }
                else
                {
                    IGithubReleaseArtifactSelector artifactSelector = new DefaultArtifactSelector();
                    if (sourceString.Length > 1)
                    {
                        artifactSelector = new RegexArtifactSelector(sourceString[1]);
                    }
                    return new GithubReleasePackageSource(packageId, GithubRepository.FromUrl(url), _cache, _client, artifactSelector);
                }
            }
            throw new ArgumentException();
        }

        public IPackageSource GetSourceForURL(string packageId, string url, HttpClient client)
        {
            if (url.Contains("github.com"))
            {
                // if (url.Contains('@'))
                // {
                //     return new GithubBranchPackageSource();
                // }
                // else
                // {
                return new GithubReleasePackageSource(packageId, GithubRepository.FromUrl(url), _cache, client);
                // }
            }
            throw new ArgumentException();
        }
    }

}