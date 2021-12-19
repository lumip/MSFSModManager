// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;

namespace MSFSModManager.Core
{


    public class PackageNotInstalledException : Exception
    {
        public string Id { get; }

        public PackageNotInstalledException(string id, Exception baseException)
            : base($"The package {id} was not installed (no manifest exists).", baseException)
        {
            Id = id;
        }
    }

}
