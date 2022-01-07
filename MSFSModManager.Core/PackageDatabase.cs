// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using MSFSModManager.Core.Parsing;
using MSFSModManager.Core.PackageSources;

namespace MSFSModManager.Core
{

    /// <summary>
    /// Database of all locally installed packages and/or known package sources.
    /// </summary>
    public class PackageDatabase : IPackageDatabase
    {

        public static readonly string COMMUNITY_PACKAGE_PATH = "Community";
        public static readonly string SYSTEM_PACKAGE_PATH = "Official";

        private Dictionary<string, InstalledPackage> _packages;
        private List<string> _erroredManifests;

        private string _installationPath;

        public IPackageSourceRegistry SourceRegistry;

        public PackageDatabase(string installationPath, IPackageSourceRegistry packageSourceRegistry)
        {
            _installationPath = installationPath;
            _packages = new Dictionary<string, InstalledPackage>();
            _erroredManifests = new List<string>();
            SourceRegistry = packageSourceRegistry;


            var installationDirectory = new DirectoryInfo(installationPath);
            foreach (var file in installationDirectory.GetFiles(PackageDirectoryLayout.ManifestFile, SearchOption.AllDirectories))
            {
                string fullPackagePath = Path.GetDirectoryName(file.FullName);
                string packagePath = Path.GetRelativePath(installationPath, fullPackagePath);
                PackageManifest? manifest = null;
                string packageId;
                try
                {
                    manifest = PackageManifest.FromFile(file.FullName);
                    packageId = manifest.Id;
                }
                catch (ManifestParsingException e)
                {
                    GlobalLogger.Log(LogLevel.Warning, $"Could not read manifest for {e.Id} ({e.InnerException.Message}).");
                    _erroredManifests.Add(e.Id);
                    continue;
                }

                string packageSourceFilePath = Path.Join(fullPackagePath, PackageDirectoryLayout.PackageSourceFile);
                IPackageSource? packageSource = null;
                if (File.Exists(packageSourceFilePath))
                {
                    string packageSourceJson = File.ReadAllText(packageSourceFilePath);
                    packageSource = packageSourceRegistry.Deserialize(packageId, JToken.Parse(packageSourceJson));
                }
                _packages.Add(packageId, new InstalledPackage(packageId, packagePath, manifest, packageSource));
            }

            foreach (var file in installationDirectory.GetFiles(PackageDirectoryLayout.PackageSourceFile, SearchOption.AllDirectories))
            {
                string fullPackagePath = Path.GetDirectoryName(file.FullName);
                string packagePath = Path.GetRelativePath(installationPath, fullPackagePath);
                if (File.Exists(Path.Join(fullPackagePath, PackageDirectoryLayout.ManifestFile)))
                {
                    continue;
                }

                string packageId = PackageDirectoryLayout.GetPackageId(file.FullName);
                string packageSourceJson = File.ReadAllText(file.FullName);

                IPackageSource packageSource = packageSourceRegistry.Deserialize(packageId, JToken.Parse(packageSourceJson));
                _packages.Add(packageId, new InstalledPackage(packageId, packagePath, null, packageSource));
            }
        }

        public IEnumerable<string> Errored => _erroredManifests.AsReadOnly();
        public IEnumerable<InstalledPackage> Packages => _packages.Values;

        public IEnumerable<InstalledPackage> CommunityPackages =>
            _packages.Values.Where(installedPackage => installedPackage.IsCommunityPackage);

        public IEnumerable<InstalledPackage> OfficialPackages =>
            _packages.Values.Where(installedPackage => installedPackage.IsSystemPackage);

        public bool Contains(string packageId)
        {
            return _packages.ContainsKey(packageId);
        }

        public bool Contains(string packageId, VersionBounds versionBounds)
        {
            if (Contains(packageId))
            {
                if (_packages[packageId].Manifest == null) return false;
                PackageManifest manifest = _packages[packageId].Manifest!;
                return versionBounds.CheckVersion(manifest.Version);
            }
            return false;
        }

