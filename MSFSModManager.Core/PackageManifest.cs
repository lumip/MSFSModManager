// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MSFSModManager.Core
{

    public class PackageManifest
    {

        public string? Creator { get; }

        public string Id { get; }

        public string Title { get; }
        public IVersionNumber MinimumGameVersion { get; }
        public VersionNumber Version { get; }
        public IVersionNumber SourceVersion { get; }

        public string Type { get; }

        private PackageDependency[] _dependencies;
        public IEnumerable<PackageDependency> Dependencies => _dependencies.AsEnumerable();

        public PackageManifest(
            string id,
            string title,
            VersionNumber versionNumber,
            IVersionNumber minimumGameVersion,
            string type,
            IEnumerable<PackageDependency> dependencies,
            string? creator = null,
            IVersionNumber? sourceVersionNumber = null
        )
        {
            Id = id;
            Title = title;
            Version = versionNumber;
            MinimumGameVersion = minimumGameVersion;
            _dependencies = dependencies.ToArray();

            Type = type;

            Creator = creator;
            SourceVersion = sourceVersionNumber ?? Version;
            // TotalPackageSize = null;
        }

        public static PackageManifest Parse(string id, string manifestText, VersionNumber? forceVersion = null, IVersionNumber? sourceVersion = null)
        {
            Parsing.PackageManifestData? data = JsonConvert.DeserializeObject<Parsing.PackageManifestData>(manifestText);
            if (!data.HasValue) throw new Parsing.ManifestParsingException("<unknown>");

            string? creator = data.Value.Creator;

            string title = data.Value.Title ?? "";

            VersionNumber versionNumber;
            if (forceVersion != null)
            {
                versionNumber = forceVersion;
            }
            else if (data.Value.PackageVersion != null)
            {
                versionNumber = VersionNumber.FromString(data.Value.PackageVersion);
            }
            else
            {
                throw new ArgumentException("Version number missing in manifest means that it must be given via argument.", nameof(forceVersion));
            }

            IVersionNumber minimumGameVersion = (data.Value.MinimumGameVersion != null) ? 
                VersionNumber.FromString(data.Value.MinimumGameVersion) : VersionNumber.Zero;

            string type = (data.Value.Type == null || data.Value.Type == "") ? "UNKNOWN" : data.Value.Type;

            // TotalPackageSize = data.TotalPackageSize;

            IEnumerable<PackageDependency> dependencies = data.Value.Dependencies.Select(rawDependency => new PackageDependency(rawDependency));

            try
            {
                return new PackageManifest(id, title, versionNumber, minimumGameVersion, type, dependencies, creator, sourceVersion);
            }
            catch (Exception e)
            {
                throw new Parsing.ManifestParsingException(id, e);
            }
        }

        public static PackageManifest FromFile(string filePath)
        {
            string id = PackageDirectoryLayout.GetPackageId(filePath);
            try
            {
                return FromFile(id, filePath);
            }
            catch (FileNotFoundException e)
            {
                throw new PackageNotInstalledException(id, e);
            }
        }

        public static PackageManifest FromFile(string id, string filePath)
        {
            string manifestText = File.ReadAllText(filePath);

            return Parse(id, manifestText);
        }

        public static PackageManifest FromStream(string id, Stream stream)
        {
            string manifestText = new StreamReader(stream).ReadToEnd();
            return Parse(id, manifestText);
        }
    }
}
