// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2022 Lukas <lumip> Prediger

using System;
using Newtonsoft.Json;

namespace MSFSModManager.Core.Parsing
{

    internal struct PackageDescriptionData
    {
        [JsonProperty("name")]
        public String? Name { get; set; }
        [JsonProperty("version")]
        public String? Version { get; set; }
    }
}
