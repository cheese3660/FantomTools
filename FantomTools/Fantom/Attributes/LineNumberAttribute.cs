using FantomTools.PodWriting;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom.Attributes;

[PublicAPI]
public class LineNumberAttribute(ushort lineNumber) : FantomAttribute("LineNumber")
{
    public ushort LineNumber = lineNumber;

    internal override void WriteBody(FantomStreamWriter writer, FantomTables tables)
    {
        writer.WriteU16(2);
        writer.WriteU16(LineNumber);
    }
}