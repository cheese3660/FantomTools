using FantomTools.Fantom.Attributes;
using FantomTools.PodWriting;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom;

[PublicAPI]
public class MethodVariable
{
    public ushort Index;
    public string Name = "";
    public TypeReference Type = TypeReference.Object;
    public bool IsParameter;
    public List<FantomAttribute> Attributes = [];

    public void Emit(FantomStreamWriter writer, FantomTables tables)
    {
        writer.WriteU16(tables.Names.Intern(Name));
        writer.WriteU16(tables.TypeReferences.Intern(Type));
        writer.WriteU8((byte)(IsParameter ? 1 : 0));
        writer.WriteU16((ushort)Attributes.Count);
        foreach (var attr in Attributes)
        {
            attr.Write(writer, tables);
        }
    }
}