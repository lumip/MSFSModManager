using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSFSModManager.Core.PackageSources;

namespace MSFSModManager.Core
{
    public class HiddenBasePackageSourceRepositoryDecorator : IPackageSourceRepository
    {

        IPackageSourceRepository _repository;

        public HiddenBasePackageSourceRepositoryDecorator(IPackageSourceRepository repository)
        {
            _repository = repository;
        }

        public IPackageSource GetSource(string packageId)
        {
            if (packageId.StartsWith("fs-base"))
            {
                return new HiddenBasePackageSource(packageId);
            }
            return _repository.GetSource(packageId);
        }

        public Task<IEnumerable<string>> ListAvailablePackages()
        {
            return _repository.ListAvailablePackages();
        }
        
    }
}
