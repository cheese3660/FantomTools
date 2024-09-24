using FantomTools.PodWriting;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom.Attributes;

[PublicAPI]
public class FantomAttribute(string name)
{
    public string Name => name;
    internal FantomBuffer? Buffer;
    public byte[]? Bytes => Buffer?.Buffer;


    internal static FantomAttribute Parse(FantomStreamReader reader)
    {
        var name = reader.ReadName();
        switch (name)
        {
            case "Facets":
                return new FacetsAttribute(name, reader);
            case "LineNumber":
                reader.ReadU16();
                return new LineNumberAttribute(reader.ReadU16());
            case "SourceFile":
                reader.ReadU16();
                return new SourceFileAttribute(reader.ReadUtf8());
            default:
                return new FantomAttribute(name)
                {
                    Buffer = FantomBuffer.Read(reader)
                };
        }
    }


    internal void Write(FantomStreamWriter writer, FantomTables tables)
    {
        writer.WriteU16(tables.Names.Intern(Name));
        WriteBody(writer, tables);
    }
    internal virtual void WriteBody(FantomStreamWriter writer, FantomTables tables)
    {
        if (Buffer != null)
        {
            Buffer.WriteBuffer(writer);
        }
        else
        {
            writer.WriteU16(0);
        }
    }
}