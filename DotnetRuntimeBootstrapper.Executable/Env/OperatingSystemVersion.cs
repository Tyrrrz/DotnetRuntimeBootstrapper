using System;

namespace DotnetRuntimeBootstrapper.Executable.Env
{
    internal readonly partial struct OperatingSystemVersion
    {
        public int Major { get; }

        public int Minor { get; }

        public OperatingSystemVersion(int major, int minor)
        {
            Major = major;
            Minor = minor;
        }
    }

    internal partial struct OperatingSystemVersion
    {
        public static OperatingSystemVersion Windows7 { get; } = new(6, 1);

        public static OperatingSystemVersion Windows8 { get; } = new(6, 2);

        public static OperatingSystemVersion Windows81 { get; } = new(6, 3);

        public static OperatingSystemVersion Windows10 { get; } = new(10, 0);
    }

    internal partial struct OperatingSystemVersion :
        IComparable<OperatingSystemVersion>,
        IEquatable<OperatingSystemVersion>
    {
        public int CompareTo(OperatingSystemVersion other)
        {
            var majorComparison = Major.CompareTo(other.Major);
            var minorComparison = Minor.CompareTo(other.Minor);

            return majorComparison != 0
                ? majorComparison
                : minorComparison;
        }

        public bool Equals(OperatingSystemVersion other) => CompareTo(other) == 0;

        public override bool Equals(object? obj) => obj is OperatingSystemVersion other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Major * 397) ^ Minor;
            }
        }

        public static bool operator ==(OperatingSystemVersion left, OperatingSystemVersion right) =>
            left.Equals(right);

        public static bool operator !=(OperatingSystemVersion left, OperatingSystemVersion right) =>
            !(left == right);

        public static bool operator >(OperatingSystemVersion left, OperatingSystemVersion right) =>
            left.CompareTo(right) > 0;

        public static bool operator <(OperatingSystemVersion left, OperatingSystemVersion right) =>
            left.CompareTo(right) < 0;

        public static bool operator >=(OperatingSystemVersion left, OperatingSystemVersion right) =>
            left == right || left > right;

        public static bool operator <=(OperatingSystemVersion left, OperatingSystemVersion right) =>
            left == right || left < right;
    }
}