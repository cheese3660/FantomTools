using FantomTools.PodWriting;
using FantomTools.Utilities;

namespace FantomTools.Fantom.Attributes;

/// <summary>
/// This is a raw fantom attribute, having a byte array for its data
/// </summary>
/// <param name="name">The name of the attribute</param>
public class BufferedAttribute(string name) : FantomAttribute(name)
{

    /// <summary>
    /// The bytes attached to this attribute
    /// </summary>
    public byte[] Bytes = [];
    
    /// <summary>
    /// This is a reader for the memory buffer if this has an attached buffer
    /// </summary>
    public BigEndianReader Reader => new(new MemoryStream(Bytes), false);
    internal override void WriteBody(BigEndianWriter writer, FantomTables tables)
    {
        writer.WriteU16((ushort)Bytes.Length);
        writer.Stream.Write(Bytes);
    }
}