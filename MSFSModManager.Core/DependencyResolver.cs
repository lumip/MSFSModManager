// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

using MSFSModManager.Core.PackageSources;

namespace MSFSModManager.Core
{

    public static class DependencyResolver
    {

        public static async Task<IEnumerable<PackageManifest>> ResolveDependencies(
            IEnumerable<PackageDependency> installationCandidates, 
            IPackageSourceRepository packageSources,
            IVersionNumber gameVersion,
            IProgressMonitor? monitor = null
        )
        {
            Dictionary<string, DependencyNode> dependencyNodes = new Dictionary<string, DependencyNode>();
            DependencyNode dependencyGraph = new DependencyNode("user requested");
            Queue<DependencyNode> resolverQueue = new Queue<DependencyNode>();

            foreach (var candidate in installationCandidates)
            {
                DependencyNode node = new DependencyNode(candidate.Id);
                dependencyNodes.Add(node.PackageId, node);
                resolverQueue.Enqueue(node);
                dependencyGraph.AddChild(node, candidate.VersionBounds);
            }

            while (resolverQueue.Count > 0)
            {
                DependencyNode node = resolverQueue.Dequeue();
                if (node.IsActualized) continue; // package is already resolved, do not resolve again

                // dependency/package has no (longer any) parents, i.e., is not required, skip it
                if (node.Parents.Count() == 0)
                {
                    dependencyNodes.Remove(node.PackageId);
                    continue;
                }

                try
                {
                    PackageManifest manifest = await packageSources.GetSource(node.PackageId).GetPackageManifest(
                        node.VersionBounds, gameVersion, monitor
                    );

                    string parentsString = String.Join(", ", node.Parents.Select(p => $"{p.PackageId} {p.ActualizedVersion}"));
                    GlobalLogger.Log(LogLevel.Info, $"Resolved package {manifest.Id} as {manifest.Version} (from {parentsString})");

                    node.Actualize(manifest);
                    foreach (var dependency in manifest.Dependencies)
                    {
                        DependencyNode dependencyNode;
                        if (dependencyNodes.ContainsKey(dependency.Id))
                        {
                            dependencyNode = dependencyNodes[dependency.Id];
                            if (dependencyNode.IsActualized)
                            {
                                // if the dependency was already actualized to a specific version,
                                // we remove that actualization to cause a re-evaluation of version bounds
                                // for the dependency later
                                dependencyNode.RemoveActualization();
                            }
                        }
                        else
                        {
                            dependencyNode = new DependencyNode(dependency.Id);
                            dependencyNodes.Add(dependency.Id, dependencyNode);
                        }

                        node.AddChild(dependencyNode, dependency.VersionBounds);
                        resolverQueue.Enqueue(dependencyNode);
                    }
                }
                catch (VersionNotAvailableException)
                {
                    // Check all parents to find those for which the dependency cannot be satisfied
                    foreach (var parent in node.Parents)
                    {
                        if (parent == dependencyGraph) continue;
                        Debug.Assert(parent.ActualizedManifest != null);
                        
                        VersionBounds bounds = parent.ActualizedManifest.Dependencies.Where(d => d.Id == node.PackageId).First().VersionBounds;
                        try
                        {
                            await packageSources.GetSource(node.PackageId).GetPackageManifest(node.VersionBounds, gameVersion, monitor);
                        }
                        catch (VersionNotAvailableException)
                        {
                            GlobalLogger.Log(LogLevel.Error, $"Cannot satisfy dependency {node.PackageId} {node.VersionBounds} of {parent.PackageId} {parent.ActualizedVersion}");
                            // this parent cannot be satisfied. 
                            VersionNumber version = parent.ActualizedVersion!;
                            try
                            {
                                VersionBounds versionBounds = parent.VersionBounds.Combine(new VersionBounds(VersionNumber.Zero, version));
                                parent.RemoveActualization();
                                dependencyGraph.AddChild(parent, versionBounds);
                                resolverQueue.Enqueue(parent);
                            }
                            catch (UnsatisfiableBoundsException)
                            {
                                // irrecoverable
                                throw;
                            }
                        }
                    }
                }
                catch (PackageNotAvailableException)
                {
                    // no source is known from which the package could be installed, irrecoverable
                    throw;
                }

            }

            return dependencyNodes.Values.Where(node => node.IsActualized).Select(node => node.ActualizedManifest!);
        }
    }
}