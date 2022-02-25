// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2022 Lukas <lumip> Prediger

using System;

namespace MSFSModManager.Core.PackageSources.Github
{

    public class ArtifactSelectionException : Exception
    {
        public ArtifactSelectionException(string msg) : base(msg) { }
    }
}
