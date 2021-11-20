// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

namespace MSFSModManager.Core.PackageSources.Github
{
    public interface IGithubReleaseArtifactSelector : IJsonSerializable
    {
        int SelectReleaseArtifact(string[] artifacts);
    }
}
