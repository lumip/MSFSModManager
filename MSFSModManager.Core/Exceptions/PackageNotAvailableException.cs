// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;

namespace MSFSModManager.Core
{
    public class PackageNotAvailableException : Exception
    {
        public string PackageId { get; }

        public PackageNotAvailableException(string packageId)
            : base($"Package {packageId} is not available.")
        {
            PackageId = packageId;
        }
    }
}
