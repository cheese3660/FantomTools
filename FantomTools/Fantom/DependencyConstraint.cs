namespace FantomTools.Fantom;

public record struct DependencyConstraint(Version Version, Version? EndVersion, bool IsPlus=false)
{
    public DependencyConstraint(Version version, bool IsPlus = false) : this(version, null, IsPlus) { }

    public override string ToString() =>
        EndVersion == null ? IsPlus ? $"{Version}+" : Version.ToString() : $"{Version}-{EndVersion}";
}