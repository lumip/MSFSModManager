// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;

namespace MSFSModManager.Core
{

    public class UnsatisfiableBoundsException : Exception
    {
        public UnsatisfiableBoundsException(IVersionNumber lower, IVersionNumber upper)
            : base($"Version bounds unsatisfiable (>={lower}, < {upper}.")
        { }
    }    
}
