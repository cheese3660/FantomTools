using FantomTools.InternalUtilities;
using FantomTools.PodWriting;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom.Attributes;

/// <summary>
/// This attributes denotes the line number of a slot
/// </summary>
/// <param name="lineNumber">The line number of the slot</param>
[PublicAPI]
public class LineNumberAttribute(ushort lineNumber) : FantomAttribute("LineNumber")
{
    /// <summary>
    /// The line number of the slot
    /// </summary>
    public ushort LineNumber = lineNumber;

    internal override void WriteBody(BigEndianWriter writer, FantomTables tables)
    {
        using var dataWriter = new FantomBufferStream(writer.Stream);
        dataWriter.WriteU16(LineNumber);
    }
}