// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.IO;
using System.Collections.Generic;

namespace MSFSModManager.Core.PackageSources
{

    /// <summary>
    /// An archive file containing package data for installation.
    ///
    /// Automatically determines the in-archive base location of
    /// the package files (i.e., the location of the manifest)
    /// and only exposes those, hiding other potential contents
    /// of the archiven file.
    /// </summary>
    public interface IPackageArchive : IDisposable
    {
        /// <summary>
        /// Enumerable of paths for all package contents (relative to the package base).
        /// </summary>
        IEnumerable<string> Entries { get; }

        /// <summary>
        /// Opens the package manifest file for reading.
        /// </summary>
        Stream OpenManifest();

        /// <summary>
        /// Extracts all package contents under the given destination.
        /// 
        /// The destination is treated as the base directory of the package, i.e.,
        /// the location in which the manifest file will be placed.
        /// </summary>
        /// <param name="destination">Path to the directory into which to extract the package.</param>
        void Extract(string destination);

    }
}
