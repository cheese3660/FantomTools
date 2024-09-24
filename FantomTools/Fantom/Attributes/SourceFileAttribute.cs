using FantomTools.PodWriting;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom.Attributes;

[PublicAPI]
public class SourceFileAttribute(string sourceFile) : FantomAttribute("SourceFile")
{
    public string SourceFile = sourceFile;
    internal override void WriteBody(FantomStreamWriter writer, FantomTables tables)
    {
        using var dataStream = new MemoryStream();
        var dataWriter = new FantomStreamWriter(dataStream);
        dataWriter.WriteUtf8(SourceFile);
        writer.WriteU16((ushort)dataStream.Length);
        writer.Stream.Write(dataStream.ToArray());
    }
}