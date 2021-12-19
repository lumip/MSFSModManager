// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using Newtonsoft.Json;

namespace MSFSModManager.Core.Parsing
{

    internal struct PackageDependencyData
    {
        [JsonProperty("package_version")]
        public String? PackageVersion { get; set; }
        [JsonProperty("name")]
        public String? DependencyName { get; set; }
    }
}
