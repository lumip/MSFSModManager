// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;

namespace MSFSModManager.Core
{
    public class VersionBounds
    {
        public IVersionNumber Lower { get; private set; }
        public IVersionNumber Upper { get; private set; }

        public VersionBounds(IVersionNumber lower, IVersionNumber upper)
        {
            Lower = lower;
            Upper = upper;
            if (lower.CompareTo(upper) >= 0) throw new UnsatisfiableBoundsException(lower, upper);
        }

        // public VersionBounds(IVersionNumber version) : this(version, new VersionNumber(version.Major, version.Minor, version.Patch + 1)) { }

        public VersionBounds(IVersionNumber version) : this(version, version.Increment()) { }

        public static VersionBounds Unbounded = new VersionBounds(VersionNumber.Zero, VersionNumber.Infinite);

        public bool CheckVersion(IVersionNumber version)
        {
            return (version.CompareTo(Lower) >= 0) && (version.CompareTo(Upper) < 0);
        }

        public VersionBounds Combine(VersionBounds other)
        {
            try
            {
                return new VersionBounds(
                    VersionNumber.Max(Lower, other.Lower),
                    VersionNumber.Min(Upper, other.Upper)
                );
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("The bounds are mutually exclusive.");
            }
        }

        public override string ToString()
        {
            return $">={Lower}, <{Upper}";
        }
    }
}
