using FantomTools.InternalUtilities;
using FantomTools.Utilities;

namespace FantomTools.PodReading;

internal class FieldReader : SlotReader
{
    public FieldReader(FantomStreamReader reader) : base(reader)
    {
        Type = reader.PodReader.TypeRefs[reader.ReadU16()];
        ReadAttrs(reader);
    }

    public readonly TypeReferenceReader Type;
}