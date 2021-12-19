// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;

namespace MSFSModManager.Core.Parsing
{

    public class ManifestParsingException : Exception
    {
        public string Id { get; }

        public ManifestParsingException(string id, Exception baseException)
            : base($"Error while parsing manifest for {id}.", baseException)
        {
            Id = id;
        }

        public ManifestParsingException(string id)
            : base($"Error while parsing manifest for {id}.")
        {
            Id = id;
        }
    }

}