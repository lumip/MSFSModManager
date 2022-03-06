// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2022 Lukas <lumip> Prediger

using System;
using System.IO;
using Newtonsoft.Json;

namespace MSFSModManager.Core
{

    internal class PackageDescription
    {
        public string Id { get; }
        public VersionNumber? VersionNumber { get; }

        public PackageDescription(string id, VersionNumber? versionNumber)
        {
            Id = id;
            VersionNumber = versionNumber;
        }

        public static PackageDescription Parse(string packageDescriptionText)
        {
            Parsing.PackageDescriptionData? data = JsonConvert.DeserializeObject<Parsing.PackageDescriptionData>(packageDescriptionText);
            if (!data.HasValue) throw new Parsing.ManifestParsingException("<unknown>");

            if (data.Value.Name == null) throw new Exception("JSON string does not contain a 'name' property");
            
            VersionNumber? versionNumber = null;
            if (data.Value.Version != null)
            {
                versionNumber = VersionNumber.FromString(data.Value.Version);
            }
            
            return new PackageDescription(data.Value.Name, versionNumber);

        }


        public static PackageDescription FromFile(string filePath)
        {
            var jsonText = File.ReadAllText(filePath);
            return Parse(jsonText);
        }
    }
}
