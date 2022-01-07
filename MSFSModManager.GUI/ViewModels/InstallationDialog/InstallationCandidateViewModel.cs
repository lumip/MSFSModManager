using MSFSModManager.Core;

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
