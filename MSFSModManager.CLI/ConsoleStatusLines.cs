// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.Collections.Generic;

using MSFSModManager.Core;

namespace MSFSModManager.CLI
{
    class ConsoleStatusLines
    {
        // private struct Key
        // {
        //     string PackageId;
        //     IVersionNumber Version;

        //     public Key(string packageId, IVersionNumber version)
        //     {
        //         PackageId = packageId;
        //         Version = version;
        //     }
        // }

        private Dictionary<string, ConsoleRenderer.LineHandle> _lines;
        private ConsoleRenderer _renderer;

        public ConsoleStatusLines(ConsoleRenderer renderer)
        {
            _lines = new Dictionary<string, ConsoleRenderer.LineHandle>();
            _renderer = renderer;
        }

        public ConsoleRenderer.LineHandle GetLineHandle(string packageId)
        {
            string k = packageId;
            if (!_lines.ContainsKey(k)) _lines.Add(k, _renderer.MakeNewLineHandle());
            return _lines[k];
        }
    }
}