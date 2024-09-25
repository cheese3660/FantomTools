using FantomTools.PodWriting;
using FantomTools.Utilities;

namespace FantomTools.Fantom.Attributes;

/// <summary>
/// 
/// </summary>
public class ErrorTableAttribute() : FantomAttribute("ErrTable")
{
    internal override void WriteBody(FantomStreamWriter writer, FantomTables tables)
    {
        
        using var dataStream = new MemoryStream();
        var dataWriter = new FantomStreamWriter(dataStream);
        // So now the information goes in here
        dataWriter.WriteU16((ushort)Entries.Count);
        foreach (var entry in Entries)
        {
            dataWriter.WriteU16(entry.TryStart);
            dataWriter.WriteU16(entry.TryEnd);
            dataWriter.WriteU16(entry.Handler);
            dataWriter.WriteU16(tables.TypeReferences.Intern(entry.ErrorType));
        }
        writer.WriteU16((ushort)dataStream.Length);
        writer.Stream.Write(dataStream.ToArray());
    }

    public List<ErrorTableEntry> Entries = [];

    internal ErrorTableAttribute(FantomStreamReader reader) : this()
    {
        var count = reader.ReadU16();
        Entries.EnsureCapacity(count);
        for (var i = 0; i < count; i++)
        {
            Entries.Add(new ErrorTableEntry(reader.ReadU16(), reader.ReadU16(), reader.ReadU16(),
                reader.PodReader.TypeRefs[reader.ReadU16()].Reference));
        }
    }
}

public readonly record struct ErrorTableEntry(ushort TryStart, ushort TryEnd, ushort Handler, TypeReference ErrorType);