using System.Collections.Generic;
using System.Threading.Tasks;

namespace MSFSModManager.Core.PackageSources
{

    public interface IPackageSourceRepository
    {
        Task<IEnumerable<string>> ListAvailablePackages();

        IPackageSource GetSource(string packageId);
    }
}
