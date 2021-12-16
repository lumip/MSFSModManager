// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace MSFSModManager.Core.PackageSources
{
    /// <summary>
    /// Registry/Factory for package source types; Parses URLs / source strings into IPackageSource objects.
    /// </summary>
    public interface IPackageSourceRegistry
    {
        IPackageSource Deserialize(string packageId, JToken serialized);
        IPackageSource GetSourceForURL(string packageId, string url, HttpClient client);

        IPackageSource ParseSourceStrings(string packageId, string[] sourceString);
    }

}