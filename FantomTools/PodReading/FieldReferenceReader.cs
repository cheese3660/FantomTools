using FantomTools.Fantom;
using FantomTools.Utilities;

namespace FantomTools.PodReading;

internal class FieldReferenceReader(TypeReferenceReader parent, string name, TypeReferenceReader type)
{
    public TypeReferenceReader Parent => parent;
    public string Name => name;
    public TypeReferenceReader Type => type;

    public static FieldReferenceReader Read(FantomStreamReader reader)
    {
        var parent = reader.PodReader.TypeRefs[reader.ReadU16()];
        var name = reader.ReadName();
        var type = reader.PodReader.TypeRefs[reader.ReadU16()];
        return new FieldReferenceReader(parent, name, type);
        
    }

    // Note, the field type is necessary for reconstructing the program, but it might be able to be omitted for known types
    public override string ToString() => $"({type}){parent}.{name}";

    public FieldReference Reference => new(parent.Reference, name, type.Reference);
}