        public InstalledPackage GetInstalledPackage(string packageId)
        {
            try
            {
                InstalledPackage package = _packages[packageId];
                return package;
            }
            catch (KeyNotFoundException e)
            {
                throw new PackageNotInstalledException(packageId, e);
            }
        }

        public void AddPackageSource(string packageId, IPackageSource packageSource)
        {
            string fullPackagePath;
            if (!Contains(packageId))
            {
                string packagePath = Path.Join(COMMUNITY_PACKAGE_PATH, packageId);
                fullPackagePath = Path.Join(_installationPath, packagePath);
                Directory.CreateDirectory(fullPackagePath);
                _packages.Add(packageId, new InstalledPackage(packageId, packagePath, null, packageSource));
            }
            else
            {
                _packages[packageId].PackageSource = packageSource;
                fullPackagePath = Path.Join(_installationPath, _packages[packageId].PackagePath);
            }

            string packageSourceFilePath = Path.Join(fullPackagePath, PackageDirectoryLayout.PackageSourceFile);
            File.WriteAllText(packageSourceFilePath, PackageSourceRegistry.Serialize(packageSource).ToString(Formatting.Indented));
        }

        public async Task InstallPackage(IPackageInstaller installer, IProgressMonitor? monitor = null)
        {
            string packagePath = Path.Join(COMMUNITY_PACKAGE_PATH, installer.PackageId);
            string fullPackagePath = Path.Join(_installationPath, packagePath);
            if (!Contains(installer.PackageId))
            {
                Directory.CreateDirectory(fullPackagePath);
                _packages.Add(installer.PackageId, new InstalledPackage(installer.PackageId, packagePath, null, null));
            }
            Debug.Assert(packagePath == _packages[installer.PackageId].PackagePath);

            if (File.Exists(Path.Join(fullPackagePath, PackageDirectoryLayout.ManifestFile)))
            {
                GlobalLogger.Log(LogLevel.Info, $"Removing previous installation of {installer.PackageId}.");
                Uninstall(installer.PackageId);
            }
            try
            {
                await installer.Install(fullPackagePath, monitor);
                GlobalLogger.Log(LogLevel.Info, $"Installation of {installer.PackageId} completed.");
            }
            catch (Exception e)
            {
                Uninstall(installer.PackageId);
                throw e;
            }
        }

        public void Uninstall(string packageId)
        {
            if (!Contains(packageId)) return;

            InstalledPackage package = _packages[packageId];
            package.Manifest = null;

            if (package.PackagePath.StartsWith(SYSTEM_PACKAGE_PATH))
            {
                throw new NotSupportedException("Cannot uninstall system packages.");
            }

            string fullPackagePath = Path.Join(_installationPath, package.PackagePath);

            DirectoryInfo packageDirectory = new DirectoryInfo(fullPackagePath);
            foreach (var file in packageDirectory.EnumerateFiles())
            {
                if (file.Name != PackageDirectoryLayout.PackageSourceFile)
                {
                    file.Delete();
                }
            }

            foreach (var dir in packageDirectory.EnumerateDirectories())
            {
                dir.Delete(recursive: true);
            }

            // remove the entry from database, if it is neither installed nor a source is known
            if (package.PackageSource == null) _packages.Remove(packageId);

            // remove the directory, if it is now empty
            try
            {
                packageDirectory.Delete();
            }
            catch (IOException)
            {
                // ignore: directory was not empty, which can happen if we retained the package source file
            }
        }

        public void RemovePackageSource(string packageId)
        {
            if (!Contains(packageId)) return;

            InstalledPackage package = _packages[packageId];
            package.PackageSource = null;

            string fullPackagePath = Path.Join(_installationPath, _packages[packageId].PackagePath);
            string fullFilePath = Path.Join(fullPackagePath, PackageDirectoryLayout.PackageSourceFile);
            File.Delete(fullFilePath);

            // remove the entry from database, if it is neither installed nor a source is known
            if (package.Manifest == null) _packages.Remove(packageId);

            // remove the directory, if it is now empty
            try
            {
                Directory.Delete(fullPackagePath);
            }
            catch (IOException)
            {
                // ignore: directory was not empty, which can happen if we only removed the package source file
                // without uninstalling the package
            }
        }
    }
}