using System;
using System.Collections.Generic;

namespace MSFSModManager.Core
{
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

        public HashSet<DependencyNode> Children => new HashSet<DependencyNode>(_children);
        public HashSet<DependencyNode> Parents => new HashSet<DependencyNode>(_parents.Keys);

        public void Actualize(PackageManifest manifest)
        {
            if (!VersionBounds.CheckVersion(manifest.Version)) throw new ArgumentOutOfRangeException(nameof(manifest));

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
