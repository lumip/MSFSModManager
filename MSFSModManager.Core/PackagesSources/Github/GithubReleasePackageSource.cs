// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using MSFSModManager.Core.Parsing;

namespace MSFSModManager.Core.PackageSources.Github
{

    public class GithubReleasePackageSource : AbstractPackageSource
    {
        private struct CachedRelease
        {
            public PackageManifest Manifest { get; }
            public string DownloadUrl { get; }

            public CachedRelease(PackageManifest manifest, string downloadUrl)
            {
                Manifest = manifest;
                DownloadUrl = downloadUrl;
            }
        }

        private Dictionary<VersionNumber, CachedRelease> _releaseCache;

        private GithubRepository _repository;
        
        private PackageCache _cache;

        public override string PackageId { get; }

        private List<GithubAPI.Release>? _githubReleases;

        private async Task<IEnumerable<GithubAPI.Release>> FetchGithubReleases()
        {
            if (_githubReleases == null)
            {
                _githubReleases = (await GithubAPI.FetchReleases(_repository, _artifactSelector, _client)).ToList();
            }
            return _githubReleases;
        }

        private IGithubReleaseArtifactSelector _artifactSelector;

        private HttpClient _client;

        public GithubReleasePackageSource(
            string packageId, GithubRepository repository, PackageCache cache, HttpClient client, IGithubReleaseArtifactSelector artifactSelector
        )
        {
            PackageId = packageId;
            _repository = repository;
            _cache = cache;
            _releaseCache = new Dictionary<VersionNumber, CachedRelease>();
            _githubReleases = null;
            _artifactSelector = artifactSelector;
            _client = client;
        }

        public GithubReleasePackageSource(string packageId, GithubRepository repository, PackageCache cache, HttpClient client)
            : this(packageId, repository, cache, client, new DefaultArtifactSelector())
        { }

        public GithubReleasePackageSource(GithubRepository repository, PackageCache cache, HttpClient client)
            : this(repository.Name, repository, cache, client) { }

        public override IPackageInstaller GetInstaller(IVersionNumber versionNumber)
        {
            if (!(versionNumber is VersionNumber)) throw new VersionNotAvailableException(PackageId, versionNumber);
            return GetInstaller((VersionNumber)versionNumber);
        }

        public IPackageInstaller GetInstaller(VersionNumber versionNumber)
        {
            if (_releaseCache.ContainsKey(versionNumber))
            {
                CachedRelease cachedRelease = _releaseCache[versionNumber];
                GithubArtifactDownloader downloader = new GithubArtifactDownloader(
                    PackageId, versionNumber, _repository, cachedRelease.DownloadUrl, _client, _cache
                );
                return new GithubReleasePackageInstaller(downloader);
            }
            else
            {
                IEnumerable<GithubAPI.Release> releases = FetchGithubReleases().Result;
                foreach (GithubAPI.Release release in releases)
                {
                    VersionNumber releaseVersion;
                    try
                    {
                        releaseVersion = VersionNumber.FromString(release.Name);
                    }
                    catch (FormatException)
                    {
                        GlobalLogger.Log(LogLevel.Info, $"Cannot parse version number of github release {release.Name} ({PackageId})");
                        continue;
                    }
                    if (versionNumber.Equals(releaseVersion))
                    {
                        GithubArtifactDownloader downloader = new GithubArtifactDownloader(
                            PackageId, versionNumber, _repository, release.DownloadUrl, _client, _cache
                        );
                        return new GithubReleasePackageInstaller(downloader);
                    }
                }
                throw new VersionNotAvailableException(PackageId, new VersionBounds(versionNumber));
            }
        }

        public static string LocateManifest(string packageFolder)
        {
            foreach (string manifestFile in Directory.EnumerateFiles(packageFolder, PackageDirectoryLayout.ManifestFile, SearchOption.AllDirectories))
            {
                return manifestFile;
            }
            throw new FileNotFoundException();
        }

