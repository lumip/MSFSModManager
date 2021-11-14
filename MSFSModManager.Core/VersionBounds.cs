using System;

namespace MSFSModManager.Core
{

    public class UnsatisfiableBoundsException : Exception
    {
        public UnsatisfiableBoundsException(IVersionNumber lower, IVersionNumber upper)
            : base($"Version bounds unsatisfiable (>={lower}, < {upper}.")
        { }
    }
    
    public class VersionBounds
    {
        public IVersionNumber Lower { get; private set; }
        public IVersionNumber Upper { get; private set; }

        public VersionBounds(IVersionNumber lower, IVersionNumber upper)
        {
            Lower = lower;
            Upper = upper;
            if (lower.CompareTo(upper) > 0) throw new UnsatisfiableBoundsException(lower, upper);
        }

        public VersionBounds(VersionNumber version) : this(version, new VersionNumber(version.Major, version.Minor, version.Patch + 1)) { }

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