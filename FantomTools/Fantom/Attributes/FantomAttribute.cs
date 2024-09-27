using FantomTools.InternalUtilities;
using FantomTools.PodWriting;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom.Attributes;

/// <summary>
/// The base class for a fantom attribute
/// </summary>
/// <param name="name">The name of the attribute</param>
[PublicAPI]
public abstract class FantomAttribute(string name)
{
    /// <summary>
    /// The name of the attribute
    /// </summary>
    public string Name => name;
    
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
            case "ErrTable":
                reader.ReadU16();
                return new ErrorTableAttribute(reader);
            default:
                return new BufferedAttribute(name)
                {
                    Bytes = FantomBuffer.Read(reader)?.Buffer ?? []
                };
        }
    }


    internal void Write(BigEndianWriter writer, FantomTables tables)
    {
        writer.WriteU16(tables.Names.Intern(Name));
        WriteBody(writer, tables);
    }

    internal abstract void WriteBody(BigEndianWriter writer, FantomTables tables);
}