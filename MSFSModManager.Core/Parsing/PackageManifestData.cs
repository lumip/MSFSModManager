// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MSFSModManager.Core.Parsing
{

    internal struct PackageManifestData
    {
        [JsonProperty("creator")]
        public String? Creator { get; set; }
        [JsonProperty("title")]
        public String? Title { get; set; }
        [JsonProperty("minimum_game_version")]
        public String? MinimumGameVersion { get; set; }
        [JsonProperty("package_version")]
        public String? PackageVersion { get; set; }
        [JsonProperty("total_package_size")]
        public String? TotalPackageSize { get; set; }
        [JsonProperty("content_type")]
        public String? Type { get; set; }
        [JsonProperty("dependencies")]
        public List<PackageDependencyData>? Dependencies { get; set; }
    }
}
