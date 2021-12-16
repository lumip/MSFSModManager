// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Newtonsoft.Json.Linq;

using MSFSModManager.Core.Parsing;

namespace MSFSModManager.Core.PackageSources.Github
{
    public class GithubBranchPackageSource : AbstractPackageSource
    {
        private GithubRepository _repository;
        private string _branch;

        private PackageCache _cache;

        private HttpClient _client;

        public override string PackageId { get; }

        public GithubBranchPackageSource(string packageId, GithubRepository repository, string branchName, PackageCache cache, HttpClient client)
        {
            PackageId = packageId;
            _repository = repository;
            _branch = branchName;
            _cache = cache;
            _client = client;
        }

        private List<GithubAPI.Commit>? _branchCommits;

        private async Task<IEnumerable<GithubAPI.Commit>> FetchCommits()
        {
            if (_branchCommits == null)
            {
                _branchCommits = (await GithubAPI.GetBranchCommits(_repository, _branch)).ToList();
            }
            return _branchCommits;
        }

        public override IPackageInstaller GetInstaller(IVersionNumber versionNumber)
        {
            if (!(versionNumber is GitCommitVersionNumber))
            {
                throw new VersionNotAvailableException(PackageId, versionNumber);
            }
            
            string commitSha = ((GitCommitVersionNumber)versionNumber).Commit;

            string downloadUrl = $"https://api.github.com/repos/{_repository.Organisation}/{_repository.Name}/zipball/{commitSha}";
            GithubArtifactDownloader downloader = new GithubArtifactDownloader(
                PackageId, versionNumber, _repository, downloadUrl, _client, _cache
            );
            return new GithubReleasePackageInstaller(downloader);
        }

        public override async Task<PackageManifest> GetPackageManifest(VersionBounds versionBounds, IVersionNumber gameVersion, IProgressMonitor? monitor = null)
        {
            foreach (var commit in await FetchCommits())
            {
                GitCommitVersionNumber version = new GitCommitVersionNumber(commit.Sha, commit.Date);
                if (!versionBounds.CheckVersion(version))
                    continue;

                string rawManifest = await GithubAPI.GetManifestString(_repository, commit.Sha);
                PackageManifest manifest;
                try
                {
                    manifest = PackageManifest.Parse(PackageId, rawManifest, sourceVersion: version);
                }
                catch (ArgumentException)
                {
                    GlobalLogger.Log(LogLevel.CriticalError, $"Manifest for {PackageId} github commit {commit.Sha} does not provide a version number.");
                    throw new PackageNotAvailableException(PackageId);
                }

                if (gameVersion.CompareTo(manifest.MinimumGameVersion) < 0)
                {
                    GlobalLogger.Log(LogLevel.Info, $"{PackageId} branch {_branch} (latest commit: {commit.Sha} requires game version {manifest.MinimumGameVersion}, installed is {gameVersion}; skipping.");
                    throw new PackageNotAvailableException(PackageId);
                }
                return manifest;
            }
            throw new VersionNotAvailableException(PackageId, versionBounds);
        }

        public override string ToString()
        {
            return $"https://github.com/{_repository.Organisation}/{_repository.Name} (Branch {_branch})";
        }

        public override async Task<IEnumerable<IVersionNumber>> ListAvailableVersions()
        {
            return (await FetchCommits()).Select(c => new GitCommitVersionNumber(c.Sha, c.Date));
        }

        public override JToken Serialize()
        {
            JObject serialized = new JObject();
            serialized.Add("repository", _repository.Serialize());
            serialized.Add("branch", _branch);
            return serialized;
        }

        public static GithubBranchPackageSource Deserialize(string packageId, JToken serialized, PackageCache cache, HttpClient client)
        {
            JObject? repositoryObj = serialized["repository"] as JObject;
            if (repositoryObj == null) throw new JsonParsingException("JSON does not contain 'repository'.");

            GithubRepository repository = GithubRepository.Deserialize(repositoryObj);

            string branch = JsonUtils.CastMember<string>(serialized, "branch");

            return new GithubBranchPackageSource(
                packageId,
                repository,
                branch,
                cache,
                client
            );
        }
    }
}