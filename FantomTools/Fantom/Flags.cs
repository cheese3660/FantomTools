using System.Text;
using JetBrains.Annotations;

namespace FantomTools.Fantom;

/// <summary>
/// Represents the flags that go on types/slots on a type
/// </summary>
[Flags,PublicAPI]
public enum Flags
{
    /// <summary>
    /// <para>For: <see cref="Type"/>, <see cref="Method"/>, <see cref="Field"/></para>
    /// <para>Is this abstract, i.e. does it need to be overloaded?</para>
    /// </summary>
    Abstract  = 0x00000001,
    /// <summary>
    /// <para>For: <see cref="Type"/></para>
    /// <para>Are instances of this type immutable? (https://fantom.org/doc/docLang/Classes#const)</para>
    /// <para>For: <see cref="Field"/></para>
    /// <para>Is this field constant? (https://fantom.org/doc/docLang/Fields#const)</para>
    /// </summary>
    Const     = 0x00000002,
    /// <summary>
    /// <para>For: <see cref="Method"/></para>
    /// <para>Is this method a constructor?</para>
    /// </summary>
    Ctor      = 0x00000004,
    /// <summary>
    /// <para>For: <see cref="Type"/></para>
    /// <para>Is this type an enum? (https://fantom.org/doc/docLang/Enums)</para>
    /// </summary>
    Enum      = 0x00000008,
    /// <summary>
    /// <para>For: <see cref="Type"/></para>
    /// <para>Is this type a facet? (https://fantom.org/doc/docLang/Facets)</para>
    /// </summary>
    Facet     = 0x00000010,
    /// <summary>
    /// <para>For: <see cref="Type"/></para>
    /// <para>Is this type unable to be subclassed?</para>
    /// <para>For: <see cref="Method"/></para>
    /// <para>Is this method non-overridable in subclasses?</para>
    /// </summary>
    Final     = 0x00000020,
    /// <summary>
    /// <para>For: <see cref="Method"/></para>
    /// <para>Is the method a field getter?</para>
    /// </summary>
    Getter    = 0x00000040,
    /// <summary>
    /// <para>For: <see cref="Type"/>, <see cref="Method"/>, <see cref="Field"/></para>
    /// <para>Is this only visible within the pod?</para>
    /// </summary>
    Internal  = 0x00000080,
    /// <summary>
    /// <para>For: <see cref="Type"/></para>
    /// <para>Is this type a mixin? (https://fantom.org/doc/docLang/Mixins)</para>
    /// </summary>
    Mixin     = 0x00000100,
    /// <summary>
    /// <para>For: <see cref="Method"/>, <see cref="Field"/></para>
    /// <para>Is this implemented in the native runtime?</para>
    /// </summary>
    Native    = 0x00000200,
    /// <summary>
    /// <para>For: <see cref="Method"/>, <see cref="Field"/></para>
    /// <para>Does this override another slot in a parent class?</para>
    /// </summary>
    Override  = 0x00000400,
    /// <summary>
    /// <para>For: <see cref="Method"/>, <see cref="Field"/></para>
    /// <para>Is this slot only visible within the type?</para>
    /// </summary>
    Private   = 0x00000800,
    /// <summary>
    /// <para>For: <see cref="Method"/>, <see cref="Field"/></para>
    /// <para>Is this slot only visible within the type or subtypes?</para>
    /// </summary>
    Protected = 0x00001000,
    /// <summary>
    /// <para>For: <see cref="Type"/>, <see cref="Method"/>, <see cref="Field"/></para>
    /// <para>Is this visible everywhere?</para>
    /// </summary>
    Public    = 0x00002000,
    /// <summary>
    /// <para>For: <see cref="Method"/></para>
    /// <para>Is the method a field setter?</para>
    /// </summary>
    Setter    = 0x00004000,
    /// <summary>
    /// <para>For: <see cref="Method"/>, <see cref="Field"/></para>
    /// <para>Is this slot attached to the type rather than instances of the type?</para>
    /// </summary>
    Static    = 0x00008000,
    /// <summary>
    /// <para>For: <see cref="Method"/>, <see cref="Field"/></para>
    /// <para>Is this slot attached to the type rather than instances of the type?</para>
    /// </summary>
    Storage   = 0x00010000,
    /// <summary>
    /// <para>For: <see cref="Type"/>, <see cref="Method"/>, <see cref="Field"/></para>
    /// <para>Is this compiler generated?</para>
    /// </summary>
    Synthetic = 0x00020000,
    /// <summary>
    /// <para>For: <see cref="Method"/>, <see cref="Field"/></para>
    /// <para>Can this slot be overridden?</para>
    /// </summary>
    Virtual   = 0x00040000,
    /// <summary>
    /// <para>For: <see cref="Method"/></para>
    /// <para>Does this method cache its value</para>
    /// <para>For: <see cref="Field"/></para>
    /// <para>Is this field the cache field of a once method</para>
    /// </summary>
    Once      = 0x00080000
}

internal static class FlagsExtensions
{
    internal static string GetString(this Flags flags)
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