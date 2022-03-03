// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

using MSFSModManager.Core.PackageSources;

namespace MSFSModManager.Core
{

    public static class DependencyResolver
    {

        private static Dictionary<string, DependencyNode> BuildDatabaseDependencyGraph(
            IPackageDatabase database,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            Dictionary<string, DependencyNode> nodes = new Dictionary<string, DependencyNode>();

            foreach (var package in database.Packages.Where(p => p.Manifest != null))
            {
                cancellationToken.ThrowIfCancellationRequested();

                DependencyNode node;
                if (!nodes.TryGetValue(package.Id, out node))
                {
                    node = new DependencyNode(package.Id);
                    nodes.Add(package.Id, node);
                }
                foreach (var dependency in package.Manifest!.Dependencies)
                {
                    DependencyNode dependencyNode;
                    if (!nodes.TryGetValue(dependency.Id, out dependencyNode))
                    {
                        dependencyNode = new DependencyNode(dependency.Id);
                        nodes.Add(dependency.Id, dependencyNode);
                    }
                    node.AddChild(dependencyNode, dependency.VersionBounds);
                }
                node.Actualize(package.Manifest);
            }

            return nodes;
        }

        /// <summary>
        /// Finds all currently installed packages that directly or indirectly depends on at least one of a collection of packages.
        /// </summary>
        /// <param name="ofPackages"></param>
        /// <param name="database"></param>
        /// <returns>
        /// An ordered enumerable of DependencyNode instances representing the given packages and all packages depending on them.
        /// The nodes are ordered such that all packages depending on a node will occur after it.
        /// </returns>
        public static IEnumerable<DependencyNode> FindDependentPackages(
            IEnumerable<string> ofPackages,
            IPackageDatabase database,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            Dictionary<string, DependencyNode> nodes = BuildDatabaseDependencyGraph(database, cancellationToken);

            // nodes now contains the full dependency graph of the database, where a parent-child connection
            // indicates that child depends on parent. We want to get rid of all nodes that do not represent parents
            // (dependants) of the packages in ofPackages.

            var orderedNodes = new List<DependencyNode>(); // keep track of the order in which nodes are processed
            var retainedNodes = new HashSet<DependencyNode>();
            Queue<DependencyNode> resolverQueue = new Queue<DependencyNode>();

            foreach (var packageId in ofPackages)
            {
                DependencyNode node;
                if (nodes.TryGetValue(packageId, out node))
                {
                    resolverQueue.Enqueue(node);
                }
            }

            while (resolverQueue.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var node = resolverQueue.Dequeue();
                if (retainedNodes.Add(node))
                {
                    foreach (var dependent in node.Parents)
                    {
                        orderedNodes.Add(dependent);
                        resolverQueue.Enqueue(dependent);
                    }
                }
            }

            // At this point, we have all dependent nodes in retainedNodes but they may still have children (i.e., dependencies)
            // outside of that set. In the next step we clear those up.
            foreach (var node in retainedNodes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var child in node.Children.Where(c => !retainedNodes.Contains(c)))
                {
                    node.RemoveChild(child);
                }
            }

            // Finally, we want to linearly order the nodes by the dependency relation (such that each node in the order can only
            // depend on succeeding nodes, or, equivalently, each nodes dependants occur before it).
            // This allows uninstallation to simply step through the lists, removing the packages corresponding to the nodes in order without.
            // In orderedNodes we currently have a list of all nodes in the order they were processed in the queue (potentially with duplicates).
            // This means that the last occurence of a node in this list is the point when it was last encountered as a parent of any node
            // in retainedNodes. Therefore, reversing orderedNodes and keeping only each first occurence of each node will give us the order we want.

            // this assumes that Distinct() always keeps the first occurence of duplicated elements. This is currently the case but no API guarantee.
            return orderedNodes.AsEnumerable().Reverse().Distinct();
        }

        /// <summary>
        /// Resolves required dependencies for a collection of packages that are about to be installed.
        /// 
        /// Will prefer to resolve a dependency with an already installed package, if it meets the version bound.
        /// Otherwise, will search through package sources to obtain a compatible version of the dependency.
        /// </summary>
        /// <param name="installationCandidates">Enumerable of packages for which to resolve dependencies.</param>
        /// <param name="packageSources">Repository of sources from which to search for available dependency packages.</param>
        /// <param name="gameVersion">Version number of the main game.</param>
        /// <param name="monitor">An optional progress monitor.</param>
        /// <returns>Enumerable of dependencies for the given installation candidates.</returns>
        public static async Task<IEnumerable<PackageManifest>> ResolveDependencies(
            IEnumerable<PackageDependency> installationCandidates, 
            IPackageSourceRepository packageSources,
            IVersionNumber gameVersion,
            IProgressMonitor? monitor = null,
            CancellationToken cancellationToken = default(CancellationToken)
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
                cancellationToken.ThrowIfCancellationRequested();
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
                        node.VersionBounds, gameVersion, monitor, cancellationToken
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
                            await packageSources.GetSource(node.PackageId).GetPackageManifest(node.VersionBounds, gameVersion, monitor, cancellationToken);
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