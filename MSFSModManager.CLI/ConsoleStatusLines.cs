using System;
using System.Collections.Generic;

using MSFSModManager.Core;

namespace MSFSModManager.CLI
{
    class ConsoleStatusLines
    {
        private struct Key
        {
            string PackageId;
            VersionNumber Version;

            public Key(string packageId, VersionNumber version)
            {
                PackageId = packageId;
                Version = version;
            }
        }

        private Dictionary<Key, ConsoleRenderer.LineHandle> _lines;
        private ConsoleRenderer _renderer;

        public ConsoleStatusLines(ConsoleRenderer renderer)
        {
            _lines = new Dictionary<Key, ConsoleRenderer.LineHandle>();
            _renderer = renderer;
        }

        public ConsoleRenderer.LineHandle GetLineHandle(string packageId, VersionNumber version)
        {
            Key k = new Key(packageId, version);
            if (!_lines.ContainsKey(k)) _lines.Add(k, _renderer.MakeNewLineHandle());
            return _lines[k];
        }
    }
}