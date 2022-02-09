// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using MSFSModManager.Core.PackageSources;

namespace MSFSModManager.Core
{
    public class EphemeralPackageSourceRepositoryDecorator : IPackageSourceRepository
    {

        Dictionary<string, IPackageSource> _sources;
        IPackageSourceRepository _baseRepository;

        public EphemeralPackageSourceRepositoryDecorator(
            IEnumerable<IPackageSource> sources, IPackageSourceRepository baseRepository)
        {
            _sources = new Dictionary<string, IPackageSource>();
            foreach (var source in sources)
            {
                _sources.Add(source.PackageId, source);
            }
            _baseRepository = baseRepository;
        }

        public IPackageSource GetSource(string packageId)
        {
            if (_sources.ContainsKey(packageId)) return _sources[packageId];
            return _baseRepository.GetSource(packageId);
        }

        public bool HasSource(string packageId)
        {
            return _sources.ContainsKey(packageId) || _baseRepository.HasSource(packageId);
        }

        public async Task<IEnumerable<string>> ListAvailablePackages(
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            return _sources.Keys.Union(await _baseRepository.ListAvailablePackages(cancellationToken));
        }
        
    }
}
