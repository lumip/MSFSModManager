// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System.IO;

namespace MSFSModManager.Core
{

    public static class PackageDirectoryLayout
    {
        public static string GetPackageId(string manifestOrSourceFilePath)
        {
            return Path.GetFileName(Path.GetDirectoryName(manifestOrSourceFilePath));
        }
        
        public static readonly string ManifestFile = "manifest.json";
        public static readonly string PackageSourceFile = "packageSource.json";
    }

}