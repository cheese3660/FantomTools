using FantomTools.Fantom;
using FantomTools.Utilities;

namespace FantomTools.PodReading;

internal sealed class TypeReferenceReader(string podName, string typeName, string signature)
{
    internal string TypeName => typeName;
    public override string ToString() => signature.Length <= 1 ? $"{podName}::{typeName}{signature}" : signature;
    
    public static TypeReferenceReader Read(FantomStreamReader streamReader)
    {
        var podName = streamReader.ReadName();
        var typeName = streamReader.ReadName();
        var signature = streamReader.ReadUtf8();
        return new TypeReferenceReader(podName, typeName, signature);
    }

    public TypeReference Reference => TypeReference.Parse(ToString());
}