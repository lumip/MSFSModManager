// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;

namespace MSFSModManager.Core
{
    public class VersionNotAvailableException : Exception
    {
        public string PackageId { get; }
        public VersionBounds VersionBounds { get; }

        public VersionNotAvailableException(string packageId, VersionBounds versionBounds)
            : base($"Package {packageId} with version in {versionBounds} is not available.")
        {
            PackageId = packageId;
            VersionBounds = versionBounds;
        }

        public VersionNotAvailableException(string packageId, IVersionNumber versionNumber)
            : this(packageId, new VersionBounds(versionNumber)) { }
    }
}
