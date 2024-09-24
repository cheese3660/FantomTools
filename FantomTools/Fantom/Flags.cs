using System.Text;

namespace FantomTools.Fantom;

[Flags]
public enum Flags
{
    Abstract  = 0x00000001,
    Const     = 0x00000002,
    Ctor      = 0x00000004,
    Enum      = 0x00000008,
    Facet     = 0x00000010,
    Final     = 0x00000020,
    Getter    = 0x00000040,
    Internal  = 0x00000080,
    Mixin     = 0x00000100,
    Native    = 0x00000200,
    Override  = 0x00000400,
    Private   = 0x00000800,
    Protected = 0x00001000,
    Public    = 0x00002000,
    Setter    = 0x00004000,
    Static    = 0x00008000,
    Storage   = 0x00010000,
    Synthetic = 0x00020000,
    Virtual   = 0x00040000,
    Once      = 0x00080000
}

public static class FlagsExtensions
{
    public static string GetString(this Flags flags)
    {
        StringBuilder sb = new();
        if (flags.HasFlag(Flags.Public)) sb.Append("public ");
        if (flags.HasFlag(Flags.Protected)) sb.Append("protected ");
        if (flags.HasFlag(Flags.Private)) sb.Append("private ");
        if (flags.HasFlag(Flags.Internal)) sb.Append("internal ");
        if (flags.HasFlag(Flags.Native)) sb.Append("native ");
        if (flags.HasFlag(Flags.Enum)) sb.Append("enum ");
        if (flags.HasFlag(Flags.Mixin)) sb.Append("mixin ");
        if (flags.HasFlag(Flags.Final)) sb.Append("final ");
        if (flags.HasFlag(Flags.Ctor)) sb.Append("new ");
        if (flags.HasFlag(Flags.Override)) sb.Append("override ");
        if (flags.HasFlag(Flags.Abstract)) sb.Append("abstract ");
        if (flags.HasFlag(Flags.Static)) sb.Append("static ");
        if (flags.HasFlag(Flags.Virtual)) sb.Append("virtual ");

        return sb.ToString();
    }
}