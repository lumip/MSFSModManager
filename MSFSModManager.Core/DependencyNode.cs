// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.Collections.Generic;

namespace MSFSModManager.Core
{
    /// <summary>
    /// A node in a package dependency graph.
    /// 
    /// The graph models dependencies, i.e., each node's children
    /// are packages that the node depends on. Each edge in the
    /// graph may be annotated with bounds for compatible versions.
    /// The version bounds for a node are the combination of
    /// those on the edges of the node's parents.
    /// </summary>
    public class DependencyNode
    {
        Dictionary<DependencyNode, VersionBounds> _parents;
        HashSet<DependencyNode> _children;

        public string PackageId { get; }

        public VersionBounds AdditionalBounds { get; set; }

        public DependencyNode(string packageId)
        {
            AdditionalBounds = VersionBounds.Unbounded;
            PackageId = packageId;
            _parents = new Dictionary<DependencyNode, VersionBounds>();
            _children = new HashSet<DependencyNode>();
            ActualizedManifest = null;
        }

        /// <summary>
        /// The version bounds for the package represented by the node.
        /// 
        /// These are determined as the combination from the bounds imposed
        /// by all incoming edges from the node's parents.
        /// </summary>
        /// <value></value>
        public VersionBounds VersionBounds {
            get
            {
                VersionBounds versionBounds = AdditionalBounds;
                foreach (var parent in _parents)
                {
                    versionBounds = versionBounds.Combine(parent.Value);
                }
                return versionBounds;
            }
        }

        public VersionNumber? ActualizedVersion => (ActualizedManifest != null) ? ActualizedManifest.Version : null;
        public PackageManifest? ActualizedManifest { get; private set; }

        public bool IsActualized => ActualizedManifest != null;

        /// <summary>
        /// Children of the node in the dependency graph.
        /// 
        /// These are the packages that the package represented by the node depends on.
        /// </summary>
        /// <returns></returns>
        public HashSet<DependencyNode> Children => new HashSet<DependencyNode>(_children);

        /// <summary>
        /// Parents of the node in the dependency graph.
        /// 
        /// These are the packages that depend on the package represented by this node.
        /// </summary>
        /// <typeparam name="DependencyNode"></typeparam>
        /// <returns></returns>
        public HashSet<DependencyNode> Parents => new HashSet<DependencyNode>(_parents.Keys);

        /// <summary>
        /// Actualizes ("instantiates") the node with a concrete package version compatible with the version bounds.
        /// </summary>
        /// <param name="manifest">The manifest of the package with which to actualize the node.</param>
        /// <exception cref="ArgumentException">Thrown if the given manifest is for a different package.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the package version in the given manifest does not satisfy the bounds.</exception>
        public void Actualize(PackageManifest manifest)
        {
            if (!manifest.Id.Equals(PackageId))
                throw new ArgumentException("Manifest package id does not correspond to node package id.", nameof(manifest));
            if (!VersionBounds.CheckVersion(manifest.Version))
                throw new ArgumentOutOfRangeException("The version of the package does not satisfy the bounds.", nameof(manifest));

            ActualizedManifest = manifest;
        }

        public void RemoveActualization()
        {
            ActualizedManifest = null;
            foreach (var child in _children)
            {
                child._parents.Remove(this);
                // note: do NOT use child.RemoveParent or this.RemoveChild here
                // since they modify _children; we clear that anyways below
            }
            _children.Clear();
        }

        /// <summary>
        /// Adds an edge to a dependency of this node in the dependency graph.
        /// </summary>
        /// <param name="child">Node representing a dependency of this node.</param>
        /// <param name="versionBounds">The bounds on versions of the dependency this node is compatible with.</param>
        public void AddChild(DependencyNode child, VersionBounds versionBounds)
        {
            _children.Add(child);

            if (child._parents.ContainsKey(this))
            {
                versionBounds = child._parents[this].Combine(versionBounds);
                child._parents.Remove(this);
            }
            child._parents.Add(this, versionBounds);
        }

        public void RemoveChild(DependencyNode child)
        {
            child._parents.Remove(this);
            _children.Remove(child);
        }

        public override bool Equals(object obj)
        {
            DependencyNode? node = obj as DependencyNode;
            if (node == null) return false;
            return node.PackageId.Equals(PackageId);
        }

        public override int GetHashCode()
        {
            return PackageId.GetHashCode();
        }

    }
    
}
