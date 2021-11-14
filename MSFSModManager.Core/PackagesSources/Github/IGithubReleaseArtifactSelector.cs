namespace MSFSModManager.Core.PackageSources.Github
{
    public interface IGithubReleaseArtifactSelector : IJsonSerializable
    {
        int SelectReleaseArtifact(string[] artifacts);
    }
}
