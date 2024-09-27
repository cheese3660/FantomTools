using FantomTools.InternalUtilities;
using FantomTools.PodWriting;
using FantomTools.Utilities;

namespace FantomTools.Fantom.Attributes;

/// <summary>
/// This contains the error handling table for a method
/// You should not modify this directly, instead use the ErrorTable in the <see cref="FantomTools.Fantom.Code.MethodBody"/> class
/// </summary>
public class ErrorTableAttribute() : FantomAttribute("ErrTable")
{
    internal override void WriteBody(BigEndianWriter writer, FantomTables tables)
    {
        // This should automatically write the buffer on dispose
        using var dataWriter = new FantomBufferStream(writer.Stream);
        dataWriter.WriteU16((ushort)Entries.Count);
        foreach (var entry in Entries)
        {
            dataWriter.WriteU16(entry.TryStart);
            dataWriter.WriteU16(entry.TryEnd);
            dataWriter.WriteU16(entry.Handler);
            dataWriter.WriteU16(tables.TypeReferences.Intern(entry.ErrorType));
        }
    }

    /// <summary>
    /// This is the list of raw error table entries, modifying this is unadvised as it will get overwritten when dumping
    /// a method
    /// </summary>
    public readonly List<ErrorTableEntry> Entries = [];

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

/// <summary>
/// This is a raw error table entry
/// </summary>
/// <param name="TryStart">The offset for the start of the protected region</param>
/// <param name="TryEnd">The offset for the end of the protected region</param>
/// <param name="Handler">The offset of the catch instruction</param>
/// <param name="ErrorType">The type that this error handler handles</param>
public readonly record struct ErrorTableEntry(ushort TryStart, ushort TryEnd, ushort Handler, TypeReference ErrorType);