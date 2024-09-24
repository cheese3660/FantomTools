using JetBrains.Annotations;

namespace FantomTools.Fantom;

/// <summary>
/// Represents a constraint on a pods dependency
/// </summary>
/// <param name="Version">The minimum version of the dependency</param>
/// <param name="EndVersion">The end version of the dependency if this is a dependency range</param>
/// <param name="IsUnconstrained">Is the dependency is an unconstrained range from the minimum version?</param>
[PublicAPI]
public readonly record struct DependencyConstraint(Version Version, Version? EndVersion, bool IsUnconstrained=false)
{
    /// <summary>
    /// Creates a dependency with the end version being set to null
    /// </summary>
    /// <param name="version">The minimum version of the dependency</param>
    /// <param name="IsUnconstrained">Is the dependency is an unconstrained range from the minimum version?</param>
    public DependencyConstraint(Version version, bool IsUnconstrained = false) : this(version, null, IsUnconstrained) { }

    /// <summary>
    /// Returns the expected meta.props representation of the constraint
    /// </summary>
    /// <returns>the expected meta.props representation of the constraint</returns>
    public override string ToString() =>
        EndVersion == null ? IsUnconstrained ? $"{Version}+" : Version.ToString() : $"{Version}-{EndVersion}";

    /// <summary>
    /// True if this is a constraint that can be satisfied by only 1 version
    /// </summary>
    public bool IsSingleVersion => EndVersion == null && !IsUnconstrained;
}