// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using MSFSModManager.Core.PackageSources;

namespace MSFSModManager.Core
{

    public class InstalledPackage
    {
        public string Id { get; }

        public string PackagePath { get; }
        public IPackageSource? PackageSource { get; set; }
        public PackageManifest? Manifest { get; }

        public VersionNumber? Version => (Manifest == null) ? null : Manifest.Version;

        public InstalledPackage(string id, string path, PackageManifest? manifest, IPackageSource? source)
        {
            Id = id;
            PackagePath = path;
            Manifest = manifest;
            PackageSource = source;
        }

        public string Type => (Manifest == null) ? "UNKNOWN" : Manifest.Type;
    }

}