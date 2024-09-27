using FantomTools.InternalUtilities;
using FantomTools.PodWriting;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom.Attributes;

/// <summary>
/// This attribute represents the source file of a type
/// </summary>
/// <param name="sourceFile">The source file of the type</param>
[PublicAPI]
public class SourceFileAttribute(string sourceFile) : FantomAttribute("SourceFile")
{
    /// <summary>
    /// The source file of the type
    /// </summary>
    public string SourceFile = sourceFile;
    internal override void WriteBody(BigEndianWriter writer, FantomTables tables)
    {
        using var dataWriter = new FantomBufferStream(writer.Stream);
        dataWriter.WriteUtf8(SourceFile);
    }
}