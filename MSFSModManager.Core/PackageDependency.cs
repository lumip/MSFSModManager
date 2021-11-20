// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;

namespace MSFSModManager.Core
{
    public struct PackageDependency
    {
        public string Id { get; }

        public VersionBounds VersionBounds { get; }

        internal PackageDependency(Parsing.PackageDependencyData data)
        {
            if (data.DependencyName != null)  Id = data.DependencyName;
            else throw new ArgumentException("DependencyName must be present.");

            if (data.PackageVersion != null) VersionBounds = new VersionBounds(VersionNumber.FromString(data.PackageVersion), VersionNumber.Infinite);
            else VersionBounds = VersionBounds.Unbounded;
        }

        public PackageDependency(string id, VersionBounds versionBounds)
        {
            Id = id;
            VersionBounds = versionBounds;
        }
    }
}
