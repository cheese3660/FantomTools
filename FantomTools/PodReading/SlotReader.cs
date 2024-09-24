using FantomTools.Fantom;
using FantomTools.Utilities;

namespace FantomTools.PodReading;

internal class SlotReader
{
    public readonly string Name;
    public readonly Flags Flags;
    public AttributesReader? Attrs;

    protected SlotReader(FantomStreamReader reader)
    {
        Name = reader.ReadName();
        Flags = (Flags)reader.ReadU32();
    }

    protected void ReadAttrs(FantomStreamReader reader)
    {
        Attrs = AttributesReader.Read(reader);
    }
}