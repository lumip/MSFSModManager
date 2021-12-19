// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;

namespace MSFSModManager.Core.PackageSources
{
    class MalformedArchiveException : Exception
    {
        public MalformedArchiveException()
            : base("Malformed package archive. File is not an archive or does not contain package manifest.")
        {

        }

    }
}