        public override async Task<PackageManifest> GetPackageManifest(VersionBounds versionBounds, IVersionNumber gameVersion, IProgressMonitor? monitor)
        {
            foreach (VersionNumber cachedVersion in _releaseCache.Keys)
            {
                if (versionBounds.CheckVersion(cachedVersion) && gameVersion.CompareTo(_releaseCache[cachedVersion].Manifest.MinimumGameVersion) >= 0)
                {
                    return _releaseCache[cachedVersion].Manifest;
                }
            }

            monitor?.RequestPending(PackageId);
            IEnumerable<GithubAPI.Release> releases = await FetchGithubReleases();
            foreach (GithubAPI.Release release in releases)
            {
                VersionNumber releaseVersion;
                try
                {
                    releaseVersion = VersionNumber.FromString(release.Name);
                }
                catch (FormatException)
                {
                    GlobalLogger.Log(LogLevel.Info, $"Cannot parse version number of github release {release.Name} ({PackageId})");
                    continue;
                }
                string cacheId = $"{_repository.Organisation}_{_repository.Name}";
                if (versionBounds.CheckVersion(releaseVersion))
                {

                    string commitSha = await GithubAPI.GetCommitShaForTag(_repository, release.Name, _client);
                    try
                    {
                        string rawManifest = await GithubAPI.GetManifestString(_repository, commitSha, _client);
                        PackageManifest manifest = PackageManifest.Parse(PackageId, rawManifest, releaseVersion);

                        if (gameVersion.CompareTo(manifest.MinimumGameVersion) < 0)
                        {
                            GlobalLogger.Log(LogLevel.Info, $"{PackageId} v{releaseVersion} requires game version {manifest.MinimumGameVersion}, installed is {gameVersion}; skipping.");
                            continue;
                        }

                        _releaseCache.Add(releaseVersion, new CachedRelease(manifest, release.DownloadUrl));
                        return manifest;
                    }
                    catch (FileNotFoundException)
                    {
                        GlobalLogger.Log(LogLevel.Info, "Could not parse manifest from release commit, downloading full package.");
                        string packageFolder;
                        if (_cache.Contains(cacheId, releaseVersion))
                        {
                            packageFolder = _cache.GetPath(cacheId, releaseVersion);
                        }
                        else
                        {
                            var downloader = new GithubArtifactDownloader(PackageId, releaseVersion, _repository, release.DownloadUrl, _client, _cache);
                            packageFolder = await downloader.DownloadToCache(monitor);
                        }
                        string manifestFilePath = LocateManifest(packageFolder);
                        PackageManifest manifest = PackageManifest.FromFile(PackageId, manifestFilePath);

                        if (gameVersion.CompareTo(manifest.MinimumGameVersion) < 0)
                        {
                            GlobalLogger.Log(LogLevel.Info, $"{PackageId} v{releaseVersion} requires game version {manifest.MinimumGameVersion}, installed is {gameVersion}; skipping.");
                            continue;
                        }

                        _releaseCache.Add(releaseVersion, new CachedRelease(manifest, release.DownloadUrl));
                        return manifest;
                    }
                }
            }
            throw new VersionNotAvailableException(PackageId, versionBounds);
        }

        public override JToken Serialize()
        {
            JObject serialized = new JObject();
            serialized.Add("repository", _repository.Serialize());
            serialized.Add("artifact_selector", ArtifactSelectorFactory.Serialize(_artifactSelector));
            return serialized;
        }

        public static GithubReleasePackageSource Deserialize(string packageId, JToken serialized, PackageCache cache, HttpClient client)
        {
            JObject? repositoryObj = serialized["repository"] as JObject;
            if (repositoryObj == null) throw new JsonParsingException("no repository present");

            GithubRepository repository = GithubRepository.Deserialize(repositoryObj);

            JObject? selectorObj = serialized["artifact_selector"] as JObject;
            IGithubReleaseArtifactSelector selector = ArtifactSelectorFactory.Deserialize(selectorObj);

            return new GithubReleasePackageSource(
                packageId,
                repository,
                cache,
                client,
                selector
            );

        }

        public override string ToString()
        {
            return $"https://github.com/{_repository.Organisation}/{_repository.Name} (Releases, {_artifactSelector})";
        }

        public override string AsSourceString()
        {
            string sourceString = $"https://github.com/{_repository.Organisation}/{_repository.Name}";
            if (_artifactSelector is RegexArtifactSelector)
            {
                sourceString += " " + ((RegexArtifactSelector)_artifactSelector).ToString();
            }
            return sourceString;
        }

        public override async Task<IEnumerable<IVersionNumber>> ListAvailableVersions()
        {
            return (await FetchGithubReleases()).Select(r => VersionNumber.FromString(r.Name));
        }

    }
}
