using System.Threading.Tasks;
using System.Net.Http;

namespace MSFSModManager.Core.PackageSources
{
    public interface IPackageInstaller
    {
        string PackageId { get; }

        Task Install(string destination, IProgressMonitor? monitor = null);
    }

}
