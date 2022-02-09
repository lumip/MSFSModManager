// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021-2022 Lukas <lumip> Prediger

namespace MSFSModManager.GUI.ViewModels
{

    class InstallationCandidateViewModel : ViewModelBase
    {

        public string Id { get; }
        public string VersionBounds { get; }

        public InstallationCandidateViewModel(string id, string versionBounds)
        {
            Id = id;
            VersionBounds = versionBounds;
        }

    }

}
