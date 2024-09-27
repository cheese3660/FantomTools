using FantomTools.InternalUtilities;
using FantomTools.Utilities;

namespace FantomTools.PodReading;

internal class MethodVariableReader(string name, TypeReferenceReader type, int flags, AttributesReader attrs)
{
    public string Name => name;
    public TypeReferenceReader Type => type;
    public bool IsParam => (flags & 1) == 1;
    public AttributesReader Attrs => attrs;

    public static MethodVariableReader Read(FantomStreamReader reader)
    {
        var name = reader.ReadName();
        var type = reader.PodReader.TypeRefs[reader.ReadU16()];
        var flags = reader.ReadU8();
        var attrs = AttributesReader.Read(reader);
        return new MethodVariableReader(name, type, flags,attrs);
    }
    
}