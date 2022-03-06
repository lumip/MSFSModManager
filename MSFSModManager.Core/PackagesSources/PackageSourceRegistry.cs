// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.Net.Http;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
            if (!(serialized is JObject)) throw new Parsing.JsonParsingException("Expected object in parsing PackageSource JSON.");
            JObject serializedObj = (JObject)serialized;
            if (!(serializedObj.ContainsKey("type") && serializedObj.ContainsKey("data")))
                throw new Parsing.JsonParsingException("PackageSource JSON does not contain 'type' or 'data'.");

            string type = Parsing.JsonUtils.CastMember<string>(serialized, "type");
            switch (type)
            {
                case nameof(GithubReleasePackageSource):
                    return GithubReleasePackageSource.Deserialize(packageId, serialized["data"]!, _cache, _client);
                case nameof(GithubBranchPackageSource):
                    return GithubBranchPackageSource.Deserialize(packageId, serialized["data"]!, _cache, _client);
                case nameof(ZipFilePackageSource):
                    return ZipFilePackageSource.Deserialize(packageId, serialized["data"]!);
                default:
                    throw new ArgumentException();
            }
        }

        public static JToken Serialize(IPackageSource source)
        {
            JObject obj = new JObject();
            obj.Add("type", source.GetType().Name);
            obj.Add("data", source.Serialize());
            return obj;
        }

        public IPackageSource ParseSourceStrings(string packageId, string[] sourceString)
        {
            if (sourceString.Length == 0) throw new ArgumentException();
            string uri = sourceString[0];
            if (uri.Contains("github.com"))
            {
                if (uri.Contains('@'))
                {
                    string[] uriSplits = uri.Split('@', 2);
                    GithubRepository repository = GithubRepository.FromUrl(uriSplits[0]);
                    string branch = uriSplits[1];
                    return new GithubBranchPackageSource(packageId, repository, branch, _cache, _client);
                }
                else
                {
                    IGithubReleaseArtifactSelector artifactSelector = new DefaultArtifactSelector();
                    if (sourceString.Length > 1)
                    {
                        artifactSelector = new RegexArtifactSelector(sourceString[1]);
                    }
                    return new GithubReleasePackageSource(packageId, GithubRepository.FromUrl(uri), _cache, _client, artifactSelector);
                }
            }

            try
            {
                if (new Uri(uri).IsFile)
                {
                    if (uri.EndsWith(".zip"))
                    {
                        return new ZipFilePackageSource(packageId, uri);
                    }
                }
            }
            catch (UriFormatException)
            {
                throw new ArgumentException();
            }
            catch (FileNotFoundException)
            {
                throw new ArgumentException();
            }
            throw new ArgumentException();
        }

        public async Task<IPackageSource> ParseSourceStrings(string[] sourceString, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sourceString.Length == 0) throw new ArgumentException();
            string uri = sourceString[0];
            if (uri.Contains("github.com"))
            {
                if (uri.Contains('@'))
                {
                    string[] uriSplits = uri.Split('@', 2);
                    GithubRepository repository = GithubRepository.FromUrl(uriSplits[0]);
                    string branch = uriSplits[1];
                    return await GithubBranchPackageSource.CreateFromRepository(repository, branch, _cache, _client);
                }
                else
                {
                    IGithubReleaseArtifactSelector artifactSelector = new DefaultArtifactSelector();
                    if (sourceString.Length > 1)
                    {
                        artifactSelector = new RegexArtifactSelector(sourceString[1]);
                    }
                    return await GithubReleasePackageSource.CreateFromRepository(GithubRepository.FromUrl(uri), _cache, _client, artifactSelector);
                }
            }

            try
            {
                if (new Uri(uri).IsFile)
                {
                    if (uri.EndsWith(".zip"))
                    {
                        string packageId = Path.GetFileNameWithoutExtension(uri);
                        return new ZipFilePackageSource(packageId, uri);
                    }
                }
            }
            catch (UriFormatException)
            {
                throw new ArgumentException();
            }
            catch (FileNotFoundException)
            {
                throw new ArgumentException();
            }
            throw new ArgumentException();
        }
    }

}