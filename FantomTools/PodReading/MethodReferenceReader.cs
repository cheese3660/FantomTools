using FantomTools.Fantom;
using FantomTools.InternalUtilities;
using FantomTools.Utilities;

namespace FantomTools.PodReading;

internal class MethodReferenceReader(TypeReferenceReader parent, string name, TypeReferenceReader returnType, List<TypeReferenceReader> parameters)
{
    public static MethodReferenceReader Read(FantomStreamReader reader)
    {
        var parent = reader.PodReader.TypeRefs[reader.ReadU16()];
        var name = reader.ReadName();
        var ret = reader.PodReader.TypeRefs[reader.ReadU16()];
        var numParams = reader.ReadU8();
        var parameters = new List<TypeReferenceReader>(numParams);
        for (var i = 0; i < numParams; i++)
        {
            parameters.Add(reader.PodReader.TypeRefs[reader.ReadU16()]);
        }

        return new MethodReferenceReader(parent, name, ret, parameters);
    }

    public override string ToString() => $"{parent}.{name}({string.Join(",", parameters)}) -> {returnType}";

    public MethodReference Reference => new(parent.Reference, name, returnType.Reference,
        parameters.Select(x => x.Reference).ToArray());
}